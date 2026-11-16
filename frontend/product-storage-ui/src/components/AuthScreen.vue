<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import api from "@/api/api";
import BrandMark from "./BrandMark.vue";
import AppIcon from "./AppIcon.vue";
const props = defineProps<{ register?: boolean }>();
const router = useRouter();
const fullName = ref("");
const email = ref("");
const password = ref("");
const visible = ref(false);
const busy = ref(false);
const error = ref("");
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
    error.value = props.register
      ? "Hesap oluşturulamadı. Bilgilerinizi kontrol edin; bu e-posta zaten kayıtlı olabilir."
      : "Giriş yapılamadı. E-posta ve şifrenizi kontrol edip tekrar deneyin.";
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
        <span class="eyebrow">DAHA DÜZENLİ BİR İŞ GÜNÜ</span>
        <h1>Her ürünün<br />yeri belli.<br /><span>Kontrol sende.</span></h1>
        <p>
          Ürünlerini bir araya getir, stoklarını takip et.<br />İşine odaklanmak
          için kendine alan aç.
        </p>
        <div class="stock-illustration" aria-hidden="true">
          <div class="illustration-top">
            <span class="mini-dots">● ● ●</span><span>rafio / envanter</span>
          </div>
          <div class="illustration-body">
            <div class="big-cube"><AppIcon name="box" /></div>
            <div>
              <span class="illustration-label">HER ŞEY YERLİ YERİNDE</span
              ><strong>Bir bakışta, tüm stokların.</strong>
              <div class="decor-bars">
                <i></i><i></i><i></i><i></i><i></i><i></i><i></i>
              </div>
            </div>
          </div>
          <div class="illustration-bottom">
            <span><i class="status-dot"></i> Sana özel çalışma alanı</span
            ><AppIcon name="check" />
          </div>
        </div>
      </div>
      <div class="story-footer">
        <span>Az karmaşa. Daha çok kontrol.</span
        ><span>© {{ new Date().getFullYear() }} Rafio</span>
      </div>
    </section>
    <section class="auth-form-side">
      <div class="auth-top">
        {{ register ? "Zaten hesabın var mı?" : "Rafio’da yeni misin?" }}
        <RouterLink :to="register ? '/login' : '/register'"
          >{{ register ? "Giriş yap" : "Hesap oluştur" }} ↗</RouterLink
        >
      </div>
      <div class="auth-form-wrap">
        <div class="section-symbol">
          <AppIcon :name="register ? 'box' : 'shield'" />
        </div>
        <span class="eyebrow">SENİN ENVANTERİN, SENİN ALANIN</span>
        <h2>{{ register ? "Düzene ilk adım." : "Tekrar hoş geldin." }}</h2>
        <p class="muted">
          {{
            register
              ? "Hesabını oluştur, ürünlerine yeni bir yer aç."
              : "Çalışma alanına giriş yap, kaldığın yerden devam et."
          }}
        </p>
        <form @submit.prevent="submit" class="auth-form">
          <div v-if="error" class="notice error" role="alert">{{ error }}</div>
          <label v-if="register" class="field"
            >Ad soyad<input
              v-model="fullName"
              autocomplete="name"
              placeholder="Adınız ve soyadınız"
              required
              maxlength="100"
              :disabled="busy"
          /></label>
          <label class="field"
            >E-posta adresi<input
              v-model="email"
              type="email"
              autocomplete="email"
              placeholder="ornek@sirket.com"
              required
              :disabled="busy"
          /></label>
          <label class="field"
            >Şifre
            <div class="password-field">
              <input
                v-model="password"
                :type="visible ? 'text' : 'password'"
                :autocomplete="register ? 'new-password' : 'current-password'"
                placeholder="Şifrenizi girin"
                required
                :disabled="busy"
              /><button
                type="button"
                class="icon-button"
                :aria-label="visible ? 'Şifreyi gizle' : 'Şifreyi göster'"
                :aria-pressed="visible"
                @click="visible = !visible"
              >
                <AppIcon name="eye" />
              </button></div
          ></label>
          <button class="button primary auth-submit" :disabled="busy">
            {{
              busy
                ? "Lütfen bekleyin…"
                : register
                  ? "Hesabımı oluştur"
                  : "Giriş yap"
            }}<AppIcon v-if="!busy" name="arrow" />
          </button>
        </form>
        <div class="auth-assurance">
          <AppIcon name="shield" /><span
            >Ürünlerin yalnızca senin hesabında görünür.</span
          >
        </div>
      </div>
      <div class="auth-bottom">
        Ürün ve stok yönetimi, olması gerektiği kadar sade.
      </div>
    </section>
  </main>
</template>
