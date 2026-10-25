const presets = {
  amber: { color: '#331f00', opacity: 30 },
  blue: { color: '#000d33', opacity: 25 },
  gray: { color: '#0d0d0d', opacity: 35 },
};
const clamp = (value, min, max) => Math.max(min, Math.min(value, max));

function initTintDemo(root) {
  const name = root.dataset.demo;
  const stage = root.querySelector('[data-stage]');
  const tint = root.querySelector('[data-tint]');
  const panel = root.querySelector('[data-controls]');
  const check = root.querySelector('[data-action="check"]');
  const storageKey = `sunroom-demo-${name}-v3`;
  const initial = { color: '#261a00', opacity: 30, enabled: true, pinned: true, rect: null };
  let state = { ...initial };
  // Start examples unlocked so the independent box and its handles are immediately visible.
  let moving = true;

  panel.innerHTML = `
    <div class="control-title"><svg aria-hidden="true"><use href="#sun"/></svg><h3>Tint box</h3></div>
    <div class="box-toggle-row"><span>Show box</span><button class="toggle" data-action="enabled" role="switch" aria-checked="true" aria-label="Show ${name} box"><span></span></button></div>
    <div class="box-toggle-row"><span>Lock box</span><button class="toggle" data-action="lock" role="switch" aria-checked="false" aria-label="Lock ${name} box"><span></span></button></div>
    <p class="control-hint box-mode-hint" data-hint>Unlocked — drag the top bar or resize the corner.</p>
    <div class="control-section"><label class="range-label" for="${name}-opacity">Opacity <output data-opacity-output>30%</output></label><input id="${name}-opacity" data-control="opacity" type="range" min="5" max="80" value="30"><div class="range-ends"><span>A little</span><span>A lot</span></div></div>
    <fieldset class="presets"><legend>Pick a mood</legend>${Object.keys(presets).map(key => `<button data-preset="${key}" class="preset" aria-pressed="${key === 'amber'}"><i></i>${key[0].toUpperCase() + key.slice(1)}</button>`).join('')}</fieldset>
    <label class="custom-color">Or choose your own <input type="color" data-control="color" value="#331f00" aria-label="${name} custom tint color"></label>
    <label class="pin-control"><input type="checkbox" data-control="pinned" checked>Always on top</label>
    <button class="reset" data-action="reset">Reset demo</button><div class="sr-only" role="status" data-status></div>`;

  // Store proportions so a saved region still fits after changing device sizes.
  function placeRect(x, y, width, height, save = true) {
    const maxWidth = stage.clientWidth;
    const maxHeight = stage.clientHeight;
    const topInset = Math.min(31, maxHeight);
    width = clamp(width, Math.min(95, maxWidth), maxWidth);
    height = clamp(height, Math.min(65, maxHeight - topInset), maxHeight - topInset);
    x = clamp(x, 0, maxWidth - width);
    y = clamp(y, topInset, maxHeight - height);
    Object.assign(tint.style, { left: `${x}px`, top: `${y}px`, width: `${width}px`, height: `${height}px`, right: 'auto', bottom: 'auto' });
    if (save) state.rect = { x: x / maxWidth, y: y / maxHeight, width: width / maxWidth, height: height / maxHeight };
  }

  function restoreRect() {
    if (!state.rect) return;
    const { x, y, width, height } = state.rect;
    placeRect(x * stage.clientWidth, y * stage.clientHeight, width * stage.clientWidth, height * stage.clientHeight, false);
  }

  function persist() {
    try { localStorage.setItem(storageKey, JSON.stringify(state)); } catch { /* The demo also works when storage is unavailable. */ }
  }

  function announce(message) {
    panel.querySelector('[data-status]').textContent = message;
  }

  function render() {
    root.style.setProperty('--tint', state.color);
    root.style.setProperty('--opacity', state.opacity / 100);
    tint.classList.toggle('off', !state.enabled);
    tint.classList.toggle('moving', moving && state.enabled);
    tint.classList.toggle('unpinned', !state.pinned);
    root.querySelectorAll('[data-action="enabled"]').forEach(button => button.setAttribute('aria-checked', String(state.enabled)));
    root.querySelectorAll('[data-control="opacity"]').forEach(input => { input.value = state.opacity; });
    root.querySelectorAll('[data-opacity-output]').forEach(output => { output.value = `${state.opacity}%`; });
    panel.querySelector('[data-control="color"]').value = state.color;
    panel.querySelector('[data-control="pinned"]').checked = state.pinned;
    panel.querySelectorAll('[data-preset]').forEach(button => {
      const preset = presets[button.dataset.preset];
      const active = preset.color === state.color && preset.opacity === state.opacity;
      button.classList.toggle('active', active);
      button.setAttribute('aria-pressed', String(active));
    });
    root.querySelectorAll('[data-action="lock"]').forEach(button => button.setAttribute('aria-checked', String(!moving)));
    panel.querySelector('[data-hint]').textContent = !state.enabled
      ? 'Hidden — your size, position, and lock mode are kept. Turn Show box on to bring it back.'
      : moving
      ? 'Unlocked — drag the top bar or resize the corner. Lock the box when you want clicks to pass through.'
      : 'Locked — clicks pass through the box. Unlock it to adjust its position and size.';
    persist();
  }

  try {
    const saved = JSON.parse(localStorage.getItem(storageKey));
    if (saved && /^#[0-9a-f]{6}$/i.test(saved.color) && Number.isFinite(saved.opacity)) {
      state = {
        color: saved.color,
        opacity: clamp(saved.opacity, 5, 80),
        enabled: typeof saved.enabled === 'boolean' ? saved.enabled : true,
        pinned: typeof saved.pinned === 'boolean' ? saved.pinned : true,
        rect: saved.rect && ['x', 'y', 'width', 'height'].every(key => Number.isFinite(saved.rect[key]) && saved.rect[key] >= 0 && saved.rect[key] <= 1) ? saved.rect : null,
      };
    }
  } catch { /* Ignore unavailable storage or an outdated saved demo. */ }

  root.querySelectorAll('[data-control="opacity"]').forEach(input => input.addEventListener('input', () => {
    state.opacity = Number(input.value);
    render();
  }));
  panel.querySelector('[data-control="color"]').addEventListener('input', event => {
    state.color = event.target.value;
    render();
  });
  panel.querySelector('[data-control="pinned"]').addEventListener('change', event => {
    state.pinned = event.target.checked;
    render();
    announce(state.pinned ? 'Tint stays above the floating screen share label.' : 'Tint sits below the floating screen share label.');
  });
  panel.querySelectorAll('[data-preset]').forEach(button => button.addEventListener('click', () => {
    Object.assign(state, presets[button.dataset.preset]);
    render();
    announce(`${button.textContent} tint selected`);
  }));
  root.querySelectorAll('[data-action="enabled"]').forEach(button => button.addEventListener('click', () => {
    state.enabled = !state.enabled;
    render();
  }));
  root.querySelector('[data-action="settings"]').addEventListener('click', event => {
    panel.hidden = !panel.hidden;
    event.currentTarget.setAttribute('aria-expanded', String(!panel.hidden));
  });
  root.querySelectorAll('[data-action="lock"]').forEach(button => button.addEventListener('click', () => {
    moving = !moving;
    render();
    announce(moving ? 'Box unlocked. Drag its top bar or resize its corner.' : 'Box locked. Clicks pass through.');
  }));
  check.addEventListener('click', () => check.setAttribute('aria-pressed', String(check.getAttribute('aria-pressed') !== 'true')));
  panel.querySelector('[data-action="reset"]').addEventListener('click', () => {
    state = { ...initial };
    moving = true;
    tint.removeAttribute('style');
    check.setAttribute('aria-pressed', 'false');
    render();
    announce('Demo reset');
  });

  root.querySelectorAll('[data-handle]').forEach(handle => {
    const resize = handle.dataset.handle === 'resize';
    handle.addEventListener('pointerdown', event => {
      if (!moving || !state.enabled || !event.isPrimary || event.button !== 0) return;
      event.preventDefault();
      handle.setPointerCapture(event.pointerId);
      const start = { x: event.clientX, y: event.clientY, left: tint.offsetLeft, top: tint.offsetTop, width: tint.offsetWidth, height: tint.offsetHeight };
      const move = next => {
        if (!moving || !state.enabled) return;
        const dx = next.clientX - start.x;
        const dy = next.clientY - start.y;
        placeRect(resize ? start.left : start.left + dx, resize ? start.top : start.top + dy, resize ? start.width + dx : start.width, resize ? start.height + dy : start.height);
      };
      const end = () => {
        handle.removeEventListener('pointermove', move);
        handle.removeEventListener('pointerup', end);
        handle.removeEventListener('pointercancel', end);
        handle.removeEventListener('lostpointercapture', end);
        persist();
      };
      handle.addEventListener('pointermove', move);
      handle.addEventListener('pointerup', end);
      handle.addEventListener('pointercancel', end);
      handle.addEventListener('lostpointercapture', end);
    });
    handle.addEventListener('keydown', event => {
      if (!moving || !state.enabled) return;
      const deltas = { ArrowLeft: [-1, 0], ArrowRight: [1, 0], ArrowUp: [0, -1], ArrowDown: [0, 1] };
      if (!deltas[event.key]) return;
      event.preventDefault();
      const step = event.shiftKey ? 25 : 10;
      const [dx, dy] = deltas[event.key].map(value => value * step);
      placeRect(tint.offsetLeft + (resize ? 0 : dx), tint.offsetTop + (resize ? 0 : dy), tint.offsetWidth + (resize ? dx : 0), tint.offsetHeight + (resize ? dy : 0));
      persist();
    });
  });

  if (name === 'hero') {
    document.addEventListener('pointerdown', event => {
      if (panel.hidden || panel.contains(event.target) || root.querySelector('[data-action="settings"]').contains(event.target)) return;
      panel.hidden = true;
      root.querySelector('[data-action="settings"]').setAttribute('aria-expanded', 'false');
    });
    root.addEventListener('keydown', event => {
      if (event.key === 'Escape' && !panel.hidden) {
        panel.hidden = true;
        const button = root.querySelector('[data-action="settings"]');
        button.setAttribute('aria-expanded', 'false');
        button.focus();
      }
    });
  }
  new ResizeObserver(restoreRect).observe(stage);
  restoreRect();
  render();
}

document.querySelectorAll('[data-demo]').forEach(initTintDemo);
const dialog = document.querySelector('#install-dialog');
document.querySelectorAll('.install').forEach(button => button.addEventListener('click', () => dialog.showModal()));
document.querySelector('#close-dialog').addEventListener('click', () => dialog.close());
dialog.addEventListener('click', event => {
  const rect = dialog.getBoundingClientRect();
  if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom) dialog.close();
});
