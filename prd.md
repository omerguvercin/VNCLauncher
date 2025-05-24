# VNC Launcher - Ürün Gereksinimleri Dokümanı

## 1. Giriş

VNC Launcher, sistem tepsisinde çalışan, TightVNC için adres defteri, ağ tarama ve hızlı bağlantı yönetimi sağlayan modern bir masaüstü uygulamasıdır.

## 2. Amaç

Bu uygulamanın temel amacı, kullanıcıların sık kullandıkları VNC bağlantılarını kolayca yönetmelerini, ağlarındaki VNC sunucularını keşfetmelerini ve bu sunuculara hızlı bir şekilde bağlanmalarını sağlamaktır.

## 3. Hedef Kitle

Sık sık farklı bilgisayarlara TightVNC ile bağlanan IT profesyonelleri, sistem yöneticileri ve uzak masaüstü kullanıcıları.

## 4. Ana Özellikler

- **Sistem Tepsisi Entegrasyonu:** Uygulama sistem tepsisinde çalışır ve tepsi ikonundan kolayca erişilebilir.
- **Adres Defteri:**
    - Bağlantı ekleme, düzenleme ve silme.
    - Bağlantı adı, IP adresi, son bağlantı zamanı gibi bilgilerin tutulması.
    - Bağlantıların erişilebilirlik durumunun (ping/TCP port) gösterilmesi.
    - Listeden çift tıklayarak hızlı bağlantı kurma.
- **Ağ Tarama (Keşif):**
    - Belirtilen IP aralığındaki (örn: "192.168.1.1-254" veya tek IP "192.168.1.10") VNC sunucularını tarama.
    - Tarama sırasında işlemi durdurabilme.
    - Bulunan VNC sunucularının IP adreslerini, mümkünse hostname'lerini ve VNC port durumlarını listeleme.
    - Taranan listeden seçilen sunucuları adres defterine kolayca ekleyebilme.
- **Ayarlar:**
    - TightVNC Viewer programının yolunu belirleme.
    - Uygulamanın Windows ile birlikte otomatik başlatılmasını ayarlama.
- **Modern Kullanıcı Arayüzü:** Sekmeli, kullanıcı dostu ve modern bir arayüz.
- **Veri Saklama:** Adres defteri ve ayarların JSON formatında yerel dosyalarda (`VNC_Adresbook.json`, `VNC_Config.json`) saklanması.
- **Bağımlılık Kontrolü:** TightVNC Viewer'ın kurulu olup olmadığını ve yolunun doğru ayarlanıp ayarlanmadığını kontrol etme, kullanıcıyı yönlendirme.

## 5. Kullanım Senaryoları

### 5.1. Adres Defteri Kullanımı
1.  Kullanıcı uygulamayı açar.
2.  "Adres Defteri" sekmesine gider.
3.  Yeni bir bağlantı eklemek için (+) butonuna tıklar, gerekli bilgileri girer ve kaydeder.
4.  Mevcut bir bağlantıyı düzenlemek için listeden seçer ve düzenle (✎) butonuna tıklar.
5.  Bir bağlantıyı silmek için listeden seçer ve sil (-) butonuna tıklar.
6.  Bir bağlantıya bağlanmak için listedeki kayda çift tıklar.

### 5.2. Ağ Tarama Kullanımı
1.  Kullanıcı "Tarama" sekmesine gider.
2.  "IP Aralığı" alanına taramak istediği IP aralığını girer (örn: `192.168.1.1-254` veya `192.168.1.50`).
3.  "Ağı Tara" butonuna tıklar. Tarama başlar ve ilerleme çubuğu ile durum bilgisi gösterilir.
4.  Tarama uzun sürerse veya kullanıcı vazgeçerse "Durdur" butonuna (eski "Ağı Tara" butonu) tıklar.
5.  Tarama tamamlandığında veya durdurulduğunda, VNC portu açık olan cihazlar listelenir.
6.  Kullanıcı listeden adres defterine eklemek istediği cihazları seçer.
7.  "Seçilenleri Ekle" butonuna tıklayarak bu cihazları adres defterine kaydeder.

