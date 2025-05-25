# VNC Launcher

## Özellikler
- Adres defteri ile kolay bağlantı yönetimi
- Favori ekleme
- IP adresi ve bağlantı adı validasyonu
- TightVNC yolu ve port ayarı
- Bağımlılık kontrolü ve modern hata bildirimleri
- Yardım sekmesinde gereksinimler ve linkler

## Kurulum
1. TightVNC Viewer (tvnviewer.exe) kurulu olmalı
2. .NET 8.0 Runtime yüklü olmalı
3. Projeyi derleyip çalıştırın

## Kullanım
- Adres defterinden yeni bağlantı ekleyin
- Bağlantı adına ve IP adresine dikkat edin (örn: 192.168.1.100)
- Bağlantıya çift tıklayarak hızlıca bağlanın
- Ayarlar sekmesinden TightVNC yolunu ve portu güncelleyebilirsiniz

## Gereksinimler
- TightVNC Viewer: https://www.tightvnc.com/download.php
- .NET 8.0 Runtime: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

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

## Ayarlar
"Ayarlar" sekmesinden TightVNC Viewer programının yolu, varsayılan VNC bağlantı portunu ve uygulamanın Windows ile otomatik başlatma seçeneği ayarlanabilir. Ayarlar `VNC_Config.json` dosyasında saklanır.

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