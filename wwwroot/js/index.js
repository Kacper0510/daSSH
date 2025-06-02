const THEME_KEY = "daSSH-theme";

document.addEventListener("DOMContentLoaded", function() {
    const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)");
    const currentTheme = localStorage.getItem(THEME_KEY) || (prefersDarkScheme.matches ? "dark" : "light");
    document.body.classList.add(currentTheme);
    localStorage.setItem(THEME_KEY, currentTheme);
});

document.getElementById("theme-switcher").addEventListener("click", function() {
    const currentTheme = localStorage.getItem(THEME_KEY);
    const newTheme = currentTheme === "dark" ? "light" : "dark";
    document.body.classList.remove(currentTheme);
    document.body.classList.add(newTheme);
    localStorage.setItem(THEME_KEY, newTheme);
});
