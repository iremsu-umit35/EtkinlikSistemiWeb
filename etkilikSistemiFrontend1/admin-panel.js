const baseUrl = "http://localhost:5231/api";
// Sayfa yüklendiğinde çalışacak ana blok
document.addEventListener("DOMContentLoaded", () => {
    // HTML'deki form elemanlarını al
    const eventForm = document.getElementById("eventForm");
    const submitBtn = document.getElementById("submitBtn");

    let selectedEventId = null;// Güncelleme yapılacak etkinlik varsa onun ID'si burada tutulur

    if (eventForm) {
        eventForm.addEventListener("submit", async (e) => {
            e.preventDefault();// Sayfanın yenilenmesini engelle

            // Formdaki inputlardan etkinlik bilgilerini al
            const etkinlik = {
                Ad: document.getElementById("ad").value,
                Aciklama: document.getElementById("aciklama").value,
                Tur: document.getElementById("tur").value,
                Tarih: document.getElementById("tarih").value,
                Lokasyon: document.getElementById("lokasyon").value,
                Kapasite: parseInt(document.getElementById("kapasite").value),
                BiletFiyati: parseFloat(document.getElementById("biletFiyati").value)
            };
            // Güncelleme mi yapılıyor yoksa yeni ekleme mi?
            if (selectedEventId !== null) {
                // GÜNCELLEME(PUT isteği atılıyor)/////////////////////////////////////////////////////////
                const response = await fetch(`${baseUrl}/Admin/etkinlik-guncelle/${selectedEventId}`, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json" // nesne JSON'a çevrilip gönderiliyor
                    },
                    body: JSON.stringify(etkinlik)
                });

                if (response.ok) {
                    alert("Etkinlik güncellendi.");
                } else {
                    alert("Etkinlik güncellenemedi.");
                }

                selectedEventId = null;  // Güncelleme sonrası her şey sıfırlanır
                submitBtn.textContent = "Etkinlik Ekle";
            } else {
                // EKLEME (POST isteği atılıyor)/////////////////////////////////////////////
                const response = await fetch(`${baseUrl}/Admin/etkinlik-ekle`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(etkinlik)
                });

                if (response.ok) {
                    alert("Etkinlik başarıyla eklendi.");
                } else {
                    alert("Etkinlik eklenemedi.");
                }
            }

            eventForm.reset();
            getEvents(); // Listeyi güncelle
        });
    }

    // Güncelleme için formu dolduran fonksiyonu global yap:
    window.fillFormForUpdate = function(event) {
        // Mevcut etkinlik bilgileri inputlara yerleştirilir
        document.getElementById("ad").value = event.ad;
        document.getElementById("aciklama").value = event.aciklama;
        document.getElementById("tur").value = event.tur;
        document.getElementById("tarih").value = event.tarih.substring(0, 10);
        document.getElementById("lokasyon").value = event.lokasyon;
        document.getElementById("kapasite").value = event.kapasite;
        document.getElementById("biletFiyati").value = event.biletFiyati;

        selectedEventId = event.id;// Hangi etkinlik güncellenecek, onun ID'si set edilir
        submitBtn.textContent = "Etkinliği Güncelle"; // Butonun yazısı değiştirilir
    };


    // Duyuru ekleme//////////////////////////////////////////////////////////////////////////////////
    const announcementForm = document.getElementById("announcementForm");
    if (announcementForm) {
        announcementForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            const duyuru = {
                Baslik: document.getElementById("baslik").value,
                Icerik: document.getElementById("icerik").value
            };
            // POST isteği ile API'ye gönderilir
            const response = await fetch(`${baseUrl}/Admin/duyuru-ekle`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(duyuru)
            });

            if (response.ok) {
                alert("Duyuru başarıyla eklendi.");
                announcementForm.reset();
            } else {
                alert("Duyuru eklenemedi.");
            }
        });
    }
    getAnnouncements()// Sayfa açıldığında duyuruları yükle

});

//onaysızları getirme fonksiyonu//////////////////////////////////////////////////////////////////////////
async function getPendingUsers() {
    const list = document.getElementById("pendingUserList");
    // Liste zaten görünüyorsa tekrar tıklamada gizle
    if (!list.classList.contains("hidden") && list.children.length > 0) {
        list.classList.add("hidden");
        return;
    }

    // API'den onaysız kullanıcıları al
    const response = await fetch(`${baseUrl}/Admin/onaysizlar`);
    const json = await response.json();
    const users = json.$values;// .NET $values içinde gönderiyor
    // Listeyi temizle ve yeniden oluştur
    list.innerHTML = "";
    users.forEach(user => {
        const li = document.createElement("li");
        li.textContent = `ID: ${user.id}, Eposta: ${user.eposta}`;
        const btn = document.createElement("button");
        btn.textContent = "Onayla";
        btn.onclick = () => approveUser(user.id);// Butona tıklanınca onayla
        li.appendChild(btn);
        list.appendChild(li);
    });

    list.classList.remove("hidden");
}



//kullanıcı onaylama fonksiyonu/////////////////////////////////////////
async function approveUser(id) {
    const response = await fetch(`${baseUrl}/Admin/onayla/${id}`, {
        method: "PUT"
    });
    if (response.ok) {
        alert("Kullanıcı onaylandı.");
        getPendingUsers(); // Listeyi güncelle
    } else {
        alert("Onay başarısız.");
    }
}

