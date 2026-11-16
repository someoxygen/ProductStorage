# Kullanıcı bazlı ürün erişimi

Ürün API'si, ürün sahibini doğrulanmış JWT'nin kullanıcı kimliğinden alır.
İstek gövdesi veya query string üzerinden ürün sahibi seçilemez.
Listeleme, arama, detay, güncelleme ve silme işlemleri yalnızca oturumdaki
kullanıcının ürünlerinde çalışır. Başka bir kullanıcıya ait ürün için 404,
oturum bulunmadığında veya kullanıcı kimliği geçersiz olduğunda 401 döner.
Ürün adları kullanıcı başına benzersizdir; farklı kullanıcılar aynı adı kullanabilir.

## Veritabanı

Depo kökünden migration'ları uygulayın:

```powershell
dotnet ef database update --project backend/ProductStorageAPI/ProductStorageAPI
```

`AddProductOwnership`, ürünlere `Users` tablosuyla ilişkili `UserId` alanı ve
`(UserId, Name)` benzersiz indeksi ekler. Eski kayıtların sahibi bilinmediğinden
bu kayıtların `UserId` değeri null kalır. Kayıtlar silinmez, ancak hiçbir
kullanıcıya gösterilmez ve API üzerinden sahiplenilemez. Yeni ürünlerin sahibi
API tarafından otomatik atanır.

Eski ürünleri kullanıma açmak için doğru sahiplerini belirledikten sonra ilgili
ürünlerin `UserId` değerlerini veritabanında mevcut bir kullanıcının `Id` değeriyle
eşleştirin. Otomatik olarak ilk kullanıcıya veya giriş yapan kullanıcıya atama
yapılmaz. Kullanıcı silme, ilişkili ürünleri varsa foreign key tarafından engellenir.

Migration geri alınırken eski global ürün adı benzersizliği geri gelir.
Farklı kullanıcıların aynı adlı ürünleri varsa önce bu çakışmalar çözülmelidir.

## Doğrulama

```powershell
dotnet test backend/ProductStorageAPI/ProductStorageAPI.slnx
```

Testler gerçek JWT doğrulaması ve geçici SQLite veritabanıyla HTTP uçlarını
çalıştırır; yerel SQL Server veritabanını değiştirmez. İki kullanıcı arasında
erişim izolasyonunu, sahte sahip gönderimini, kimliksiz erişimi, eski kayıtların
gizlenmesini ve kullanıcı bazlı benzersizliği doğrular.

Frontend, `/products` için giriş kontrolü uygular; 401 yanıtında oturumu temizleyip
giriş sayfasına yönlendirir. Başka sekmede oturum değiştiğinde ürün sayfası yenilenir.
Erişim güvenliği backend tarafından sağlanır.
