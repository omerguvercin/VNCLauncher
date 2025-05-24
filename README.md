# VNC Launcher

VNC Launcher, sistem tepsisinde çalışan, TightVNC için adres defteri ve hızlı bağlantı yönetimi sağlayan modern bir masaüstü uygulamasıdır.

## Ana Özellikler
- Sistem tepsisinde çalışır
- TightVNC için adres defteri işlevi görür
- Ağdaki VNC sunucularını tarama ve adres defterine ekleme
- Tek sayfa sekmeli arayüz
- Modern ve kullanıcı dostu arabirim
- Bağlantı durumu kontrolü (ping)
- Otomatik başlangıç seçeneği
- Bağımlılık kontrolü
- Ayarlanabilir VNC bağlantı portu

## Teknik Gereksinimler
- .NET 8.0 Runtime
- TightVNC Viewer (örn: `C:\Program Files\TightVNC\tvnviewer.exe`)

## Kurulum
1. [Releases](https://github.com/omerguvercin/VNCLauncher/releases) sayfasından en son sürümü indirin.
2. ZIP dosyasını istediğiniz bir klasöre çıkarın.
3. `VNCLauncher.exe` dosyasını çalıştırın.
4. İlk çalıştırmada, "Ayarlar" sekmesine giderek TightVNC Viewer programının yolunu ve varsayılan VNC bağlantı portunu kontrol edin, gerekirse doğru bilgileri girip kaydedin.

## Kullanım
Program başlatıldığında sistem tepsisinde simge olarak görünür. Simgeye çift tıklandığında adres defteri arayüzü açılır. Kayıtlı bağlantılara çift tıklandığında, ayarlarda belirtilen port numarası kullanılarak TightVNC Viewer ile bağlantı kurulur.

## Ayarlar
"Ayarlar" sekmesinden TightVNC Viewer programının yolu, varsayılan VNC bağlantı portu ve uygulamanın Windows ile otomatik başlatma seçeneği ayarlanabilir. Ayarlar `VNC_Config.json` dosyasında saklanır.

## Bağlantı Yönetimi
- Bağlantı eklemek: (+) butonuna tıklayıp bağlantı adı ve IP adresi (örn: `192.168.1.10` veya `10.0.0.5`) girerek.
- Bağlantı silmek: Listeden kaydı seçip (-) butonuna tıklayarak.
- Bağlantı düzenlemek: Listeden kaydı seçip düzenle (✎) butonuna tıklayarak.

Bağlantı bilgileri `VNC_Adresbook.json` dosyasında saklanır.

## Geliştirme
Projeyi geliştirmek için:
```bash
git clone https://github.com/omerguvercin/VNCLauncher.git
cd VNCLauncher/VNCLauncher # Proje ana dizinine gidin
dotnet build
```

## Lisans
Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakın. 