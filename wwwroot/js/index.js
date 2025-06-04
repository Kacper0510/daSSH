const THEME_KEY = "daSSH-theme";

const oppositeTheme = (theme) => theme === "dark" ? "light" : "dark";

function switchTheme(theme) {
    document.body.classList.remove("light", "dark");
    document.body.classList.add(theme);
    localStorage.setItem(THEME_KEY, theme);
    document.getElementById("theme-switcher-icon").innerHTML = oppositeTheme(theme) + "_mode";
}

document.addEventListener("DOMContentLoaded", function() {
    const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)");
    const currentTheme = localStorage.getItem(THEME_KEY) || (prefersDarkScheme.matches ? "dark" : "light");
    switchTheme(currentTheme);
});

document.getElementById("theme-switcher").addEventListener("click", function() {
    const currentTheme = localStorage.getItem(THEME_KEY);
    switchTheme(oppositeTheme(currentTheme));
});