// TÜM ETKİNLİKLERİ GETİRME FONKSİYONU/////////////////////////////////////////////////////////////////////////
async function getEvents() {
    const list = document.getElementById("eventList");

    // Toggle görünürlük
    if (!list.classList.contains("hidden") && list.children.length > 0) {
        list.classList.add("hidden");
        return;
    }

    const response = await fetch(`${baseUrl}/Admin/etkinlikleri`);
    const json = await response.json();
    const events = json.$values; // ← ÖNEMLİ: Doğru veri dizisine erişiyoruz


    list.innerHTML = "";
    events.forEach(event => {
        const li = document.createElement("li");
        const date = new Date(event.tarih);
        const formattedDate = date.toLocaleDateString("tr-TR");
        li.textContent = `#${event.id} - ${event.ad} (${formattedDate}) - ${event.lokasyon}`;
        list.appendChild(li);

        // Sil butonu
        const deleteBtn = document.createElement("button");
        deleteBtn.textContent = "Sil";
        deleteBtn.style.marginLeft = "10px";
        deleteBtn.onclick = () => deleteEvent(event.id);
        li.appendChild(deleteBtn);


        // Güncelle butonu
        const updateBtn = document.createElement("button");
        updateBtn.textContent = "Güncelle";
        updateBtn.style.marginLeft = "10px";
        updateBtn.onclick = () => fillFormForUpdate(event);
        li.appendChild(updateBtn);

        list.appendChild(li);
    });


    //etkinlik silme fonksiyonu///////////////////////////////////////////
    function deleteEvent(id) {
        if (confirm("Bu etkinliği silmek istediğinize emin misiniz?")) {
            fetch(`${baseUrl}/Admin/etkinlik-sil/${id}`,  {
                method: "DELETE"
            })
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => { throw new Error(text); });
                    }
                    alert("Etkinlik silindi.");
                    getEvents(); // Listeyi güncelle
                })
                .catch(error => {
                    alert("Hata: " + error.message);
                });
        }
    }
    list.classList.remove("hidden");
}

//etkinlik güncelleme
function fillFormForUpdate(event) {
    document.getElementById("ad").value = event.ad;
    document.getElementById("aciklama").value = event.aciklama;
    document.getElementById("tur").value = event.tur;
    document.getElementById("tarih").value = event.tarih.substring(0, 10); // yyyy-MM-dd
    document.getElementById("lokasyon").value = event.lokasyon;
    document.getElementById("kapasite").value = event.kapasite;
    document.getElementById("biletFiyati").value = event.biletFiyati;

    selectedEventId = event.id;

    // Butonun metnini "Güncelle" yap
    document.getElementById("submitBtn").textContent = "Etkinliği Güncelle";
}


// Duyuruları getirme/////////////////////////////////////////////////////////////////////////////////////////////
async function getAnnouncements() {
    const list = document.getElementById("announcementList");

    const response = await fetch(`${baseUrl}/Admin/duyurular`);
    const result = await response.json();

    const announcements = result.$values; // 🔥 DİKKAT BURASI

    console.log(announcements); // Artık array
    announcements.forEach(announcement => {
        const li = document.createElement("li");

        // Başlık ve içerik ekleme
        const title = document.createElement("h3");
        title.textContent = announcement.baslik;

        const content = document.createElement("p");
        content.textContent = announcement.icerik;

        // Yayın durumu butonu
        const publishBtn = document.createElement("button");
        publishBtn.textContent = announcement.isPublished ? "Yayından Kaldır" : "Yayınla";
        publishBtn.style.marginLeft = "10px";
        publishBtn.onclick = () => togglePublishStatus(announcement.id, announcement.isPublished);

        li.appendChild(title);
        li.appendChild(content);
        li.appendChild(publishBtn);

        list.appendChild(li);
    });

    list.classList.remove("hidden"); // Listeyi görünür hale getirme
}


// Yayın durumu değişikliği  //yayın durumunu değiştirir. Butona tıklanınca, duyurunun yayın durumu değiştirilir ve güncellenmiş duyuru listesi tekrar görüntülenir
async function togglePublishStatus(id, currentStatus) {
    const newStatus = !currentStatus;

    const response = await fetch(`${baseUrl}/Admin/duyuru-yayinla/${id}?isPublished=${newStatus}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(newStatus) // sadece true veya false gönderiyoruz
    });

    if (response.ok) {
        alert(newStatus ? "Duyuru yayınlandı." : "Duyuru yayından kaldırıldı.");
        getAnnouncements(); // Listeyi güncelle
    } else {
        alert("Yayın durumu değiştirilemedi.");
    }


}

// HAVA DURUMU GÜNCELLEME BUTONU TIKLANDIĞINDA
document.getElementById("guncelleHavaBtn").addEventListener("click", async () => {
    try {
        const response = await fetch("http://localhost:5231/api/Weather/guncelle-hava-durumu", {
            method: "PUT"
        });

        if (response.ok) {
            const result = await response.text();
            document.getElementById("guncelleSonucu").innerText = result;
        } else {
            document.getElementById("guncelleSonucu").innerText = "Güncelleme başarısız.";
        }
    } catch (error) {
        console.error("Hata:", error);
        document.getElementById("guncelleSonucu").innerText = "Bir hata oluştu.";
    }
});
