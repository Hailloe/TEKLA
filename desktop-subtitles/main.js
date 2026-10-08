// Desktop subtitle app: the live translator page in a normal window, plus a transparent,
// always-on-top, click-through subtitle overlay that floats over any program.
const {
  app, BrowserWindow, protocol, net, session, desktopCapturer, globalShortcut, ipcMain, screen, shell
} = require('electron');
const path = require('node:path');
const fs = require('node:fs');
const { pathToFileURL } = require('node:url');

// app://local/... serves the bundled web page (web/) and the overlay page as a secure origin,
// so microphone, screen-audio capture and fetch() behave like on https.
protocol.registerSchemesAsPrivileged([
  { scheme: 'app', privileges: { standard: true, secure: true, supportFetchAPI: true, corsEnabled: true } }
]);

const WEB_DIR = path.join(__dirname, 'web');
const OVERLAY_FILES = new Set(['overlay.html']);
const LOCK_KEY = 'CommandOrControl+Shift+L';
const HIDE_KEY = 'CommandOrControl+Shift+H';

let mainWin = null;
let overlay = null;
let locked = true; // click-through; unlocked = can be dragged and resized
let overlayWanted = true;

const boundsFile = () => path.join(app.getPath('userData'), 'overlay-bounds.json');
function loadBounds() {
  try {
    const b = JSON.parse(fs.readFileSync(boundsFile(), 'utf8'));
    // Ignore bounds from a monitor that is no longer attached.
    const area = screen.getDisplayMatching(b).workArea;
    const visible = b.x < area.x + area.width && b.x + b.width > area.x && b.y < area.y + area.height && b.y + b.height > area.y;
    return visible ? b : null;
  } catch {
    return null;
  }
}
function saveBounds() {
  try { fs.writeFileSync(boundsFile(), JSON.stringify(overlay.getBounds())); } catch {}
}

function serveFile(request) {
  const rel = decodeURIComponent(new URL(request.url).pathname).replace(/^\/+/, '') || 'index.html';
  const base = OVERLAY_FILES.has(rel) ? __dirname : WEB_DIR;
  const full = path.normalize(path.join(base, rel));
  if (!full.startsWith(base + path.sep)) return new Response('Not found', { status: 404 });
  return net.fetch(pathToFileURL(full).toString());
}

function setLocked(value) {
  locked = value;
  overlay.setIgnoreMouseEvents(locked);
  overlay.setFocusable(!locked);
  overlay.webContents.send('overlay-mode', { locked });
  mainWin?.webContents.send('overlay-state', { visible: overlay.isVisible(), locked });
}

function setVisible(value) {
  overlayWanted = value;
  if (value) overlay.showInactive(); else overlay.hide();
  mainWin?.webContents.send('overlay-state', { visible: value, locked });
}

function createOverlay() {
  const { workArea } = screen.getPrimaryDisplay();
  const width = Math.round(workArea.width * 0.7), height = 170;
  overlay = new BrowserWindow({
    x: workArea.x + Math.round((workArea.width - width) / 2),
    y: workArea.y + workArea.height - height - 40,
    width, height, minWidth: 300, minHeight: 80,
    ...loadBounds(),
    transparent: true, frame: false, resizable: true, hasShadow: false,
    alwaysOnTop: true, skipTaskbar: true, focusable: false, show: false,
    webPreferences: { preload: path.join(__dirname, 'overlay-preload.js') }
  });
  // 'screen-saver' keeps it above full-screen video and meeting windows.
  overlay.setAlwaysOnTop(true, 'screen-saver');
  overlay.setIgnoreMouseEvents(true);
  overlay.loadURL('app://local/overlay.html');
  // Transparent windows may never emit 'ready-to-show' on some platforms; page load is reliable.
  overlay.webContents.once('did-finish-load', () => {
    overlay.webContents.send('overlay-mode', { locked });
    if (overlayWanted) overlay.showInactive();
  });
  overlay.on('moved', saveBounds);
  overlay.on('resized', saveBounds);
}

function createMain() {
  mainWin = new BrowserWindow({
    width: 1000, height: 760, title: '实时翻译字幕',
    webPreferences: { preload: path.join(__dirname, 'main-preload.js') }
  });
  mainWin.setMenuBarVisibility(false);
  mainWin.loadURL('app://local/index.html');
  // Links such as "apply for a key" open in the normal browser.
  mainWin.webContents.setWindowOpenHandler(({ url }) => {
    if (/^https:\/\//.test(url)) shell.openExternal(url);
    return { action: 'deny' };
  });
  mainWin.on('closed', () => { mainWin = null; app.quit(); });
}

app.whenReady().then(() => {
  protocol.handle('app', serveFile);

  const ses = session.defaultSession;
  ses.setPermissionRequestHandler((_wc, permission, cb) => cb(['media', 'display-capture'].includes(permission)));
  // "电脑声音": capture the whole system's audio (Windows loopback) without a picker.
  ses.setDisplayMediaRequestHandler((_req, cb) => {
    desktopCapturer.getSources({ types: ['screen'] })
      .then((sources) => cb({ video: sources[0], audio: 'loopback' }))
      .catch(() => cb({}));
  });

  createMain();
  createOverlay();

  ipcMain.on('subtitles', (_e, data) => overlay?.webContents.send('subtitles', data));
  ipcMain.on('overlay-toggle', () => setVisible(!overlayWanted));
  ipcMain.on('overlay-lock', () => setLocked(!locked));

  globalShortcut.register(LOCK_KEY, () => setLocked(!locked));
  globalShortcut.register(HIDE_KEY, () => setVisible(!overlayWanted));
});

app.on('will-quit', () => globalShortcut.unregisterAll());
app.on('window-all-closed', () => app.quit());