### 5.3. Ayarların Yapılandırılması
1.  Kullanıcı "Ayarlar" sekmesine gider.
2.  TightVNC Viewer programının yolunu "Gözat..." butonu ile seçer veya manuel olarak girer.
3.  "Windows ile birlikte başlat" seçeneğini işaretler veya işaretini kaldırır.
4.  "Ayarları Kaydet" butonuna tıklar.

## 6. Teknik Gereksinimler

- .NET 8.0 Runtime (veya üstü)
- TightVNC Viewer (Kurulu ve yolu uygulamada doğru şekilde belirtilmiş olmalı)
- Windows İşletim Sistemi

## 7. Kurulum ve İlk Çalıştırma

1.  Uygulamanın yayınlanmış sürümünü (örn: ZIP paketi) edinin.
2.  ZIP dosyasını istediğiniz bir klasöre çıkarın.
3.  `VNCLauncher.exe` dosyasını çalıştırın.
4.  İlk çalıştırmada, "Ayarlar" sekmesine giderek TightVNC Viewer programının kurulu olduğu yolu kontrol edin ve gerekirse doğru yolu belirtip kaydedin.

## 8. Yardım ve Destek

Uygulama arayüzündeki "Yardım" sekmesi, temel kullanım ve özellikler hakkında bilgi içerir.

- **Program Kullanımı:** Program başlatıldığında sistem tepsisinde simge olarak görünür. Simgeye çift tıklandığında adres defteri arayüzü açılır. Kayıtlı bağlantılara çift tıklandığında TightVNC Viewer ile bağlantı kurulur.
- **Tarama Sekmesi:** Belirtilen IP aralığındaki VNC sunucularını bulmanızı sağlar. IP aralığını "192.168.1.1-254" veya "192.168.1.10" (tek IP) formatında girebilirsiniz. Tarama sırasında "Durdur" butonu ile işlemi iptal edebilirsiniz. Bulunan cihazlardan seçtiklerinizi "Seçilenleri Ekle" butonu ile adres defterine kaydedebilirsiniz.
- **Özellikler Listesi:**
    - Sistem tepsisinde çalışır
    - TightVNC için adres defteri işlevi görür
    - Ağdaki VNC sunucularını tarama ve adres defterine ekleme
    - Tek sayfa sekmeli arayüz
    - Modern ve kullanıcı dostu arabirim
    - Bağlantı durumu kontrolü (ping ve TCP port)
    - Otomatik başlangıç seçeneği
- **İletişim:** Geliştirici Ömer Güvercin (t.me/iomerg)

## 9. Dağıtım ve Dosya Yapısı

Uygulamanın son kullanıcıya dağıtımı için hedeflenen dosya yapısı aşağıdaki gibidir:

```
C:\VNCLauncher\  (Kullanıcının uygulamayı kopyaladığı ana dizin)
|-- VNCLauncher.exe
|-- Data\               (Veri ve kaynak dosyaları için alt klasör)
    |-- Res\            (Bağımlı DLL'ler ve diğer kaynaklar için alt klasör)
        |-- VNCLauncher.dll
        |-- Hardcodet.NotifyIcon.Wpf.dll
        |-- Newtonsoft.Json.dll
        |-- win-x64\        (Platforma özel dosyalar)
            |-- publish\    (Gerekirse)
            |-- ... (diğer DLL'ler)
        |-- ... (diğer DLL ve kaynak dosyaları)
    |-- VNC_Adresbook.json  (Kullanıcı adres defteri)
    |-- VNC_Config.json     (Uygulama ayarları)
|-- VNCLauncher.runtimeconfig.json
|-- VNCLauncher.deps.json
```

