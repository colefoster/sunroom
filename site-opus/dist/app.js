(() => {
  "use strict";

  // Values copied from native/SunglassesApp.swift (RGB 0–1 → hex, opacity in %).
  const LAUNCH = { color: "#261a00", opacity: 30 };
  const PRESETS = [
    { key: "amber", name: "Amber", color: "#331f00", opacity: 30 },
    { key: "blue", name: "Blue", color: "#000d33", opacity: 25 },
    { key: "gray", name: "Gray", color: "#0d0d0d", opacity: 35 },
  ];
  const MIN_OPACITY = 5;
  const MAX_OPACITY = 80;
  const STRIP_STEPS = [5, 15, 30, 45, 60, 80];
  const NARROW_BELOW = 640; // must match the @container scene breakpoint in styles.css
  const MIN_W = 14;
  const MIN_H = 14;

  // Start positions are deliberately offset from the share: you place the tint, it never snaps.
  const RIGS = {
    hero: {
      label: "call demo",
      targetName: "View Options",
      geo: { wide: { x: 38.5, y: 9.5, w: 56.5, h: 56.5 }, narrow: { x: 1.5, y: 8.5, w: 87, h: 33 } },
    },
    slides: {
      label: "slide demo",
      targetName: "Request control",
      geo: { wide: { x: 31, y: 12, w: 44, h: 62 }, narrow: { x: 6, y: 8, w: 84, h: 35 } },
    },
  };

  const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const clamp = (v, lo, hi) => Math.min(hi, Math.max(lo, v));
  const round1 = (v) => Math.round(v * 10) / 10;

  const store = {
    get(key) {
      try { return JSON.parse(localStorage.getItem(key)); } catch { return null; }
    },
    set(key, value) {
      try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* private mode: demo still works, just forgets */ }
    },
  };

  function hexToRgb(hex) {
    const n = parseInt(hex.slice(1), 16);
    return [(n >> 16) & 255, (n >> 8) & 255, n & 255];
  }
  function rgba(hex, pct) {
    const [r, g, b] = hexToRgb(hex);
    return `rgba(${r}, ${g}, ${b}, ${pct / 100})`;
  }
  // Swatches in the preset buttons: the stored colours are near-black, so show them lifted.
  function swatch(hex) {
    const [r, g, b] = hexToRgb(hex);
    const max = Math.max(r, g, b, 1);
    const k = 200 / max;
    return `rgb(${Math.round(r * k)}, ${Math.round(g * k)}, ${Math.round(b * k)})`;
  }

  function panelMarkup(id) {
    const presets = PRESETS.map((p) =>
      `<button class="preset" type="button" data-preset="${p.key}" aria-pressed="false"><i style="background:${swatch(p.color)}"></i>${p.name}</button>`
    ).join("");
    return `
      <div class="panel">
        <div class="p-head">
          <svg aria-hidden="true"><use href="#i-sun"/></svg>
          <span class="p-title">Safelight</span>
          <span class="p-show"><span id="${id}-show-l">Show</span>
            <button class="switch" type="button" role="switch" aria-checked="true" aria-labelledby="${id}-show-l" data-show></button>
          </span>
        </div>
        <div class="p-sec p-sec-mode">
          <fieldset class="seg">
            <legend class="p-label">Lock</legend>
            <input type="radio" name="${id}-mode" id="${id}-locked" value="locked" data-mode>
            <label for="${id}-locked"><b><svg aria-hidden="true"><use href="#i-lock"/></svg>Locked</b><small>Clicks pass through</small></label>
            <input type="radio" name="${id}-mode" id="${id}-unlocked" value="unlocked" data-mode>
            <label for="${id}-unlocked"><b><svg aria-hidden="true"><use href="#i-unlock"/></svg>Unlocked</b><small>Move &amp; resize</small></label>
          </fieldset>
        </div>
        <div class="p-sec p-sec-tint">
          <div class="p-row">
            <label class="p-label" for="${id}-color">Tint colour</label>
            <span class="color"><input type="color" id="${id}-color" data-color><span class="hex" data-hex></span></span>
          </div>
          <div class="p-row">
            <label class="p-label" for="${id}-opacity">Opacity</label>
            <input class="range" type="range" id="${id}-opacity" min="${MIN_OPACITY}" max="${MAX_OPACITY}" step="1" data-opacity>
            <output class="pct" for="${id}-opacity" data-pct></output>
          </div>
        </div>
        <div class="p-sec p-sec-presets">
          <span class="p-label" id="${id}-presets-l">Presets</span>
          <div class="presets" role="group" aria-labelledby="${id}-presets-l">${presets}</div>
        </div>
        <div class="p-status-wrap">
          <p class="p-status"><span class="p-geo" data-geo></span><span class="p-msg" data-msg role="status" aria-live="polite"></span></p>
          <div class="p-foot"><span>Always-on-top and Spaces aren’t simulated here.</span><button class="p-reset" type="button" data-reset>Reset demo</button></div>
        </div>
      </div>`;
  }

  class Rig {
    constructor(root) {
      this.id = root.dataset.rig;
      this.cfg = RIGS[this.id];
      this.key = `safelight.demo.v1.${this.id}`;
      this.root = root;
      this.frame = root.querySelector(".scene-frame");
      this.scene = root.querySelector(".scene");
      this.tint = root.querySelector("[data-tint]");
      this.bar = this.tint.querySelector(".tint-bar");
      this.corner = this.tint.querySelector(".tint-corner");
      this.panelHost = root.querySelector("[data-panel]");
      this.panelHost.innerHTML = panelMarkup(this.id);
      this.panel = this.panelHost.querySelector(".panel");
      this.q = (s) => this.panel.querySelector(s);
      this.strip = root.querySelector("[data-strip]");

      this.state = this.load();
      this.layout = this.currentLayout();

      this.buildStrip();
      this.bindPanel();
      this.bindTint();
      this.bindTargets();
      this.render();
      this.message(this.state.locked
        ? `Locked. Clicks go straight through to the call.`
        : `Unlocked. Drag the title bar or the corner to place it.`);

      if (!reduceMotion && this.state.visible) {
        this.tint.classList.add("developing");
        this.tint.addEventListener("animationend", () => this.tint.classList.remove("developing"), { once: true });
      }

      new ResizeObserver(() => {
        const next = this.currentLayout();
        if (next !== this.layout) { this.layout = next; this.render(); }
      }).observe(this.frame);
    }

    defaults() {
      return {
        color: LAUNCH.color,
        opacity: LAUNCH.opacity,
        visible: true,
        locked: false,
        geo: JSON.parse(JSON.stringify(this.cfg.geo)),
      };
    }

    load() {
      const d = this.defaults();
      const s = store.get(this.key);
      if (!s || typeof s !== "object") return d;
      const out = { ...d };
      if (/^#[0-9a-f]{6}$/i.test(s.color)) out.color = s.color.toLowerCase();
      if (Number.isFinite(s.opacity)) out.opacity = clamp(Math.round(s.opacity), MIN_OPACITY, MAX_OPACITY);
      if (typeof s.visible === "boolean") out.visible = s.visible;
      if (typeof s.locked === "boolean") out.locked = s.locked;
      for (const k of ["wide", "narrow"]) {
        const g = s.geo && s.geo[k];
        if (g && ["x", "y", "w", "h"].every((p) => Number.isFinite(g[p]))) out.geo[k] = this.fit(g);
      }
      return out;
    }

    save() { store.set(this.key, this.state); }

    currentLayout() { return this.frame.clientWidth < NARROW_BELOW ? "narrow" : "wide"; }
    get geo() { return this.state.geo[this.layout]; }

    fit(g) {
      const w = clamp(g.w, MIN_W, 100);
      const h = clamp(g.h, MIN_H, 100);
      return { x: clamp(g.x, 0, 100 - w), y: clamp(g.y, 0, 100 - h), w, h };
    }

    setGeo(g) {
      this.state.geo[this.layout] = this.fit(g);
      this.renderGeo();
    }

    // ---- rendering ----
    render() {
      const s = this.state;
      this.tint.style.setProperty("--tint", rgba(s.color, s.opacity));
      this.tint.classList.toggle("is-hidden", !s.visible);
      this.tint.classList.toggle("is-unlocked", !s.locked);
      this.tint.setAttribute("aria-hidden", String(!s.visible || s.locked));

      this.q("[data-show]").setAttribute("aria-checked", String(s.visible));
      this.q(`#${this.id}-locked`).checked = s.locked;
      this.q(`#${this.id}-unlocked`).checked = !s.locked;
      this.q("[data-color]").value = s.color;
      this.q("[data-hex]").textContent = s.color;
      const range = this.q("[data-opacity]");
      range.value = s.opacity;
      range.style.setProperty("--fill", `${((s.opacity - MIN_OPACITY) / (MAX_OPACITY - MIN_OPACITY)) * 100}%`);
      this.q("[data-pct]").textContent = `${s.opacity}%`;
      this.panel.querySelectorAll("[data-preset]").forEach((b) => {
        const p = PRESETS.find((x) => x.key === b.dataset.preset);
        b.setAttribute("aria-pressed", String(p.color === s.color && p.opacity === s.opacity));
      });

      if (this.strip) {
        this.strip.querySelectorAll(".band").forEach((b) => {
          const step = Number(b.dataset.step);
          b.querySelector(".band-tint").style.background = rgba(s.color, step);
          b.setAttribute("aria-pressed", String(step === s.opacity));
        });
      }
      this.renderGeo();
    }

    renderGeo() {
      const g = this.geo;
      const st = this.tint.style;
      st.setProperty("--x", g.x);
      st.setProperty("--y", g.y);
      st.setProperty("--w", g.w);
      st.setProperty("--h", g.h);
      const where = `${Math.round(g.w)}% × ${Math.round(g.h)}% of the screen, ${Math.round(g.x)}% from left, ${Math.round(g.y)}% from top`;
      this.q("[data-geo]").textContent = this.state.visible ? where : `Hidden. Kept: ${where}`;
    }

    message(text, good = false) {
      const el = this.q("[data-msg]");
      el.textContent = text;
      el.classList.toggle("good", good);
    }

    update(patch, msg) {
      Object.assign(this.state, patch);
      this.render();
      this.save();
      if (msg) this.message(msg);
    }

    // ---- controls ----
    bindPanel() {
      this.q("[data-show]").addEventListener("click", () => {
        const visible = !this.state.visible;
        this.update({ visible }, visible
          ? "Shown again, in exactly the same place."
          : "Hidden. Its size and position are kept for when you switch it back on.");
      });

      this.panel.querySelectorAll("[data-mode]").forEach((r) =>
        r.addEventListener("change", () => {
          const locked = r.value === "locked";
          this.update({ locked }, locked
            ? `Locked. Handles gone; clicks now pass through to ${this.cfg.targetName}.`
            : "Unlocked. The tint has a title bar and corner, and it catches clicks.");
        })
      );

      const color = this.q("[data-color]");
      color.addEventListener("input", () => this.update({ color: color.value.toLowerCase() }));

      const range = this.q("[data-opacity]");
      range.addEventListener("input", () => this.update({ opacity: clamp(Number(range.value), MIN_OPACITY, MAX_OPACITY) }));

      this.panel.querySelectorAll("[data-preset]").forEach((b) =>
        b.addEventListener("click", () => {
          const p = PRESETS.find((x) => x.key === b.dataset.preset);
          this.update({ color: p.color, opacity: p.opacity }, `${p.name} preset: ${p.color} at ${p.opacity}%.`);
        })
      );

      this.q("[data-reset]").addEventListener("click", () => {
        this.state = this.defaults();
        this.render();
        this.save();
        this.message("Reset to the app’s launch tint, unlocked, in its starting spot.");
      });

      // The menu-bar icon in the scene opens the real controls, like the app's status item.
      this.root.querySelectorAll("[data-menubar]").forEach((b) =>
        b.addEventListener("click", () => {
          this.panel.classList.remove("pulse");
          void this.panel.offsetWidth;
          this.panel.classList.add("pulse");
          this.q("[data-show]").focus({ preventScroll: true });
          if (this.panel.getBoundingClientRect().top > window.innerHeight - 80) {
            this.panel.scrollIntoView({ behavior: reduceMotion ? "auto" : "smooth", block: "center" });
          }
        })
      );
    }

    buildStrip() {
      if (!this.strip) return;
      this.strip.innerHTML = STRIP_STEPS.map((step) =>
        `<button class="band" type="button" data-step="${step}" aria-pressed="false" aria-label="Set opacity to ${step}%">
           <span class="band-paper" aria-hidden="true"><span class="band-tint"></span></span>
           <span class="band-label"><span>${step}%</span></span>
         </button>`
      ).join("");
      this.strip.querySelectorAll(".band").forEach((b) =>
        b.addEventListener("click", () => {
          const step = Number(b.dataset.step);
          this.update({ opacity: step }, `Opacity set to ${step}% from the test strip.`);
        })
      );
    }

    // ---- dragging, resizing, keyboard ----
    bindTint() {
      const startDrag = (e, mode) => {
        if (this.state.locked || e.button > 0) return;
        e.preventDefault();
        const handle = e.currentTarget;
        handle.setPointerCapture(e.pointerId);
        const rect = this.scene.getBoundingClientRect();
        const start = { px: e.clientX, py: e.clientY, ...this.geo };
        this.tint.classList.add("dragging");

        const move = (ev) => {
          const dx = ((ev.clientX - start.px) / rect.width) * 100;
          const dy = ((ev.clientY - start.py) / rect.height) * 100;
          if (mode === "move") {
            this.setGeo({ ...start, x: start.x + dx, y: start.y + dy });
          } else {
            this.setGeo({
              ...start,
              w: clamp(start.w + dx, MIN_W, 100 - start.x),
              h: clamp(start.h + dy, MIN_H, 100 - start.y),
            });
          }
        };
        const end = () => {
          handle.removeEventListener("pointermove", move);
          handle.removeEventListener("pointerup", end);
          handle.removeEventListener("pointercancel", end);
          this.tint.classList.remove("dragging");
          this.state.geo[this.layout] = this.mapRound(this.geo);
          this.save();
          this.message(mode === "move" ? "Moved. Lock it when it covers the glare." : "Resized. Lock it when it covers the glare.");
        };
        handle.addEventListener("pointermove", move);
        handle.addEventListener("pointerup", end);
        handle.addEventListener("pointercancel", end);
      };

      this.bar.addEventListener("pointerdown", (e) => startDrag(e, "move"));
      this.corner.addEventListener("pointerdown", (e) => startDrag(e, "resize"));

      const keys = (mode) => (e) => {
        const d = { ArrowLeft: [-1, 0], ArrowRight: [1, 0], ArrowUp: [0, -1], ArrowDown: [0, 1] }[e.key];
        if (!d || this.state.locked) return;
        e.preventDefault();
        const step = e.shiftKey ? 5 : 1;
        const g = this.geo;
        if (mode === "move") this.setGeo({ ...g, x: g.x + d[0] * step, y: g.y + d[1] * step });
        else this.setGeo({ ...g, w: clamp(g.w + d[0] * step, MIN_W, 100 - g.x), h: clamp(g.h + d[1] * step, MIN_H, 100 - g.y) });
        this.save();
      };
      this.bar.addEventListener("keydown", keys("move"));
      this.corner.addEventListener("keydown", keys("resize"));

      // Unlocked, the tint is a window and swallows clicks, exactly like the app in Move & Resize.
      this.tint.addEventListener("click", (e) => {
        if (this.state.locked || e.target.closest(".tint-bar, .tint-corner")) return;
        this.tint.classList.remove("caught");
        void this.tint.offsetWidth;
        this.tint.classList.add("caught");
        this.message("The tint caught that click: it’s unlocked. Lock it and clicks pass through.");
      });
    }

    mapRound(g) { return { x: round1(g.x), y: round1(g.y), w: round1(g.w), h: round1(g.h) }; }

    insideTint(x, y) {
      if (!this.state.visible) return false;
      const r = this.tint.getBoundingClientRect();
      return x >= r.left && x <= r.right && y >= r.top && y <= r.bottom;
    }

    // The real things underneath: clicking them is the click-through proof.
    reportHit(btn, e, what) {
      btn.classList.remove("hit");
      void btn.offsetWidth;
      btn.classList.add("hit");
      if (e.detail === 0) {
        this.message(`${what} (keyboard).`);
      } else if (this.insideTint(e.clientX, e.clientY)) {
        this.message(`That click went through the tint to ${this.cfg.targetName}. ${what}.`, true);
      } else {
        this.message(`${what}. The tint wasn’t over that spot, so nothing was in the way.`);
      }
    }

    bindTargets() {
      const vo = this.scene.querySelector(".vo-btn");
      if (vo) {
        const menu = this.scene.querySelector(".vo-menu");
        const close = () => { menu.hidden = true; vo.setAttribute("aria-expanded", "false"); };
        vo.addEventListener("click", (e) => {
          const open = menu.hidden;
          menu.hidden = !open;
          vo.setAttribute("aria-expanded", String(open));
          this.reportHit(vo, e, open ? "View Options opened" : "View Options closed");
        });
        menu.querySelectorAll("[data-vo]").forEach((item) =>
          item.addEventListener("click", (e) => {
            menu.querySelectorAll("[data-vo]").forEach((i) => i.setAttribute("aria-checked", String(i === item)));
            close();
            this.reportHit(vo, e, `Picked “${item.dataset.vo}”`);
            vo.focus({ preventScroll: true });
          })
        );
        menu.addEventListener("keydown", (e) => { if (e.key === "Escape") { close(); vo.focus(); } });
        document.addEventListener("pointerdown", (e) => {
          if (!menu.hidden && !e.target.closest(".vo")) close();
        });
      }

      const rc = this.scene.querySelector(".rc-btn");
      if (rc) {
        rc.addEventListener("click", (e) => {
          const on = rc.getAttribute("aria-pressed") !== "true";
          rc.setAttribute("aria-pressed", String(on));
          rc.textContent = on ? "Control requested" : "Request control";
          this.reportHit(rc, e, on ? "Control requested" : "Request withdrawn");
        });
      }
    }
  }

  document.querySelectorAll("[data-rig]").forEach((el) => new Rig(el));

  // Copy the clone command.
  document.querySelectorAll("[data-copy]").forEach((btn) => {
    const label = btn.querySelector("span");
    btn.addEventListener("click", async () => {
      const src = document.querySelector(btn.dataset.copy);
      const text = src.textContent.trim();
      let ok = false;
      try { await navigator.clipboard.writeText(text); ok = true; } catch {
        const range = document.createRange();
        range.selectNodeContents(src);
        const sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
      }
      label.textContent = ok ? "Copied" : "Press ⌘C";
      setTimeout(() => { label.textContent = "Copy"; }, 1800);
    });
  });
})();
