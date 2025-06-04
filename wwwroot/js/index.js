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

document.querySelectorAll(".copy-button").forEach(button => {
    button.addEventListener("click", function() {
        const code = document.getElementById("copy-content").innerHTML;
        navigator.clipboard.writeText(code).then(() => {
            document.getElementById("copy-text").innerHTML = "Copied!";
            setTimeout(() => {
                document.getElementById("copy-text").innerHTML = "Copy to clipboard";
            }, 1000);
        }).catch(err => {
            console.error('Failed to copy text: ', err);
        });
    });
});

const accountButton = document.getElementById("account");
if (accountButton) {
    accountButton.addEventListener("click", () => {
        const menu = document.getElementById("account-menu");
        menu.open = !menu.open;
    });
}

document.querySelectorAll(".open-dialog").forEach(button => {
    button.addEventListener("click", async function() {
        const dialogId = this.getAttribute("open-dialog");
        const dialog = document.getElementById(dialogId);
        if (dialog) {
            await dialog.show();
        } else {
            console.error(`Dialog with ID ${dialogId} not found.`);
        }
    });
});
