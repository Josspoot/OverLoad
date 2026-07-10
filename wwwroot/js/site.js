// OverLoad — barra lateral de configuración y sistema de temas.
// El tema activo se guarda en localStorage y se aplica en <html data-theme>.
// (La aplicación inicial sin parpadeo la hace un script inline en <head>.)

(function () {
    'use strict';

    var STORAGE_KEY = 'overload-theme';
    var DEFAULT_THEME = 'aqua';
    var THEMES = ['aqua', 'dark', 'midnight'];

    function currentTheme() {
        var t = localStorage.getItem(STORAGE_KEY);
        return THEMES.indexOf(t) !== -1 ? t : DEFAULT_THEME;
    }

    function applyTheme(theme) {
        if (THEMES.indexOf(theme) === -1) theme = DEFAULT_THEME;
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem(STORAGE_KEY, theme); } catch (e) { /* modo privado */ }
        // Refleja el estado activo en las opciones del panel.
        document.querySelectorAll('.theme-option').forEach(function (opt) {
            opt.classList.toggle('active', opt.getAttribute('data-theme') === theme);
        });
    }

    var MINI_KEY = 'overload-nav-mini';

    function toggle(el, cls, on) { if (el) el.classList.toggle(cls, on); }

    function openPanel() {
        closeNav();
        toggle(document.getElementById('settingsPanel'), 'open', true);
        toggle(document.getElementById('settingsOverlay'), 'open', true);
        document.body.style.overflow = 'hidden';
    }
    function closePanel() {
        toggle(document.getElementById('settingsPanel'), 'open', false);
        toggle(document.getElementById('settingsOverlay'), 'open', false);
        document.body.style.overflow = '';
    }

    function openNav() {
        closePanel();
        toggle(document.getElementById('navDrawer'), 'open', true);
        toggle(document.getElementById('navOverlay'), 'open', true);
        document.body.style.overflow = 'hidden';
    }
    function closeNav() {
        toggle(document.getElementById('navDrawer'), 'open', false);
        toggle(document.getElementById('navOverlay'), 'open', false);
        document.body.style.overflow = '';
    }

    function applyMini() {
        var mini = localStorage.getItem(MINI_KEY) === '1';
        toggle(document.getElementById('navDrawer'), 'mini', mini);
    }

    document.addEventListener('DOMContentLoaded', function () {
        // Sincroniza el marcado del panel con el tema ya aplicado.
        applyTheme(currentTheme());
        applyMini();

        // Panel de configuración.
        var gear = document.getElementById('settingsGear');
        var closeBtn = document.getElementById('settingsClose');
        var overlay = document.getElementById('settingsOverlay');
        if (gear) gear.addEventListener('click', openPanel);
        if (closeBtn) closeBtn.addEventListener('click', closePanel);
        if (overlay) overlay.addEventListener('click', closePanel);

        // Menú de navegación (hamburguesa) con estado comprimido persistente.
        var burger = document.getElementById('navHamburger');
        var navOverlay = document.getElementById('navOverlay');
        var collapseBtn = document.getElementById('navCollapse');
        if (burger) burger.addEventListener('click', openNav);
        if (navOverlay) navOverlay.addEventListener('click', closeNav);
        if (collapseBtn) collapseBtn.addEventListener('click', function () {
            var drawer = document.getElementById('navDrawer');
            var mini = drawer && drawer.classList.toggle('mini');
            try { localStorage.setItem(MINI_KEY, mini ? '1' : '0'); } catch (e) { /* modo privado */ }
        });

        document.querySelectorAll('.theme-option').forEach(function (opt) {
            opt.addEventListener('click', function () {
                applyTheme(opt.getAttribute('data-theme'));
            });
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { closePanel(); closeNav(); }
        });
    });
})();
