// Bridge for the translator page: it sends subtitle lines to the overlay and controls it.
const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('desktop', {
  sendSubtitles: (data) => ipcRenderer.send('subtitles', data),
  toggleOverlay: () => ipcRenderer.send('overlay-toggle'),
  toggleLock: () => ipcRenderer.send('overlay-lock'),
  onOverlayState: (cb) => ipcRenderer.on('overlay-state', (_e, state) => cb(state))
});
