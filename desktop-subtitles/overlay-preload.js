const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('overlay', {
  onSubtitles: (cb) => ipcRenderer.on('subtitles', (_e, data) => cb(data)),
  onMode: (cb) => ipcRenderer.on('overlay-mode', (_e, mode) => cb(mode))
});
