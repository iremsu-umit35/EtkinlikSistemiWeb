
const apiUrl = "http://localhost:5231/api/Event/tum";
const cartApiUrl = 'http://localhost:5231/api/Cart';
const ticketApiUrl = 'http://localhost:5231/api/Ticket';

// Sayfalama ve sepet verisi için değişkenler
let currentPage = 1;
const eventsPerPage = 5;
let cart = [];

// Sayfa yüklendiğinde çalışacak ana fonksiyon/////////////////////////////////////////////////////////////////////////////////
document.addEventListener('DOMContentLoaded', () => {

    // Token cookie'den alınır
    const token = document.cookie
        .split('; ')
        .find(row => row.startsWith('AuthToken='))
        ?.split('=')[1];

    if (!token) {
        // window.location.href = 'login.html';
    }

    loadMoreEvents(); // Sayfa yüklendiğinde etkinlikleri yükle
    getAnnouncements();//duyurukar için//////////////////////////////////////
    havaDurumuGetir(); //hava durumu için

    //  Kullanıcı ID'si localStorage'dan alınır
    const kullaniciId = localStorage.getItem("userId");
    if (kullaniciId) {
        updateCartDisplay(kullaniciId);  // Sepet görünümü güncellenir
    }

    //  Ödeme butonları için event listener'lar
    const payCartBtn = document.getElementById("pay-cart-btn");
    if (payCartBtn) {
        payCartBtn.addEventListener("click", () => {
            document.getElementById("payment-modal").style.display = "block";
        });
    }

    const closePaymentModalBtn = document.getElementById("close-payment-modal");
    if (closePaymentModalBtn) {
        closePaymentModalBtn.addEventListener("click", closePaymentModal);
    }

    const confirmPaymentBtn = document.getElementById("confirm-payment-btn");
    if (confirmPaymentBtn) {
        confirmPaymentBtn.addEventListener("click", makePayment);
    }
});

// Kullanıcı çıkışı
function cikisYap() {
    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    localStorage.removeItem("rol");
    window.location.href = "login.html";
}
/*
//  Kullanıcının biletlerini getirir ve ekranda alert ile gösterir/////////////////////////////////////////////////////////////////
async function kullaniciBiletleriniGetir() {
    const token = localStorage.getItem('token');
    if (!token) return alert("Giriş yapmanız gerekiyor.");

    try {
        const response = await fetch(`${ticketApiUrl}/kullanici`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!response.ok) throw new Error("Bilet bilgileri alınamadı.");

        const biletler = await response.json();
        let biletListesi = "🎟 Biletleriniz:\n";
        biletler.forEach(bilet => {
            biletListesi += `Etkinlik: ${bilet.etkinlik.ad}, Tarih: ${new Date(bilet.etkinlik.tarih).toLocaleDateString()}, Fiyat: ${bilet.fiyat} TL\n`;
        });
        alert(biletListesi);
    } catch (error) {
        console.error(error);
        alert("Bilet bilgileri alınamadı.");
    }
}
*/

//  Etkinlikleri listeleyen fonksiyon/////////////////////////////////////////////////////////////////////////////////
function loadMoreEvents(reset = false) {
    // Filtreleme sıfırlanıyorsa ilk sayfaya dön ve listeyi temizle
   if (reset) {
        currentPage = 1;
        document.getElementById('event-list').innerHTML = '';
    }

    //  Filtre değerleri alınır
    const arama = document.getElementById('arama').value.toLowerCase();
    const tur = document.getElementById('tur').value;
    const tarih = document.getElementById('tarih').value;
    const sehir = document.getElementById('sehir').value;
    const minFiyat = parseFloat(document.getElementById('minFiyat').value) || 0;
    const maxFiyat = parseFloat(document.getElementById('maxFiyat').value) || Infinity;

    // 🎉 Etkinlikleri API'den çek
    fetch(apiUrl)
        .then(response => response.json())
        .then(data => {
            console.log("API'den gelen veri:", data);
            const etkinlikler = data.$values;

            // Hatalı veri kontrolü
            if (!Array.isArray(etkinlikler)) {
                console.error("Beklenen dizi formatında veri alınamadı:", data);
                alert("Veri hatası. Lütfen tekrar deneyin.");
                return;
            }

            const listContainer = document.getElementById('event-list');

            //  Etkinlikleri filtrele
            const filteredEvents = etkinlikler.filter(event => {
                const adMatch = event.ad.toLowerCase().includes(arama);
                const turMatch = tur ? event.tur === tur : true;
                const tarihMatch = tarih ? new Date(event.tarih).toISOString().split('T')[0] === tarih : true;
                const sehirMatch = sehir ? event.lokasyon === sehir : true;
                const fiyatMatch = event.biletFiyati >= minFiyat && event.biletFiyati <= maxFiyat;
                return adMatch && turMatch && tarihMatch && sehirMatch && fiyatMatch;
            });

            // Sayfalama
            const startIndex = (currentPage - 1) * eventsPerPage;
            const endIndex = startIndex + eventsPerPage;
            const eventsToDisplay = filteredEvents.slice(startIndex, endIndex);

            //  Etkinlik kartlarını DOM'a ekle
            eventsToDisplay.forEach(event => {
                const card = document.createElement('div');
                card.className = 'event-card';
                card.innerHTML = `
                    <h2>${event.ad}</h2>
                    <p><strong>${event.aciklama}</strong></p>
                    <p>Tarih: ${new Date(event.tarih).toLocaleDateString()}</p>
                    <p>Şehir: ${event.lokasyon}</p>
                    <p>Tür: ${event.tur}</p>
                    <p>Fiyat: ${event.biletFiyati} ₺</p>
                    <p>Kalan: ${event.kalanKapasite}</p>
                    <p>Hava Durumu: ${event.havaDurumu}</p>
                    <p>Etkinlik Gerçekleşecek mi: ${event.planlanabilirMi ? "Evet" : "Hayır"}</p>
                    <button onclick="addToCart(${event.id})">Sepete Ekle 🛒</button>
                `;
                listContainer.appendChild(card);
            });

            // "Daha fazla yükle" butonunun görünürlüğünü ayarla
            document.getElementById('load-more').style.display = (endIndex < filteredEvents.length) ? 'block' : 'none';
            currentPage++;
        })
        .catch(error => {
            console.error('Hata:', error);
            alert("Bir hata oluştu. Lütfen tekrar deneyin.");
        });
}

