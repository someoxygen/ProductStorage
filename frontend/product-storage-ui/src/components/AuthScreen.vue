<script setup lang="ts">
import { t, type MessageKey } from "@/preferences";

import { ref } from "vue";
import { useRouter } from "vue-router";
import api from "@/api/api";
import BrandMark from "./BrandMark.vue";
import PreferenceControls from "./PreferenceControls.vue";
import AppIcon from "./AppIcon.vue";
const props = defineProps<{ register?: boolean }>();
const router = useRouter();
const fullName = ref("");
const email = ref("");
const password = ref("");
const visible = ref(false);
const busy = ref(false);
const error = ref<MessageKey | "">("");
async function submit() {
  if (busy.value) return;
  busy.value = true;
  error.value = "";
  try {
    const response = await api.post(
      `/Auth/${props.register ? "register" : "login"}`,
      {
        fullName: fullName.value.trim(),
        email: email.value.trim(),
        password: password.value,
      },
    );
    localStorage.setItem("token", response.data.token);
    await router.replace("/products");
  } catch {
    error.value = props.register ? "registerError" : "loginError";
  } finally {
    busy.value = false;
  }
}
</script>
<template>
  <main class="auth-page">
    <section class="auth-story">
      <BrandMark />
      <div class="story-content">
        <span class="eyebrow"> {{ t("storyEyebrow") }} </span>
        <h1>
          {{ t("storyLine1") }} <br />
          {{ t("storyLine2") }} <br /><span> {{ t("storyAccent") }} </span>
        </h1>
        <p>
          {{ t("storyIntro") }} <br />
          {{ t("storyDetail") }}
        </p>
        <div class="stock-illustration" aria-hidden="true">
          <div class="illustration-top">
            <span class="mini-dots">● ● ●</span
            ><span> {{ t("illustrationPath") }} </span>
          </div>
          <div class="illustration-body">
            <div class="big-cube"><AppIcon name="box" /></div>
            <div>
              <span class="illustration-label">
                {{ t("inPlaceLabel") }} </span><strong>
                {{ t("atAGlance") }}
              </strong>
              <div class="decor-bars">
                <i></i><i></i><i></i><i></i><i></i><i></i><i></i>
              </div>
            </div>
          </div>
          <div class="illustration-bottom">
            <span><i class="status-dot"></i> {{ t("privateWorkspace") }} </span
            ><AppIcon name="check" />
          </div>
        </div>
      </div>
      <div class="story-footer">
        <span> {{ t("lessClutter") }} </span><span
          >© {{ new Date().getFullYear() }} Rafio</span
        >
      </div>
    </section>
    <section class="auth-form-side">
      <PreferenceControls class="auth-preferences" />
      <div class="auth-top">
        {{ register ? t("haveAccount") : t("newHere") }}
        <RouterLink :to="register ? '/login' : '/register'"
          >{{ register ? t("login") : t("createAccount") }} ↗</RouterLink
        >
      </div>
      <div class="auth-form-wrap">
        <div class="section-symbol">
          <AppIcon :name="register ? 'box' : 'shield'" />
        </div>
        <span class="eyebrow"> {{ t("yourSpace") }} </span>
        <h2>{{ register ? t("registerTitle") : t("welcomeBack") }}</h2>
        <p class="muted">
          {{ register ? t("registerIntro") : t("loginIntro") }}
        </p>
        <form @submit.prevent="submit" class="auth-form">
          <div v-if="error" class="notice error" role="alert">
            {{ error ? t(error) : "" }}
          </div>
          <label v-if="register" class="field">
            {{ t("fullName") }}
            <input
              v-model="fullName"
              autocomplete="name"
              :placeholder="t('fullNamePlaceholder')"
              required
              maxlength="100"
              :disabled="busy"
          /></label>
          <label class="field">
            {{ t("email") }}
            <input
              v-model="email"
              type="email"
              autocomplete="email"
              :placeholder="t('emailPlaceholder')"
              required
              :disabled="busy"
          /></label>
          <label class="field">
            {{ t("password") }}
            <div class="password-field">
              <input
                v-model="password"
                :type="visible ? 'text' : 'password'"
                :autocomplete="register ? 'new-password' : 'current-password'"
                :placeholder="t('passwordPlaceholder')"
                required
                :disabled="busy"
              /><button
                type="button"
                class="icon-button"
                :aria-label="visible ? t('hidePassword') : t('showPassword')"
                :aria-pressed="visible"
                @click="visible = !visible"
              >
                <AppIcon name="eye" />
              </button></div
          ></label>
          <button class="button primary auth-submit" :disabled="busy">
            {{
              busy
                ? t("pleaseWait")
                : register
                  ? t("createMyAccount")
                  : t("login")
            }}<AppIcon v-if="!busy" name="arrow" />
          </button>
        </form>
        <div class="auth-assurance">
          <AppIcon name="shield" /><span> {{ t("privacyNote") }} </span>
        </div>
      </div>
      <div class="auth-bottom">{{ t("simpleManagement") }}</div>
    </section>
  </main>
</template>
