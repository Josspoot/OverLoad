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

    function toggle(el, cls, on) { if (el) el.classList.toggle(cls, on); }

    function openPanel() {
        collapseNav();
        toggle(document.getElementById('settingsPanel'), 'open', true);
        toggle(document.getElementById('settingsOverlay'), 'open', true);
        document.body.style.overflow = 'hidden';
    }
    function closePanel() {
        toggle(document.getElementById('settingsPanel'), 'open', false);
        toggle(document.getElementById('settingsOverlay'), 'open', false);
        document.body.style.overflow = '';
    }

    // El rail de navegación siempre está visible (iconos); expandir muestra los nombres.
    function expandNav() {
        closePanel();
        toggle(document.getElementById('navDrawer'), 'expanded', true);
        toggle(document.getElementById('navOverlay'), 'open', true);
    }
    function collapseNav() {
        toggle(document.getElementById('navDrawer'), 'expanded', false);
        toggle(document.getElementById('navOverlay'), 'open', false);
    }
    function toggleNav() {
        var drawer = document.getElementById('navDrawer');
        if (drawer && drawer.classList.contains('expanded')) collapseNav();
        else expandNav();
    }

    document.addEventListener('DOMContentLoaded', function () {
        // Sincroniza el marcado del panel con el tema ya aplicado.
        applyTheme(currentTheme());

        // Panel de configuración.
        var gear = document.getElementById('settingsGear');
        var closeBtn = document.getElementById('settingsClose');
        var overlay = document.getElementById('settingsOverlay');
        if (gear) gear.addEventListener('click', openPanel);
        if (closeBtn) closeBtn.addEventListener('click', closePanel);
        if (overlay) overlay.addEventListener('click', closePanel);

        // Rail de navegación: el botón lo expande/comprime; el fondo lo comprime.
        var navOverlay = document.getElementById('navOverlay');
        var collapseBtn = document.getElementById('navCollapse');
        if (collapseBtn) collapseBtn.addEventListener('click', toggleNav);
        if (navOverlay) navOverlay.addEventListener('click', collapseNav);

        document.querySelectorAll('.theme-option').forEach(function (opt) {
            opt.addEventListener('click', function () {
                applyTheme(opt.getAttribute('data-theme'));
            });
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { closePanel(); collapseNav(); }
        });
    });
})();