// 🎛 Filtrele butonu////////////////////////////////////////////////////////////////////////////////////////////////////
function filtrele() {
    loadMoreEvents(true);
}


//  Sepete ürün ekleme işlemi//////////////////////////////////////////////////////////////////////////////////////////////////////////
async function addToCart(eventId) {
    const token = localStorage.getItem("token");
    const kullaniciId = localStorage.getItem("userId");

    if (!token || !kullaniciId) {
        alert("Lütfen giriş yapın");
        window.location.href = "login.html";
        return;
    }

    try {
        console.log("📤 Sepete ekleme isteği gönderiliyor...");
        const response = await fetch(
            `${cartApiUrl}/ekle?kullaniciId=${kullaniciId}&etkinlikId=${eventId}&adet=1`,
            {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                }
            }
        );

        const result = await response.json();
        console.log("📥 Sepete ekleme yanıtı:", result);

        if (!response.ok) {
            throw new Error(result.message || "Sunucudan hata döndü.");
        }

        alert(result.message || "Sepete eklendi.");


        console.log("🔄 Sepet gösterimi güncelleniyor...");
        await updateCartDisplay(kullaniciId);// Sepeti güncelle
    } catch (error) {
        console.error("🚨 Hata oluştu:", error.message);
        alert("Hata: " + error.message);
    }
}

