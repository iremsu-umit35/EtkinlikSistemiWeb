// DOM içeriği tamamen yüklendiğinde bu fonksiyon çalıştırılır
document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("loginForm");

    // Eğer zaten giriş yaptıysanız, kullanıcıyı yönlendir
    const token = localStorage.getItem("token");
    if (token) {
        window.location.href = "user-home.html";  // Veya admin paneli yönlendirmesi
    }

    /////////////////////////////////////
    //     // GİRİŞ FORMU (LOGIN)
    //     ///////////////////////////////////
    if (loginForm) {
        loginForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const eposta = document.getElementById("eposta").value;
            const sifre = document.getElementById("sifre").value;
            const output = document.getElementById("sonuc");

            try {
                // API'ye giriş isteği gönder
                const response = await fetch("http://localhost:5231/api/Auth/login", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        Eposta: eposta,
                        Sifre: sifre
                    })
                });

                const result = await response.json();
               // alert(JSON.stringify(result));  // debug için doğru

                if (response.ok) {
                    // Gelen verileri kontrol et (özellikle token bazen gelmiyor)
                    if (result.userId) localStorage.setItem("userId", result.userId);
                    if (result.rol) localStorage.setItem("rol", result.rol);
                    if (result.token) localStorage.setItem("token", result.token);

                    // Şifre değişimi gerekiyorsa hemen yönlendir ve return
                    if (result.requiresPasswordChange === true) {
                        window.location.href = "change-password.html";
                        return; // kodun devam etmesini engeller
                    }

                    // Şifre değişimine gerek yoksa yönlendir
                    output.textContent = "Giriş başarılı. Yönlendiriliyorsunuz...";
                    output.style.color = "green";

                    if (result.rol === "admin") {
                        window.location.href = "admin-panel.html";
                    } else {
                        window.location.href = "user-home.html";
                    }

                } else {
                    // Giriş başarısızsa mesaj göster
                    output.textContent = result.message || "Giriş başarısız.";
                    output.style.color = "red";
                }

            } catch (err) {
                // Sunucuya ulaşılamadıysa veya hata varsa
                console.error(err);
                output.textContent = "Sunucu hatası veya geçersiz cevap.";
                output.style.color = "red";
            }
        });
    }

    ///////////////////////////////////
    // ŞİFRE DEĞİŞTİRME FORMU
    ///////////////////////////////////
    const changeForm = document.getElementById("changePasswordForm");
    if (changeForm) {
        changeForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const email = document.getElementById("email").value;
            const oldPassword = document.getElementById("oldPassword").value;
            const newPassword = document.getElementById("newPassword").value;
            const confirmPassword = document.getElementById("confirmPassword").value;
            const result = document.getElementById("result");
            // E-posta boşsa uyarı ver
            if (!email) {
                result.textContent = "Lütfen e-posta adresinizi girin.";
                result.style.color = "red";
                return;
            }
            // Yeni şifreler uyuşmuyorsa uyarı ver
            if (newPassword !== confirmPassword) {
                result.textContent = "Yeni şifreler uyuşmuyor.";
                result.style.color = "red";
                return;
            }
            // Şifre değiştirme isteği gönder
            const response = await fetch("http://localhost:5231/api/Auth/change-password", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    eposta: email,
                    eskiSifre: oldPassword,
                    yeniSifre: newPassword
                })
            });

            const data = await response.json();

            if (response.ok) {
                result.textContent = "Şifre başarıyla değiştirildi. Giriş sayfasına yönlendiriliyorsunuz...";
                result.style.color = "green";
                setTimeout(() => window.location.href = "login.html", 2000);
            } else {
                result.textContent = data.message || "Hata oluştu.";
                result.style.color = "red";
            }
        });
    }

    //
    //     ///////////////////////////////////
    //     // KAYIT FORMU (REGISTER)
    //     ///////////////////////////////////
    const registerForm = document.getElementById("registerForm");
    if (registerForm) {
        registerForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const eposta = document.getElementById("eposta").value;
            const sifre = document.getElementById("sifre").value;
            const sifreTekrar = document.getElementById("sifreTekrar").value;
            const output = document.getElementById("registerResult");
            // Şifreler uyuşmuyorsa uyarı ver
            if (sifre !== sifreTekrar) {
                output.textContent = "Şifreler uyuşmuyor.";
                output.style.color = "red";
                return;
            }
            // Kayıt isteği gönder
            const response = await fetch("http://localhost:5231/api/Auth/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    Eposta: eposta,
                    Sifre: sifre,
                    Rol: "kullanici"
                })
            });

            const sonuc = document.getElementById("registerResult");

            if (response.ok) {
                sonuc.textContent = "Kayıt başarılı. Lütfen hesabınızın onaylanmasını bekleyiniz.";
                sonuc.style.color = "green";
                setTimeout(() => {
                    window.location.href = "login.html";
                }, 2000);
            } else {
                const resultText = await response.text();
                sonuc.textContent = resultText || "Kayıt sırasında hata oluştu.";
                sonuc.style.color = "red";
            }
        });
    }
});
