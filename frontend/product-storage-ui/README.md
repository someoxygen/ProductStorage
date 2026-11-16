# Rafio — Ürün ve Stok Yönetimi

Vue 3 + TypeScript ile hazırlanmış kullanıcıya özel ürün yönetim arayüzü.

## Çalıştırma

Bu dizinde `npm install` ve ardından `npm run dev` çalıştırın.
`.env` dosyasındaki `VITE_API_URL`, backend API adresini (`/api` dahil) göstermelidir.
Üretim çıktısı ve TypeScript kontrolü için `npm run build` kullanın.

## Arayüz

- Giriş ve kayıt ekranları ortak `AuthScreen.vue` bileşenini kullanır.
- `public/rafio.svg`, hem navigasyondaki logonun hem tarayıcı sekmesinin ikonudur.
- Ortak renkler, tipografi ve responsive kurallar `src/styles.css` içindedir.
- Sayfa başlıkları `App.vue` üzerinden geçerli sayfaya ve seçilen dile göre güncellenir.
- Ürün panelinde arama, durum filtresi, sıralama ve sekizer ürünlük sayfalama bulunur.
- Ürün ekleme/düzenleme ve silme onayı klavye ile kullanılabilen yerel dialog pencerelerindedir.
- Dışa aktarım, mevcut arama ve filtreye uyan tüm ürünleri CSV olarak indirir.
- TR / EN butonlarıyla arayüz dili, güneş / ay butonuyla açık ve koyu tema seçilir. Tercihler tarayıcıda saklanır.
- Çeviriler `src/locales/`, ortak tercih yönetimi `src/preferences.ts` içindedir.
- Fiyatların para birimi TRY (₺) olarak kalır; tarih ve sayı biçimleri seçilen dile göre değişir.
- Düşük stok, aktif ürünlerde 5 adet ve altıdır. Envanter değeri tüm ürünlerin fiyat × adet toplamıdır.
- Fontlar Google Fonts üzerinden yüklenir; erişim yoksa sistem sans-serif fontu kullanılır.

API sözleşmesi ve backend'deki kullanıcı bazlı erişim kontrolleri korunur.

Projenin tamamına ait İngilizce kurulum, mimari ve API rehberi için [ana README](../../README.md) dosyasına bakın.