Bu yapılandırmanın temel amacı, ana uygulama dizinini (`C:\VNCLauncher\`) mümkün olduğunca temiz tutmak ve sadece çalıştırılabilir dosya (`VNCLauncher.exe`) ile temel yapılandırma dosyalarını burada barındırmaktır. Tüm bağımlılıklar ve kullanıcı verileri `Data` klasörü altında organize edilecektir.

**Karşılaşılan Zorluk ve Çözüm Arayışı:**

.NET uygulamalarının bağımlılıkları (özellikle ana uygulama DLL'i olan `VNCLauncher.dll`) çözümleme şekli nedeniyle, `VNCLauncher.exe`'nin `VNCLauncher.dll` dosyasını kendi yanında bulamaması durumunda uygulama başlatılamamaktadır. Bu sorunu aşmak için aşağıdaki yöntemler denenmiş ve değerlendirilmektedir:

1.  **`VNCLauncher.runtimeconfig.json` ile `additionalProbingPaths` Kullanımı:** Bu yöntem, ek DLL arama yolları belirtmek için kullanılır. `Data/Res` ve `Data/Res/win-x64` yolları eklenerek DLL'lerin bu konumlarda bulunması hedeflenmiştir.
2.  **`VNCLauncher.deps.json` Dosyasının Düzenlenmesi:** Bu dosya, uygulamanın bağımlılıklarının nerede olduğunu tanımlar. `VNCLauncher.dll` için yolun `Data/Res/VNCLauncher.dll` olarak belirtilmesi denenmiştir.
3.  **`App.xaml.cs` İçinde `AssemblyResolve` Olayı:** Uygulama çalıştıktan sonra bulunamayan assembly'ler için özel bir arama mantığı eklenmiştir. Ancak bu, ana uygulama DLL'i olan `VNCLauncher.dll` yüklenemediğinde devreye girememektedir.

Mevcut durumda, .NET çalışma zamanının `VNCLauncher.dll` dosyasını her zaman `.exe` ile aynı dizinde araması nedeniyle sorunlar yaşanmaktadır. İdeal dosya yapısına ulaşmak için en stabil çözümün, başarılı bir derleme sonrasında yapılandırma dosyalarının (`.runtimeconfig.json` ve `.deps.json`) dikkatlice ayarlanması ve test edilmesi olduğu görülmektedir. Gerekirse, tüm dosyaların tek bir dizine yayılması (istenmeyen bir durum olsa da) bir son çare olarak değerlendirilebilir.

## 10. Gelecek Geliştirmeler ve Bilinen Sorunlar

- **Versiyonlama:** Uygulama için bir versiyonlama stratejisi belirlenmeli ve her sürümde değişiklikler takip edilmelidir.
- **Detaylı Hata Yönetimi ve Günlükleme:** Kullanıcıya gösterilen genel hata mesajlarının ötesinde, geliştirici için daha detaylı hata günlükleri (örn: bir log dosyasına) tutulabilir.
- **Çoklu Dil Desteği:** Uygulama arayüzü için farklı dil seçenekleri eklenebilir.
- **Gelişmiş Ağ Tarama Seçenekleri:** Port aralığı belirleme, farklı VNC portlarını tarama gibi özellikler eklenebilir.
- **İçe/Dışa Aktarma:** Adres defterini içe/dışa aktarma özelliği eklenebilir (örn: CSV, JSON formatlarında).
- **Otomatik Güncelleme:** Uygulama için bir otomatik güncelleme mekanizması düşünülebilir.
- **Bilinen Sorunlar:**
    - Uygulamanın istenen "kök dizinde tek exe" dosya yapısıyla dağıtılmasında .NET bağımlılık çözümleme mekanizmaları nedeniyle zorluklar yaşanmaktadır. Bu durum, uygulamanın başlatılamamasına neden olabilmektedir.
    - `dotnet build` komutunun terminal üzerinden belirli çalışma dizinlerinde proje dosyasını bulamama sorunu (manuel derleme ile aşılmaya çalışılıyor). 