<script setup lang="ts">
import { watchEffect } from "vue";
import { useRoute } from "vue-router";
import { t } from "./preferences";
const route = useRoute();
watchEffect(() => {
  const title =
    route.path === "/products"
      ? "myProducts"
      : route.path === "/register"
        ? "createAccount"
        : "login";
  document.title = `${t(title)} | Rafio — ${t("tagline")}`;
});
</script>

<template>
  <router-view />
</template>

<style scoped></style>
