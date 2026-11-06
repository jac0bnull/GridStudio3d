# GRID STUDIO

**Windows x64 · 0.2.1-alpha · Unity 6 ile dışa aktarım**

GRID STUDIO, oyun geliştiricilerinin **3B model bulup bir sahne kurmasına ve o sahnenin oyun içi arayüzünü (HUD) tasarlamasına** yarayan bir masaüstü aracıdır. Örneğin bir ev modeli bulabilir, sahnedeki yerini ayarlayabilir, ekranın köşesine bir can çubuğu ekleyebilir ve ikisini birlikte Unity’de açabilirsin.

![GRID STUDIO HUD tasarım ekranında can çubuğu ve radar alanı](docs/hud.png)

## Windows’ta başlat

1. Sürüm dosyalarından **`GRID-STUDIO-0.2.1-alpha-win-x64.zip`** paketini al. Son kullanıcı için gereken dosya budur. Kaynaktan çalıştırmak istersen aşağıdaki geliştirici adımlarını kullan.
2. ZIP’e sağ tıkla, **Tümünü ayıkla** seçeneğini kullan ve çıkan klasörde **`GridStudio.exe`** dosyasını aç. Hazır paket kendi .NET çalışma zamanını içerir. Model aramak ve sahne hazırlamak için Unity kurman gerekmez.
3. Çalışma alanı varsayılan tarayıcında açılır. Sekme açılmazsa konsolda yazan `http://127.0.0.1:…` adresini tarayıcıya kopyala. Bu adres kendi bilgisayarındaki uygulamaya gider. Çalışırken konsol penceresini açık tut.

3B görünüm için **WebGL 2 destekleyen bir tarayıcı**, çevrimiçi katalog ve indirmeler için internet bağlantısı gerekir. Daha önce indirdiğin modellerle çevrimdışı sahne düzenleyebilirsin.

## İlk projen: bir ev, bir sahne, bir can çubuğu

| Adım | Uygulamada yapacağın işlem |
| --- | --- |
| **1. Model bul** | **Marketplace → All models** bölümünde `ev` veya `house` ara. Bir karta tıklayıp sağ panelde modelin kaynağını, üreticisini ve lisansını incele. |
| **2. Sahneye ekle** | Desteklenen bir modelde **Download model**, indirme bitince **Open in scene lab** düğmesine tıkla. Modeli ve gerekli dokuları birlikte almak için uygulamadaki indirmeyi kullan. |
| **3. Yerleştir** | **Scene lab** içinde modeli seç. **Move**, **Rotate** ve **Scale** ile konumunu, dönüşünü ve boyutunu ayarla. Sol fareyle sürükleyerek çevresinde dön, tekerlekle yaklaş. Modeli gözden kaybedersen **Frame** ile kadraja al. **Save scene** ile kaydet. |
| **4. HUD ekle** | **HUD designer** içinde **Health bar** öğesini tasarım alanına sürükle. Konumunu ve boyutunu ayarla. **Preview** ile nasıl göründüğüne bak, ardından **Save layout** ile kaydet. |
| **5. Dışa aktar** | Sağ üstten **Dışa Aktar → Sahne + modeller + HUD** seç. Yalnızca arayüzü kullanacaksan **Yalnızca HUD** seçeneğini kullan. Oluşturulan ZIP’in tam yolu alt durum çubuğunda görünür. |

### Unity’de sonucu gör

1. Temiz bir **Unity 6** projesi aç. Model içeren sahneler için Package Manager’dan `com.unity.cloud.gltfast` paketini ekle. Aktarım, Unity 6000.6.0f1 ve glTFast 6.19.0 ile sınandı.
2. **Dışa aktardığın ZIP’i** Unity projesinin köküne çıkar. Doğru klasör yapısı `Assets/GridStudioExport/` şeklindedir.
3. Unity içe aktarmayı bitirince **Tools → GRID STUDIO → Open Exported Scene** seç. Oluşturulan sahneyi kaydet ve **Play** düğmesine bas. Evi ve tasarladığın can çubuğunu birlikte görürsün.

