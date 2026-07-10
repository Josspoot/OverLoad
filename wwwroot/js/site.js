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

    function openPanel() {
        var panel = document.getElementById('settingsPanel');
        var overlay = document.getElementById('settingsOverlay');
        if (panel) panel.classList.add('open');
        if (overlay) overlay.classList.add('open');
        document.body.style.overflow = 'hidden';
    }

    function closePanel() {
        var panel = document.getElementById('settingsPanel');
        var overlay = document.getElementById('settingsOverlay');
        if (panel) panel.classList.remove('open');
        if (overlay) overlay.classList.remove('open');
        document.body.style.overflow = '';
    }

    document.addEventListener('DOMContentLoaded', function () {
        // Sincroniza el marcado del panel con el tema ya aplicado.
        applyTheme(currentTheme());

        var gear = document.getElementById('settingsGear');
        var closeBtn = document.getElementById('settingsClose');
        var overlay = document.getElementById('settingsOverlay');

        if (gear) gear.addEventListener('click', openPanel);
        if (closeBtn) closeBtn.addEventListener('click', closePanel);
        if (overlay) overlay.addEventListener('click', closePanel);

        document.querySelectorAll('.theme-option').forEach(function (opt) {
            opt.addEventListener('click', function () {
                applyTheme(opt.getAttribute('data-theme'));
            });
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closePanel();
        });
    });
})();
