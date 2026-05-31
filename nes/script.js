const SVG_W = 612.002, SVG_H = 251.924;

const BTNS = {
	'btn-b': {
		type: 'circle',
		cx: 430.15,
		cy: 178.63,
		r: 28.40
	},
	'btn-a': {
		type: 'circle',
		cx: 508.84,
		cy: 178.63,
		r: 28.40
	},
	'btn-select': {
		type: 'pill',
		cx: 240.38,
		cy: 179.94,
		w: 48.52,
		h: 19.12
	},
	'btn-start': {
		type: 'pill',
		cx: 324.37,
		cy: 179.94,
		w: 48.52,
		h: 19.12
	},
	'dpad-up': {
		type: 'rect',
		cx: 101.21,
		cy: 112.00,
		w: 36.76,
		h: 36.00
	},
	'dpad-down': {
		type: 'rect',
		cx: 101.21,
		cy: 186.80,
		w: 36.76,
		h: 38.46
	},
	'dpad-left': {
		type: 'rect',
		cx: 63.50,
		cy: 149.09,
		w: 38.65,
		h: 36.95
	},
	'dpad-right': {
		type: 'rect',
		cx: 138.92,
		cy: 149.09,
		w: 38.65,
		h: 36.95
	},
};

for (const [id, b] of Object.entries(BTNS)) b.el = document.getElementById(id);
const svg = document.getElementById('nes-svg');
const dbg = document.getElementById('dbg');

function positionAll() {
	const rect = svg.getBoundingClientRect();
	const sx = rect.width  / SVG_W;
	const sy = rect.height / SVG_H;
	for (const b of Object.values(BTNS)) {
		const el = b.el;
		if (b.type === 'circle') {
			const r = b.r * Math.min(sx, sy);
			el.style.cssText += `;width:${r*2}px;height:${r*2}px;left:${b.cx*sx-r}px;top:${b.cy*sy-r}px`;
		} else {
			const w = b.w*sx, h = b.h*sy;
			el.style.cssText += `;width:${w}px;height:${h}px;left:${b.cx*sx-w/2}px;top:${b.cy*sy-h/2}px`;
			if (b.type === 'pill') el.style.borderRadius = h/2 + 'px';
		}
	}
}

window.addEventListener('resize', positionAll);
positionAll();

const held = new Set();

function sync() {
	for (const [id, b] of Object.entries(BTNS))
		b.el.classList.toggle('pressed', held.has(id));
	dbg.textContent = held.size ? [...held].join(' + ') : '';
}
function receiveBtn(id, down) {
	if (!BTNS[id]) return;
	if (down) held.add(id);
	else held.delete(id);
	sync();
}
window.addEventListener('blur', () => { held.clear(); sync(); });
function setDbg(visible) {
	dbg.style.display = visible ? '' : 'none';
}
function applySkin(btn, pressed) {
	const root = document.documentElement;
	root.style.setProperty('--skin', btn);
	root.style.setProperty('--skin-pressed', pressed);
}

applySkin('#FF0000', '#5a0000');
setDbg(false);