**Yalnızca HUD aktardıysan** sahnendeki bir GameObject’e `GridHud` bileşenini ekleyip Play’e bas. Can çubuğunu oyununa bağlamak için bu bileşenin `SetValue` metoduna öğenin kimliğini ve 0–1 arasında bir değer ver. Örneğin `0.42f`, çubuğu %42 dolu gösterir. Radar ve envanter alanlarının içeriğini oyunun sağlar.

## Model araması nasıl çalışır?

- **Çok dilli arama:** Kendi dilinde bir nesne adı veya kısa tarif yazabilirsin. Örneğin `itfaiye aracı`, `camion de pompiers` veya `宇宙船`. Uygulama yerel sözlükte bulunmayan arama metnini İngilizce karşılığını almak için MyMemory’ye gönderir. Çeviri yapılamazsa özgün metinle devam eder.
- **Önce ücretsiz modeller:** Arama Poly Haven ve Icosa Gallery kataloglarını kapsar. İkisinde de eşleşme bulunmazsa Synty Store’da aynı sorgu aranır. Mağaza kartında ürünün fiyatı veya fiyat aralığı kendi para birimiyle görünür. Lisansı ve son fiyatı ürün sayfasında kontrol edebilirsin.
- **Kendi dosyanla çalış:** **Scene lab → Import local model** ile GLB, glTF, OBJ veya FBX dosyanı açabilirsin. Harici dosyaları olan glTF modellerinde `.gltf`, `.bin` ve dokuları birlikte seç. Bu sürümde doğrulanan kullanım statik modellerdir. FBX animasyonları ve bazı malzemeler ek işlem gerektirebilir.

## Dosyalarımı nerede bulurum?

| İçerik | Konum |
| --- | --- |
| İndirilen modeller | Varsayılan olarak Windows’taki **İndirilenler → GRID STUDIO Models** klasöründe. Uygulamada **Marketplace → Installed** altında görünür. |
| Sahne ve HUD kayıtları | `%LOCALAPPDATA%\GridStudio\documents`. Tekrar açmak için **Reopen scene** veya **Reopen layout** kullan. |
| Unity’ye aktarılacak ZIP’ler | `%LOCALAPPDATA%\GridStudio\exports`. Son oluşturulan dosyanın tam yolu durum çubuğunda da gösterilir. |

**Ayarlar → Model indirme konumu → Klasör seç…** ile yeni indirmeler için başka bir disk seçebilirsin. Önceki modeller eski klasörleri erişilebilir olduğu sürece kütüphanede kalır. **Ayarlar → Görünüm** bölümünde Stüdyo, Kar/Kış, Yaz ve Cadılar Bayramı temaları bulunur. Uygulama teması ve oyuna aktarılan HUD renkleri ayrı ayarlanır.

Her çalışma alanı bir sahne ve bir HUD düzeni saklar. Ayrı bir proje açmak için uygulama klasöründeki PowerShell penceresinde örneğin `.\GridStudio.exe --library "D:\Oyunlar\UzayIstasyonu"` çalıştırabilirsin.

## Kaynaktan çalıştırma ve test

Windows x64, **.NET 10 SDK** ve **Node.js 24** ile depo kökünde çalıştır:

```powershell
npm ci
npm run assets
dotnet publish -c Release -o artifacts/build
.\artifacts\build\GridStudio.exe
```

Testler için uygulamayı kapattıktan sonra:

```powershell
dotnet run --project tests/Contracts/Contracts.csproj -c Release
npx playwright install chromium --only-shell
npm run test:ui
node tests/multilingual.mjs
```

Tarayıcı testleri canlı katalogları kullanır ve internet bağlantısı gerektirir. Paylaşılabilir Windows ZIP’ini üretmek için `.\scripts\package.ps1` çalıştır. Paket `artifacts` klasörüne yazılır.

## Lisans

GRID STUDIO’nun kendi kodu MIT lisanslıdır. İndirdiğin modellerin ve dokuların kullanım koşullarını üreticileri belirler. Oyunu veya sahne paketini paylaşırken her modelin lisansını ve atıf gereksinimlerini kontrol et. [MIT lisansı](LICENSE) · [Üçüncü taraf lisansları](THIRD-PARTY.md) · [Test edilen özellikler ve sürüm sınırları](docs/ACCEPTANCE.md)
