<script setup lang="ts">
import { t, locale, type MessageKey } from "@/preferences";

import api from "@/api/api";
import type { Product } from "@/models/product";
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import PreferenceControls from "@/components/PreferenceControls.vue";
import BrandMark from "@/components/BrandMark.vue";
import AppIcon from "@/components/AppIcon.vue";
const router = useRouter();
const products = ref<Product[]>([]);
const loading = ref(true);
const busy = ref(false);
const error = ref<MessageKey | "">("");
const formError = ref<MessageKey | "">("");
const notice = ref<MessageKey | "">("");
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
  new Intl.NumberFormat(locale.value, {
    style: "currency",
    currency: "TRY",
    maximumFractionDigits: 2,
  }).format(value);
const number = (value: number) =>
  new Intl.NumberFormat(locale.value).format(value);
const date = (value: Date) =>
  new Date(value).toLocaleDateString(locale.value, {
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
  const query = search.value.trim().toLocaleLowerCase(locale.value);
  return products.value
    .filter(
      (p) =>
        (!query ||
          `${p.name} ${p.description}`
            .toLocaleLowerCase(locale.value)
            .includes(query)) &&
        (filter.value === "all" ||
          (filter.value === "active" && p.isActive) ||
          (filter.value === "inactive" && !p.isActive) ||
          (filter.value === "low" && p.isActive && p.amount <= 5)),
    )
    .sort((a, b) =>
      sort.value === "name"
        ? a.name.localeCompare(b.name, locale.value)
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
    error.value = "loadError";
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
    formError.value = "validationError";
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
    notice.value = form.id ? "updated" : "created";
    await getProducts();
  } catch {
    formError.value = "saveError";
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
    notice.value = "deleted";
    await getProducts();
  } catch {
    formError.value = "deleteError";
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
    [
      t("csvProduct"),
      t("description"),
      t("csvPrice"),
      t("csvStock"),
      t("csvStatus"),
    ],
    ...filtered.value.map((p) => [
      p.name,
      p.description,
      p.price,
      p.amount,
      p.isActive ? t("active") : t("inactive"),
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
          <strong> {{ t("personalWorkspace") }} </strong><small>
            {{ t("management") }}
          </small>
        </div>
      </div>
      <span class="nav-caption"> {{ t("workspaceLabel") }} </span>
      <nav :aria-label="t('mainMenu')">
        <RouterLink class="nav-item selected" to="/products"
          ><AppIcon name="grid" /> {{ t("myProducts") }}
          <span class="nav-count">{{ products.length }}</span></RouterLink
        >
      </nav>
      <div class="sidebar-note">
        <span class="note-icon"><AppIcon name="box" /></span
        ><strong> {{ t("orderFeelsGood") }} </strong>
        <p>
          {{ t("allInOnePlace") }} <br />
          {{ t("underControl") }}
        </p>
        <div class="note-line"></div>
      </div>
      <div class="sidebar-bottom">
        <AppIcon name="shield" /><span> {{ t("privateAndSecure") }} </span>
      </div>
    </aside>
    <div class="main-area">
      <header class="topbar">
        <div class="mobile-brand"><BrandMark /></div>
        <div class="breadcrumb">
          {{ t("workspace") }} <span>/</span
          ><strong> {{ t("myProducts") }} </strong>
        </div>
        <div class="topbar-right">
          <PreferenceControls />
          <span class="personal-badge"
            ><i class="status-dot"></i> {{ t("personalAccount") }} </span
          ><button
            class="logout-button"
            :aria-label="t('logout')"
            @click="logout"
          >
            <AppIcon name="logout" /><span> {{ t("logout") }} </span>
          </button>
        </div>
      </header>
      <main class="dashboard">
        <div class="page-heading">
          <div>
            <div class="eyebrow">{{ t("overview") }}</div>
            <h1>
              {{ t("productsTogether") }} <span class="title-dot">.</span>
            </h1>
            <p>{{ t("dashboardIntro") }}</p>
          </div>
          <button class="button primary" @click="openEditor()">
            <AppIcon name="plus" /> {{ t("addProduct") }}
          </button>
        </div>
        <div v-if="notice" class="notice success" role="status">
          <AppIcon name="check" />{{ notice ? t(notice) : ""
          }}<button
            class="icon-button"
            :aria-label="t('dismissNotice')"
            @click="notice = ''"
          >
            <AppIcon name="close" />
          </button>
        </div>
        <div class="stats-grid">
          <article class="stat-card">
            <div class="stat-label">
              {{ t("totalProducts") }}
              <span class="stat-icon"><AppIcon name="box" /></span>
            </div>
            <strong
              >{{ loading ? "—" : number(products.length)
              }}<small> {{ t("productUnit") }} </small></strong
            ><span class="stat-foot"> {{ t("productTypes") }} </span>
          </article>
          <article class="stat-card">
            <div class="stat-label">
              {{ t("totalStock") }}
              <span class="stat-icon"><AppIcon name="chart" /></span>
            </div>
            <strong
              >{{ loading ? "—" : number(totalUnits)
              }}<small> {{ t("unit") }} </small></strong
            ><span class="stat-foot"> {{ t("stockTotalHint") }} </span>
          </article>
          <article class="stat-card value-card">
            <div class="stat-label">
              {{ t("inventoryValue") }}
              <span class="stat-icon"><span>₺</span></span>
            </div>
            <strong>{{ loading ? "—" : money(totalValue) }}</strong><span
              class="stat-foot"
            >
              {{ t("valueHint") }}
            </span>
          </article>
          <article class="stat-card">
            <div class="stat-label">
              {{ t("lowStock") }}
              <span class="stat-icon amber"><AppIcon name="clock" /></span>
            </div>
            <strong
              >{{ loading ? "—" : number(lowStock)
              }}<small> {{ t("productUnit") }} </small></strong
            ><button
              class="stat-link"
              @click="
                filter = 'low';
                page = 1;
              "
            >
              {{ t("lowStockHint") }}
            </button>
          </article>
        </div>
        <section class="inventory-panel" aria-labelledby="inventory-title">
          <div class="panel-heading">
            <div>
              <h2 id="inventory-title">
                {{ t("productList") }}
                <span class="count-badge">{{ products.length }}</span>
              </h2>
              <p>{{ t("listIntro") }}</p>
            </div>
            <button
              class="button secondary export-button"
              :disabled="!filtered.length || loading"
              @click="exportProducts"
            >
              <AppIcon name="download" /> {{ t("export") }}
            </button>
          </div>
          <div class="table-toolbar">
            <div class="filter-tabs" :aria-label="t('productFilters')">
              <button
                v-for="tab in [
                  { id: 'all', label: t('allProducts') },
                  { id: 'active', label: t('active') },
                  { id: 'inactive', label: t('inactive') },
                  { id: 'low', label: t('lowStock') },
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
                  :aria-label="t('searchProducts')"
                  :placeholder="t('searchPlaceholder')"
                  @input="page = 1" /></label
              ><select
                v-model="sort"
                :aria-label="t('sorting')"
                @change="page = 1"
              >
                <option value="newest">{{ t("newest") }}</option>
                <option value="name">{{ t("nameSort") }}</option>
                <option value="stock">{{ t("stockSort") }}</option>
              </select>
            </div>
          </div>
          <div v-if="loading" class="empty-state" role="status">
            <span class="spinner"></span>
            <h3>{{ t("loadingProducts") }}</h3>
          </div>
          <div v-else-if="error" class="empty-state" role="alert">
            <AppIcon name="box" />
            <h3>{{ t("connectionError") }}</h3>
            <p>{{ error ? t(error) : "" }}</p>
            <button class="button secondary" @click="getProducts">
              {{ t("retry") }}
            </button>
          </div>
          <div v-else-if="!filtered.length" class="empty-state">
            <div class="empty-icon">
              <AppIcon :name="products.length ? 'search' : 'box'" />
            </div>
            <h3>
              {{ products.length ? t("noResults") : t("emptyTitle") }}
            </h3>
            <p>
              {{ products.length ? t("noResultsHint") : t("emptyHint") }}
            </p>
            <button
              v-if="!products.length"
              class="button primary"
              @click="openEditor()"
            >
              <AppIcon name="plus" /> {{ t("addFirst") }}</button
            ><button
              v-else
              class="button secondary"
              @click="
                search = '';
                filter = 'all';
              "
            >
              {{ t("clearFilters") }}
            </button>
          </div>
          <div v-else class="table-scroll">
            <table>
              <thead>
                <tr>
                  <th scope="col">{{ t("productColumn") }}</th>
                  <th scope="col">{{ t("priceColumn") }}</th>
                  <th scope="col">{{ t("stockColumn") }}</th>
                  <th scope="col">{{ t("statusColumn") }}</th>
                  <th scope="col">{{ t("createdColumn") }}</th>
                  <th scope="col" class="align-right">
                    {{ t("actionsColumn") }}
                  </th>
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
                        <strong>{{ product.name }}</strong><span
                          :title="product.description"
                          >{{ product.description || t("noDescription") }}</span
                        >
                      </div>
                    </div>
                  </td>
                  <td class="price-cell">{{ money(product.price) }}</td>
                  <td>
                    <span :class="{ 'low-amount': product.amount <= 5 }"
                      >{{ number(product.amount) }}
                      <span class="muted"> {{ t("unit") }} </span></span
                    >
                  </td>
                  <td>
                    <span
                      class="status-badge"
                      :class="product.isActive ? 'is-active' : 'is-inactive'"
                      ><i></i
                      >{{
                        product.isActive ? t("active") : t("inactive")
                      }}</span
                    >
                  </td>
                  <td class="date-cell">{{ date(product.createDate) }}</td>
                  <td>
                    <div class="row-actions">
                      <button
                        class="icon-button"
                        :aria-label="t('editNamed', { name: product.name })"
                        :title="t('edit')"
                        @click="openEditor(product)"
                      >
                        <AppIcon name="edit" /></button
                      ><button
                        class="icon-button danger-icon"
                        :aria-label="t('deleteNamed', { name: product.name })"
                        :title="t('delete')"
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
              >{{ t("productCount", { count: number(filtered.length) })
              }}<span v-if="filtered.length">
                ·
                {{
                  t("showing", {
                    from: number((currentPage - 1) * 8 + 1),
                    to: number(Math.min(currentPage * 8, filtered.length)),
                  })
                }}</span
              ></span
            >
            <div class="pagination">
              <button
                :disabled="currentPage === 1"
                :aria-label="t('previousPage')"
                @click="page = currentPage - 1"
              >
                ‹</button
              ><span>{{ currentPage }} / {{ pages }}</span
              ><button
                :disabled="currentPage === pages"
                :aria-label="t('nextPage')"
                @click="page = currentPage + 1"
              >
                ›
              </button>
            </div>
          </div>
        </section>
        <footer class="dashboard-footer">
          <span> {{ t("inPlace") }} </span><span> {{ t("footerBrand") }} </span>
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
            <span class="eyebrow"> {{ t("editInventory") }} </span>
            <h2 id="editor-title">
              {{ form.id ? t("editProduct") : t("newProduct") }}
            </h2>
          </div>
          <button
            type="button"
            class="icon-button"
            :aria-label="t('closeDialog')"
            :disabled="busy"
            @click="editor?.close()"
          >
            <AppIcon name="close" />
          </button>
        </div>
        <div class="modal-body">
          <div v-if="formError" class="notice error" role="alert">
            {{ formError ? t(formError) : "" }}
          </div>
          <label class="field">
            {{ t("productName") }}
            <input
              v-model="form.name"
              :placeholder="t('productNamePlaceholder')"
              maxlength="450"
              required
              autofocus
              :disabled="busy" /></label
          ><label class="field">
            {{ t("description") }}
            <span class="optional"> {{ t("optional") }} </span><textarea
              v-model="form.description"
              :placeholder="t('descriptionPlaceholder')"
              rows="3"
              :disabled="busy"
            ></textarea>
          </label>
          <div class="form-columns">
            <label class="field">
              {{ t("unitPrice") }}
              <input
                v-model.number="form.price"
                type="number"
                min="0"
                step="0.01"
                required
                :disabled="busy" /></label
            ><label class="field">
              {{ t("stockQuantity") }}
              <input
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
              ><strong> {{ t("activeProduct") }} </strong
              ><small> {{ t("activeHint") }} </small></span
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
            {{ t("cancel") }}</button><button
            class="button primary"
            :disabled="busy"
          >
            <AppIcon name="check" />{{ busy ? t("saving") : t("saveProduct") }}
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
        <h2 id="delete-title">{{ t("deleteTitle") }}</h2>
        <p class="muted">
          <strong>{{ deleting?.name }}</strong> {{ t("deleteWarning") }}
        </p>
        <div v-if="formError" class="notice error" role="alert">
          {{ formError ? t(formError) : "" }}
        </div>
      </div>
      <div class="modal-footer">
        <button
          class="button secondary"
          :disabled="busy"
          @click="deleteDialog?.close()"
        >
          {{ t("cancel") }}</button><button
          class="button danger"
          :disabled="busy"
          @click="deleteProduct"
        >
          {{ busy ? t("deleting") : t("deleteProduct") }}
        </button>
      </div>
    </dialog>
  </div>
</template>
