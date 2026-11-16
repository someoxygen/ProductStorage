<script setup lang="ts">
import { t } from "@/preferences";

const icon = `${import.meta.env.BASE_URL}rafio.svg`;
</script>
<template>
  <RouterLink class="brand" to="/" :aria-label="t('brandHome')"
    ><img :src="icon" alt="" width="42" height="42" /><span
      >rafio<span class="brand-dot">.</span></span
    ></RouterLink
  >
</template>
