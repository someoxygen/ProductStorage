<script setup lang="ts">
import api from "@/api/api";
import type { Product } from "@/models/product";
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import BrandMark from "@/components/BrandMark.vue";
import AppIcon from "@/components/AppIcon.vue";
const router = useRouter();
const products = ref<Product[]>([]);
const loading = ref(true);
const busy = ref(false);
const error = ref("");
const formError = ref("");
const notice = ref("");
const search = ref("");
const filter = ref("all");
const sort = ref("newest");
const page = ref(1);
const editor = ref<HTMLDialogElement>();
const deleteDialog = ref<HTMLDialogElement>();
const deleting = ref<Product>();
const form = reactive({
  id: 0,
  name: "",
  description: "",
  price: 0,
  amount: 0,
  isActive: true,
  createDate: new Date(),
});
const money = (value: number) =>
  new Intl.NumberFormat("tr-TR", {
    style: "currency",
    currency: "TRY",
    maximumFractionDigits: 2,
  }).format(value);
const number = (value: number) => new Intl.NumberFormat("tr-TR").format(value);
const date = (value: Date) =>
  new Date(value).toLocaleDateString("tr-TR", {
    day: "2-digit",
    month: "short",
    year: "numeric",
  });
const totalUnits = computed(() =>
  products.value.reduce((total, product) => total + product.amount, 0),
);
const totalValue = computed(() =>
  products.value.reduce(
    (total, product) => total + product.price * product.amount,
    0,
  ),
);
const lowStock = computed(
  () =>
    products.value.filter((product) => product.amount <= 5 && product.isActive)
      .length,
);
const filtered = computed(() => {
  const query = search.value.trim().toLocaleLowerCase("tr-TR");
  return products.value
    .filter(
      (p) =>
        (!query ||
          `${p.name} ${p.description}`
            .toLocaleLowerCase("tr-TR")
            .includes(query)) &&
        (filter.value === "all" ||
          (filter.value === "active" && p.isActive) ||
          (filter.value === "inactive" && !p.isActive) ||
          (filter.value === "low" && p.isActive && p.amount <= 5)),
    )
    .sort((a, b) =>
      sort.value === "name"
        ? a.name.localeCompare(b.name, "tr")
        : sort.value === "stock"
          ? a.amount - b.amount
          : new Date(b.createDate).getTime() - new Date(a.createDate).getTime(),
    );
});
const pages = computed(() => Math.max(1, Math.ceil(filtered.value.length / 8)));
const currentPage = computed(() => Math.min(page.value, pages.value));
const visibleProducts = computed(() =>
  filtered.value.slice((currentPage.value - 1) * 8, currentPage.value * 8),
);
async function getProducts() {
  loading.value = true;
  error.value = "";
  try {
    products.value = (await api.get<Product[]>("/Products")).data;
  } catch {
    error.value =
      "Ürünler yüklenemedi. Bağlantınızı kontrol edip tekrar deneyin.";
  } finally {
    loading.value = false;
  }
}
function openEditor(product?: Product) {
  Object.assign(
    form,
    product
      ? { ...product, createDate: new Date(product.createDate) }
      : {
          id: 0,
          name: "",
          description: "",
          price: 0,
          amount: 0,
          isActive: true,
          createDate: new Date(),
        },
  );
  formError.value = "";
  editor.value?.showModal();
}
async function saveProduct() {
  if (busy.value) return;
  if (
    !form.name.trim() ||
    !Number.isFinite(form.price) ||
    form.price < 0 ||
    !Number.isInteger(form.amount) ||
    form.amount < 0
  ) {
    formError.value =
      "Ürün adı, geçerli bir fiyat ve tam sayı stok miktarı girin.";
    return;
  }
  busy.value = true;
  formError.value = "";
  notice.value = "";
  try {
    const request = { ...form, name: form.name.trim(), updateDate: new Date() };
    if (form.id) await api.put(`/Products/${form.id}`, request);
    else await api.post("/Products", request);
    editor.value?.close();
    notice.value = form.id
      ? "Ürün bilgileri güncellendi."
      : "Yeni ürün envanterine eklendi.";
    await getProducts();
  } catch {
    formError.value =
      "Ürün kaydedilemedi. Ürün adı daha önce kullanılmış olabilir. Bilgilerinizi kontrol edip tekrar deneyin.";
  } finally {
    busy.value = false;
  }
}
function askDelete(product: Product) {
  deleting.value = product;
  formError.value = "";
  deleteDialog.value?.showModal();
}
async function deleteProduct() {
  if (!deleting.value || busy.value) return;
  busy.value = true;
  formError.value = "";
  notice.value = "";
  try {
    await api.delete(`/Products/${deleting.value.id}`);
    deleteDialog.value?.close();
    notice.value = "Ürün silindi.";
    await getProducts();
  } catch {
    formError.value = "Ürün silinemedi. Lütfen tekrar deneyin.";
  } finally {
    busy.value = false;
  }
}
function logout() {
  localStorage.removeItem("token");
  products.value = [];
  router.replace("/login");
}
function handleSessionChange(event: StorageEvent) {
  if (event.key === "token" || event.key === null) {
    products.value = [];
    window.location.reload();
  }
}
function exportProducts() {
  const cell = (value: unknown) =>
    `"${String(value)
      .replace(/^[=+@\-]/, "'$&")
      .replaceAll('"', '""')}"`;
  const rows = [
    ["Ürün", "Açıklama", "Birim fiyat (TRY)", "Stok", "Durum"],
    ...filtered.value.map((p) => [
      p.name,
      p.description,
      p.price,
      p.amount,
      p.isActive ? "Aktif" : "Pasif",
    ]),
  ];
  const url = URL.createObjectURL(
    new Blob(
      ["\uFEFF" + rows.map((row) => row.map(cell).join(";")).join("\r\n")],
      { type: "text/csv;charset=utf-8;" },
    ),
  );
  const link = document.createElement("a");
  link.href = url;
  link.download = "rafio-envanter.csv";
  link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
onMounted(() => {
  window.addEventListener("storage", handleSessionChange);
  getProducts();
});
onUnmounted(() => window.removeEventListener("storage", handleSessionChange));
</script>

<template>
  <div class="workspace">
    <aside class="sidebar">
      <BrandMark />
      <div class="workspace-label">
        <span class="workspace-avatar">R</span>
        <div>
          <strong>Kişisel çalışma alanı</strong
          ><small>Ürün ve stok yönetimi</small>
        </div>
      </div>
      <span class="nav-caption">ÇALIŞMA ALANI</span>
      <nav aria-label="Ana menü">
        <RouterLink class="nav-item selected" to="/products"
          ><AppIcon name="grid" />Ürünlerim<span class="nav-count">{{
            products.length
          }}</span></RouterLink
        >
      </nav>
      <div class="sidebar-note">
        <span class="note-icon"><AppIcon name="box" /></span
        ><strong>Düzen, iyi hissettirir.</strong>
        <p>Tüm ürünlerin tek bir yerde.<br />Her şey kontrolün altında.</p>
        <div class="note-line"></div>
      </div>
      <div class="sidebar-bottom">
        <AppIcon name="shield" /><span>Sana özel. Güvenle saklı.</span>
      </div>
    </aside>
    <div class="main-area">
      <header class="topbar">
        <div class="mobile-brand"><BrandMark /></div>
        <div class="breadcrumb">
          Çalışma alanı <span>/</span><strong>Ürünlerim</strong>
        </div>
        <div class="topbar-right">
          <span class="personal-badge"
            ><i class="status-dot"></i> Kişisel hesap</span
          ><button class="logout-button" @click="logout">
            <AppIcon name="logout" /><span>Çıkış yap</span>
          </button>
        </div>
      </header>
      <main class="dashboard">
        <div class="page-heading">
          <div>
            <div class="eyebrow">ENVANTERİNE GENEL BAKIŞ</div>
            <h1>Ürünlerin, bir arada<span class="title-dot">.</span></h1>
            <p>Stoklarını takip et, ürünlerini düzenle, işine odaklan.</p>
          </div>
          <button class="button primary" @click="openEditor()">
            <AppIcon name="plus" />Yeni ürün ekle
          </button>
        </div>
        <div v-if="notice" class="notice success" role="status">
          <AppIcon name="check" />{{ notice
          }}<button
            class="icon-button"
            aria-label="Bildirimi kapat"
            @click="notice = ''"
          >
            <AppIcon name="close" />
          </button>
        </div>
        <div class="stats-grid">
          <article class="stat-card">
            <div class="stat-label">
              Toplam ürün<span class="stat-icon"><AppIcon name="box" /></span>
            </div>
            <strong
              >{{ loading ? "—" : number(products.length)
              }}<small>ürün</small></strong
            ><span class="stat-foot">Envanterindeki ürün çeşitleri</span>
          </article>
          <article class="stat-card">
            <div class="stat-label">
              Toplam stok<span class="stat-icon"><AppIcon name="chart" /></span>
            </div>
            <strong
              >{{ loading ? "—" : number(totalUnits)
              }}<small>adet</small></strong
            ><span class="stat-foot">Tüm ürünlerin toplam adedi</span>
          </article>
          <article class="stat-card value-card">
            <div class="stat-label">
              Envanter değeri<span class="stat-icon"><span>₺</span></span>
            </div>
            <strong>{{ loading ? "—" : money(totalValue) }}</strong
            ><span class="stat-foot">Birim fiyat × stok miktarı</span>
          </article>
          <article class="stat-card">
            <div class="stat-label">
              Düşük stok<span class="stat-icon amber"
                ><AppIcon name="clock"
              /></span>
            </div>
            <strong
              >{{ loading ? "—" : number(lowStock) }}<small>ürün</small></strong
            ><button
              class="stat-link"
              @click="
                filter = 'low';
                page = 1;
              "
            >
              5 adet ve altındaki aktif ürünler ↗
            </button>
          </article>
        </div>
        <section class="inventory-panel" aria-labelledby="inventory-title">
          <div class="panel-heading">
            <div>
              <h2 id="inventory-title">
                Ürün listesi
                <span class="count-badge">{{ products.length }}</span>
              </h2>
              <p>Envanterinin tüm detayları burada.</p>
            </div>
            <button
              class="button secondary export-button"
              :disabled="!filtered.length || loading"
              @click="exportProducts"
            >
              <AppIcon name="download" />Dışa aktar
            </button>
          </div>
          <div class="table-toolbar">
            <div class="filter-tabs" aria-label="Ürün filtreleri">
              <button
                v-for="tab in [
                  { id: 'all', label: 'Tüm ürünler' },
                  { id: 'active', label: 'Aktif' },
                  { id: 'inactive', label: 'Pasif' },
                  { id: 'low', label: 'Düşük stok' },
                ]"
                :key="tab.id"
                :class="{ active: filter === tab.id }"
                :aria-pressed="filter === tab.id"
                @click="
                  filter = tab.id;
                  page = 1;
                "
              >
                {{ tab.label }}
              </button>
            </div>
            <div class="search-sort">
              <label class="search-field"
                ><AppIcon name="search" /><input
                  v-model="search"
                  aria-label="Ürün ara"
                  placeholder="Ürün adı veya açıklama ara…"
                  @input="page = 1" /></label
              ><select v-model="sort" aria-label="Sıralama" @change="page = 1">
                <option value="newest">En yeni</option>
                <option value="name">İsim: A–Z</option>
                <option value="stock">En az stok</option>
              </select>
            </div>
          </div>
          <div v-if="loading" class="empty-state" role="status">
            <span class="spinner"></span>
            <h3>Ürünlerin yükleniyor…</h3>
          </div>
          <div v-else-if="error" class="empty-state" role="alert">
            <AppIcon name="box" />
            <h3>Şu an bağlantı kuramadık.</h3>
            <p>{{ error }}</p>
            <button class="button secondary" @click="getProducts">
              Tekrar dene
            </button>
          </div>
          <div v-else-if="!filtered.length" class="empty-state">
            <div class="empty-icon">
              <AppIcon :name="products.length ? 'search' : 'box'" />
            </div>
            <h3>
              {{
                products.length
                  ? "Aradığın ürün burada görünmüyor."
                  : "Yeni bir düzen, ilk ürünle başlar."
              }}
            </h3>
            <p>
              {{
                products.length
                  ? "Farklı bir arama veya filtre deneyebilirsin."
                  : "İlk ürününü ekle; fiyatını, stok durumunu ve detaylarını tek yerden yönet."
              }}
            </p>
            <button
              v-if="!products.length"
              class="button primary"
              @click="openEditor()"
            >
              <AppIcon name="plus" />İlk ürününü ekle</button
            ><button
              v-else
              class="button secondary"
              @click="
                search = '';
                filter = 'all';
              "
            >
              Filtreleri temizle
            </button>
          </div>
          <div v-else class="table-scroll">
            <table>
              <thead>
                <tr>
                  <th scope="col">ÜRÜN</th>
                  <th scope="col">BİRİM FİYAT</th>
                  <th scope="col">STOK</th>
                  <th scope="col">DURUM</th>
                  <th scope="col">EKLENME TARİHİ</th>
                  <th scope="col" class="align-right">İŞLEMLER</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="product in visibleProducts" :key="product.id">
                  <td>
                    <div class="product-cell">
                      <div
                        class="product-avatar"
                        :class="`tone-${product.id % 3}`"
                      >
                        <AppIcon name="box" />
                      </div>
                      <div>
                        <strong>{{ product.name }}</strong
                        ><span :title="product.description">{{
                          product.description || "Açıklama eklenmedi"
                        }}</span>
                      </div>
                    </div>
                  </td>
                  <td class="price-cell">{{ money(product.price) }}</td>
                  <td>
                    <span :class="{ 'low-amount': product.amount <= 5 }"
                      >{{ number(product.amount) }}
                      <span class="muted">adet</span></span
                    >
                  </td>
                  <td>
                    <span
                      class="status-badge"
                      :class="product.isActive ? 'is-active' : 'is-inactive'"
                      ><i></i>{{ product.isActive ? "Aktif" : "Pasif" }}</span
                    >
                  </td>
                  <td class="date-cell">{{ date(product.createDate) }}</td>
                  <td>
                    <div class="row-actions">
                      <button
                        class="icon-button"
                        :aria-label="`${product.name} ürününü düzenle`"
                        title="Düzenle"
                        @click="openEditor(product)"
                      >
                        <AppIcon name="edit" /></button
                      ><button
                        class="icon-button danger-icon"
                        :aria-label="`${product.name} ürününü sil`"
                        title="Sil"
                        @click="askDelete(product)"
                      >
                        <AppIcon name="trash" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="table-footer">
            <span
              >{{ filtered.length }} ürün<span v-if="filtered.length">
                · {{ (currentPage - 1) * 8 + 1 }}–{{
                  Math.min(currentPage * 8, filtered.length)
                }}
                arası gösteriliyor</span
              ></span
            >
            <div class="pagination">
              <button
                :disabled="currentPage === 1"
                aria-label="Önceki sayfa"
                @click="page = currentPage - 1"
              >
                ‹</button
              ><span>{{ currentPage }} / {{ pages }}</span
              ><button
                :disabled="currentPage === pages"
                aria-label="Sonraki sayfa"
                @click="page = currentPage + 1"
              >
                ›
              </button>
            </div>
          </div>
        </section>
        <footer class="dashboard-footer">
          <span>Her şey yerli yerinde.</span
          ><span>Rafio · Ürün ve Stok Yönetimi</span>
        </footer>
      </main>
    </div>
    <dialog
      ref="editor"
      class="modal"
      aria-labelledby="editor-title"
      @cancel="busy && $event.preventDefault()"
    >
      <form @submit.prevent="saveProduct">
        <div class="modal-header">
          <div>
            <span class="eyebrow">ENVANTERİNİ DÜZENLE</span>
            <h2 id="editor-title">
              {{ form.id ? "Ürünü düzenle" : "Yeni bir ürün ekle" }}
            </h2>
          </div>
          <button
            type="button"
            class="icon-button"
            aria-label="Pencereyi kapat"
            :disabled="busy"
            @click="editor?.close()"
          >
            <AppIcon name="close" />
          </button>
        </div>
        <div class="modal-body">
          <div v-if="formError" class="notice error" role="alert">
            {{ formError }}
          </div>
          <label class="field"
            >Ürün adı<input
              v-model="form.name"
              placeholder="Örn. Seramik kahve fincanı"
              maxlength="450"
              required
              autofocus
              :disabled="busy" /></label
          ><label class="field"
            >Açıklama <span class="optional">İsteğe bağlı</span
            ><textarea
              v-model="form.description"
              placeholder="Ürünle ilgili bilmek istediğin detaylar…"
              rows="3"
              :disabled="busy"
            ></textarea>
          </label>
          <div class="form-columns">
            <label class="field"
              >Birim fiyat (₺)<input
                v-model.number="form.price"
                type="number"
                min="0"
                step="0.01"
                required
                :disabled="busy" /></label
            ><label class="field"
              >Stok miktarı<input
                v-model.number="form.amount"
                type="number"
                min="0"
                step="1"
                required
                :disabled="busy"
            /></label>
          </div>
          <label class="toggle-field"
            ><span
              ><strong>Aktif ürün</strong
              ><small>Bu ürünü aktif envanterinde takip et.</small></span
            ><input
              v-model="form.isActive"
              type="checkbox"
              role="switch"
              :disabled="busy"
          /></label>
        </div>
        <div class="modal-footer">
          <button
            type="button"
            class="button secondary"
            :disabled="busy"
            @click="editor?.close()"
          >
            Vazgeç</button
          ><button class="button primary" :disabled="busy">
            <AppIcon name="check" />{{
              busy ? "Kaydediliyor…" : "Ürünü kaydet"
            }}
          </button>
        </div>
      </form>
    </dialog>
    <dialog
      ref="deleteDialog"
      class="modal small-modal"
      aria-labelledby="delete-title"
      @cancel="busy && $event.preventDefault()"
    >
      <div class="modal-body">
        <div class="delete-symbol"><AppIcon name="trash" /></div>
        <h2 id="delete-title">Bu ürünü silmek istiyor musun?</h2>
        <p class="muted">
          <strong>{{ deleting?.name }}</strong> envanterinden kaldırılacak. Bu
          işlem geri alınamaz.
        </p>
        <div v-if="formError" class="notice error" role="alert">
          {{ formError }}
        </div>
      </div>
      <div class="modal-footer">
        <button
          class="button secondary"
          :disabled="busy"
          @click="deleteDialog?.close()"
        >
          Vazgeç</button
        ><button class="button danger" :disabled="busy" @click="deleteProduct">
          {{ busy ? "Siliniyor…" : "Ürünü sil" }}
        </button>
      </div>
    </dialog>
  </div>
</template>
