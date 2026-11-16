<script setup lang="ts">
import { language, theme, setLanguage, setTheme, t } from "@/preferences";
import AppIcon from "./AppIcon.vue";
</script>

<template>
  <div class="preference-controls" role="group" :aria-label="t('preferences')">
    <div class="language-buttons" role="group" :aria-label="t('language')">
      <button
        type="button"
        lang="tr"
        aria-label="Türkçe"
        :aria-pressed="language === 'tr'"
        @click="setLanguage('tr')"
      >
        TR
      </button>
      <button
        type="button"
        lang="en"
        aria-label="English"
        :aria-pressed="language === 'en'"
        @click="setLanguage('en')"
      >
        EN
      </button>
    </div>
    <button
      type="button"
      class="theme-button"
      :aria-label="t(theme === 'dark' ? 'lightMode' : 'darkMode')"
      :title="t(theme === 'dark' ? 'lightMode' : 'darkMode')"
      @click="setTheme(theme === 'dark' ? 'light' : 'dark')"
    >
      <AppIcon :name="theme === 'dark' ? 'sun' : 'moon'" />
    </button>
  </div>
</template>
