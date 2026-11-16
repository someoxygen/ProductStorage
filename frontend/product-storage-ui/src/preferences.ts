import { computed, readonly, ref, watchEffect } from "vue";
import tr from "./locales/tr.json";
import en from "./locales/en.json";

export type MessageKey = keyof typeof tr;
export type Language = "tr" | "en";
export type Theme = "light" | "dark";
const messages: Record<Language, Record<MessageKey, string>> = { tr, en };
const languageState = ref<Language>("tr");
const themeState = ref<Theme>("light");
export const language = readonly(languageState);
export const theme = readonly(themeState);
export const locale = computed(() =>
  languageState.value === "tr" ? "tr-TR" : "en-US",
);

export function t(
  key: MessageKey,
  params: Record<string, string | number> = {},
): string {
  return messages[languageState.value][key].replace(
    /\{(\w+)\}/g,
    (match, name: string) => String(params[name] ?? match),
  );
}

function read(key: string): string | null {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}
function persist(key: string, value: string) {
  try {
    localStorage.setItem(key, value);
  } catch {
    /* Preferences still work in memory. */
  }
}
export function setLanguage(value: Language) {
  languageState.value = value;
  persist("rafio.language", value);
}
export function setTheme(value: Theme) {
  themeState.value = value;
  persist("rafio.theme", value);
}

export function initializePreferences() {
  const systemTheme = window.matchMedia("(prefers-color-scheme: dark)");
  function sync() {
    languageState.value = read("rafio.language") === "en" ? "en" : "tr";
    const saved = read("rafio.theme");
    themeState.value =
      saved === "light" || saved === "dark"
        ? saved
        : systemTheme.matches
          ? "dark"
          : "light";
  }
  sync();
  const stop = watchEffect(() => {
    document.documentElement.lang = languageState.value;
    document.documentElement.dataset.theme = themeState.value;
    document.documentElement.style.colorScheme = themeState.value;
    document
      .querySelector('meta[name="theme-color"]')
      ?.setAttribute(
        "content",
        themeState.value === "dark" ? "#14221e" : "#173e35",
      );
  });
  const onStorage = (event: StorageEvent) => {
    if (
      event.key === null ||
      event.key === "rafio.language" ||
      event.key === "rafio.theme"
    )
      sync();
  };
  const onSystemTheme = () => {
    if (!read("rafio.theme"))
      themeState.value = systemTheme.matches ? "dark" : "light";
  };
  window.addEventListener("storage", onStorage);
  systemTheme.addEventListener("change", onSystemTheme);
  if (import.meta.hot)
    import.meta.hot.dispose(() => {
      stop();
      window.removeEventListener("storage", onStorage);
      systemTheme.removeEventListener("change", onSystemTheme);
    });
}
