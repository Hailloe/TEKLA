// Bundles the web translator (../meeting-translator) into web/ before running or packaging.
const fs = require('node:fs');
const path = require('node:path');

const src = path.join(__dirname, '..', 'meeting-translator');
const dst = path.join(__dirname, 'web');
fs.rmSync(dst, { recursive: true, force: true });
fs.cpSync(src, dst, { recursive: true, filter: (p) => !p.endsWith('.md') });
console.log('copied', src, '->', dst);
