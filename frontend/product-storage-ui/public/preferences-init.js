// Apply saved appearance before CSS is painted, including when storage is blocked.
(() => {
  let theme;
  let language;
  try {
    theme = localStorage.getItem("rafio.theme");
    language = localStorage.getItem("rafio.language");
  } catch {
    /* Use the defaults below. */
  }
  if (theme !== "light" && theme !== "dark") {
    theme = window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "dark"
      : "light";
  }
  document.documentElement.dataset.theme = theme;
  document.documentElement.style.colorScheme = theme;
  document.documentElement.lang = language === "en" ? "en" : "tr";
})();