//  Sepeti güncelleyen fonksiyon//////////////////////////////////////////////////////////////////////////////////////////////
async function updateCartDisplay() {
    const kullaniciId = localStorage.getItem("userId");
    const token = localStorage.getItem("token");

    if (!kullaniciId || !token) return;

    try {
        const response = await fetch(`${cartApiUrl}/${kullaniciId}`, {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) throw new Error("Sepet alınamadı");

        const data = await response.json();

        console.log("🛒 Sepet verisi:", data);  // Debug için

        const cartItemsElement = document.getElementById("cart-items");
        const cartTotalElement = document.getElementById("cart-total");
        const payCartBtn = document.getElementById("pay-cart-btn");

        cartItemsElement.innerHTML = "";

        const itemsArray = data.items?.$values || [];

        if (itemsArray.length === 0) {
            cartItemsElement.innerHTML = "<li>Sepetiniz Boş</li>";
            cartTotalElement.textContent = "0";
            payCartBtn.style.display = "none";
        } else {
            let total = 0;
            itemsArray.forEach(item => {
                const li = document.createElement("li");
                li.textContent = `${item.etkinlik.ad} (${item.adet} adet): ${item.toplamFiyat} ₺`;
                cartItemsElement.appendChild(li);
                total += item.toplamFiyat;
            });

            cartTotalElement.textContent = total.toFixed(2);
            payCartBtn.style.display = "inline-block";
        }
    } catch (err) {
        console.error("🚨 Sepet güncelleme hatası:", err);
    }
}


// ❌ Ödeme modalını kapatır///////////////////////////////////////////////////////////////////////
function closePaymentModal() {
    document.getElementById("payment-modal").style.display = "none";
}

// ✅ Sepetteki ürünleri ödeme işlemi///////////////////////////////////////////////////////////////////
async function makePayment() {
    const token = localStorage.getItem("token");
    const userId = localStorage.getItem("userId");
    const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked').value;

    if (!token || !userId) {
        alert("Lütfen giriş yapın.");
        return;
    }

    try {
        const response = await fetch(`${cartApiUrl}/odeme/${userId}?odemeYontemi=${paymentMethod}`, {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            const errorResult = await response.json();
            throw new Error(errorResult?.message || "Ödeme işlemi başarısız.");
        }

        alert("🎉 Ödeme başarılı! Biletleriniz eklendi.");
        closePaymentModal();
        document.getElementById("pay-cart-btn").style.display = "none";
        document.getElementById("cart-items").innerHTML = "<li>Sepetiniz boş.</li>";
        document.getElementById("cart-total").textContent = 0;
        //kullaniciBiletleriniGetir();

    } catch (error) {
        alert("Ödeme başarısız: " + error.message);
        console.error("Ödeme hatası:", error);
    }
}


//duyuruları gösterme///////////////////////////////////////////////////////////////////////////////////////////
async function getAnnouncements() {
    try {
        const response = await fetch("http://localhost:5231/api/Admin/yayindaki-duyurular");
        const data = await response.json();

        const announcements = data.$values || data;
        const duyuruContainer = document.getElementById("duyuruContainer");
        duyuruContainer.innerHTML = "";

        announcements.forEach(duyuru => {
            const div = document.createElement("div");
            div.classList.add("duyuru");

            div.innerHTML = `
                <h3>${duyuru.baslik}</h3>
                <p>${duyuru.icerik}</p>
                <small>${new Date(duyuru.tarih).toLocaleString()}</small>
            `;

            duyuruContainer.appendChild(div);
        });

    } catch (error) {
        console.error("Duyurular alınamadı:", error);
    }
}

/////////////////////////////biletler için///////////////////////////////////////
/*
async function getUserTickets() {
    const token = localStorage.getItem("token");
    const userId = localStorage.getItem("userId");

    try {
        const response = await fetch("http://localhost:5231/api/Ticket/kullanici", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        if (!response.ok) {
            throw new Error("Biletler alınamadı.");
        }

        const tickets = await response.json();
        console.log("Biletler:", tickets);

        // Paneli görünür yap
        document.getElementById("ticket-panel").style.display = "block";

        displayTickets(tickets);
    } catch (error) {
        console.error("Biletleri alırken hata:", error);
    }
}

function displayTickets(tickets) {
    const ticketContainer = document.getElementById("ticketContainer");
    ticketContainer.innerHTML = "";

    const ticketList = tickets.$values || tickets;

    if (ticketList.length === 0) {
        ticketContainer.innerHTML = "<p>Henüz bir biletiniz yok.</p>";
        return;
    }

    ticketList.forEach(ticket => {
        const div = document.createElement("div");
        div.classList.add("ticket-card");
        div.innerHTML = `
            <h3>${ticket.etkinlik.ad}</h3>
            <p><strong>Tarih:</strong> ${new Date(ticket.etkinlik.tarih).toLocaleDateString()}</p>
            <p><strong>Fiyat:</strong> ${ticket.fiyat} ₺</p>
            <p><strong>Ödeme Yöntemi:</strong> ${ticket.odemeYontemi}</p>
            <p><strong>Alım Tarihi:</strong> ${new Date(ticket.alimTarihi).toLocaleString()}</p>
        `;
        ticketContainer.appendChild(div);
    });
}
*/
//hava durumu getirme için ////////////////////////////////////////////////////////////////////////////////////////////////////////
async function havaDurumuGetir() {
    const sehir = document.getElementById("sehirInput").value.trim();
    const panel = document.getElementById("havaBilgisi");
    const ikon = document.getElementById("havaIkon");

    if (!sehir) {
        panel.textContent = "Lütfen bir şehir girin.";
        ikon.style.display = "none";
        return;
    }

    panel.textContent = "Yükleniyor...";
    ikon.style.display = "none";

    try {
        const response = await fetch(`http://localhost:5231/api/Weather/${sehir}`);
        const data = await response.json();

        if (response.ok) {
            panel.textContent = `${data.city} için hava durumu: ${data.weather}`;

            // Küçük harfe çeviriyoruz karşılaştırma için
            const durum = data.weather.toLowerCase();

            // Hava durumu ikon eşlemeleri
            const ikonlar = {
                "açık": "fas fa-sun",
                "güneşli": "fas fa-sun",
                "kapalı": "fas fa-cloud",
                "bulutlu": "fas fa-cloud",
                "parçalı bulutlu": "fas fa-cloud-sun",
                "yağmurlu": "fas fa-cloud-showers-heavy",
                "sağanak": "fas fa-cloud-rain",
                "kar": "fas fa-snowflake",
                "sisli": "fas fa-smog",
            };

            const ikonSinifi = ikonlar[durum] || "fas fa-question"; // eşleşmezse soru işareti
            ikon.className = ikonSinifi;
            ikon.style.display = "inline-block";
        } else {
            panel.textContent = "Hava durumu alınamadı.";
            ikon.style.display = "none";
        }
    } catch (error) {
        panel.textContent = "Sunucu hatası oluştu.";
        ikon.style.display = "none";
        console.error(error);
    }
}

