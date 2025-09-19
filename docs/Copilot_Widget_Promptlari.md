Birinci widget promptları

İlgili controller ve view context olarak agent'a bildirildikten sonra aşağıdaki prompt yazıldı;

- Bu controller ve buna bağlı cshtml de kategoriler ve bu kategorilere bağlı ürün sayılarını gösteren modern bir widget eklenecek. Bu widget her 10 saniyede bir yenilenecek.

İşlem bittikten sonra bir hata alındı ve bu prompt ile düzeltilmesi sağlandı;

- Widget yüklenmiyor. Console'da aşağıdaki hatayı alıyorum.
Uncaught TypeError: items.forEach is not a function at renderList (admin:310:23) at Object.<anonymous> (admin:326:25) at fire (jquery-3.7.0.js:3213:31) at Object.fireWith [as resolveWith] (jquery-3.7.0.js:3343:7) at done (jquery-3.7.0.js:9617:14) at XMLHttpRequest.<anonymous> (jquery-3.7.0.js:9878:9)

Düzgün bir şekilde çalıştıktan sonra görsel düzeltmeler için aşağıdaki prompt girildi;

- Şu anda çalışıyor ama liste gibi görünüyor. Bu widget pie chart şeklinde gösterilsin. Kütüphane eklemen gerekiyorsa ekle.

___________________________________________________________________________________________________________________________________

İkinci widget promtları

İlgili controller, view ve ExchangeRateHelper context olarak agent'a bildirildikten sonra aşağıdaki prompt yazıldı;

- Evet şimdi de AdminController'a ExchangeRateHelper.cs deki metodları kullanarak veya gerekli metodları yazarak aktif kur bilgilerini getiren widgeti son eklediğimiz widgetin yanına ekleyecek şekilde geliştir.
 
Kur bilgileri başarılı bir şekilde ilk prompt ile geldi. Kur bilgisi cache de tutuluyordu servise her seferinde gitmemek için. Sonrasında her saniye yenileme işlemi için cacheden kaldırılmasını istediğim prompt;

- Kur üzerinde cache uygulamasını engelle.