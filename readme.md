# ITK Drone Mania

ITK Drone Mania adalah sebuah game bergenre simulasi drone dengan sudut pandang orang pertama (First-Person View). Objektif utama dalam game ini adalah pemain harus mampu mencapai garis finish secepat mungkin dalam batasan waktu yang telah ditentukan, tanpa mengenai apapun dan melewati semua ring checkpoint yang ada untuk memenangkan permainan. Dengan laboratorium terpadu ITK 1  yang menjadi arena utama permainan, game ini dapat menjadi suatu pengalaman unik dalam mengeksplorasi laboratorium ITK 1 dengan sudut pandang drone.

Proyek Akhir IPPL

- **Video presentasi:** : 
- **Engine:** Unity 6 (6000.6.6f1) | **Bahasa:** C# | **Platform:** Windows PC
- **Pembuat:** Muhamad Radyt Iksan Pratama (11241048)

> Prototype UTS. Arena yang dipakai saat ini adalah arena testing (greybox). Arena interior Laboratorium Terpadu 1 ITK masih dalam tahap pengembangan.

## Status Fitur (mengacu pada GDD)
>Selesai disini memiliki arti bahwa fitur sudah dapat berfungsi, Selesai tidak berarti bahwa **tampilan** UI/fitur sudah dalam kondisi final, tampilan UI/Fitur dapat berubah dalam tahap pengembangan game untuk memaksimalkan nilai astetika

| No | Fitur | Status | Catatan |
|---|---|---|---|
| 1 | Time Attack Mode | Selesai | Ring checkpoint sudah ditetapkan dan setiap kali player memasuki ring checkpoint maka player tersebut mendapatkan tambahan waktu sebanyak 5 detik, finish line sudah dibuat dalam bentuk lempengan hijau besar, timer sudah dibuat. Fungsi Kalah jika menabrak, waktu habis, atau ada ring terlewat sudah dapat dijalankan.  |
| 2 | Free Roam Mode | selesai sebagian | Mode Free Roam sudah dapat dimainkan, namun fitur collectibles dan progression (yang merupakan fitur extra) yang terhubung ke mode ini masih dalam tahap pengembangan |
| 3 | Customizable Time attack, time limit | selesai | Limit waktu pada mode time attack sudah dapat di kustomasisai melalui Pause > Settings > Slider Time Limit Value |
| 4 | Personal LeaderBoard | Selesai  | Personal Leaderboard sudah dapat menampilkan skor tertinggi yang didapatkan oleh pemain pada mode time attack. Personal Leaderboard hanya akan mencatat session time attack yang memiliki time limit 60s |
| 5 | Kondisi Menang | selesai sebagian | Kondisi menang sudah dapat dicapai saat player memainkan mode time attack dan dapat melalui garis finish tanpa kehabisan sisa waktu waktu yang ada dan ring checkpoint sudah dilewati semua, kondisi menang untuk mode free roam masih dalam tahap pengembangan dikarenakan hal tersebut berkaitan dengan fitur collectibles dan progression (fitur extra) |
| 6 | Kondisi kalah  |  selesai  | Kondisi kalah untuk mode time attack sudah dapat ter-trigger jika pemain menabrak, waktu habis, atau ada ring terlewat |
| 7 | Session Select Option | Selesai | Pemain sudah dapat berpindah antara mode game time attack dan free roam dengan memanfaatkan session select option |
| 8 | Personal Leaderboard Screen | selesai | pemain dapat melihat skor tertinggi mereka pada mode time attack (leaderboard hanya akan mencatat data time attack mode khusus untuk time limit 60s) |
| 9 | Settings Screen | 90% | Toggle OSD dan slider time limit cuztomaztion sudah dapat digunakan, pengaturan terhadap transparansi OSD drone masih dalam tahap pengembangan |
| 10 | Drone UI & OSD | 90% | Reticle, Speed, throttle, time left, ring checkpoint passed sudah tampil, alert peringatan benturan masih dalam tahap pengembangan |
| 11 | teks menang dan kalah | Selesai | teks hasil menang/kalah sederhana yang menampilkan informasi kalah(sisa waktu, jumlah ring yang dilewati, dll) dan menang(total skor, sisa waktu, dll) . sudah dapat tampil |
| 12 | Pause Screen | 90% | Resume, Retry, Settings, Change Session, Personal Leaderboard sudah dapat digunakan. Namun, saat ini hanya dapat di akses dengan control keyboard.  |
| 13 | Overall Drone Control | Selesai | Fisika Rigidbody, rate ala Betaflight dan PID, gamepad dan keyboard. |
| 14 | UI Menang dan Kalah | Belum | masih dalam tahap pengembangan |
| 15 | Drone Skin | Belum | masih dalam tahap pengembangan |
| 16 | SFX hum, explosion, alert, UI_Interact | Belum | masih akan dikembangkan  |
| 17 | Game Menu | Belum | Game menu masih dalam tahap pengembangan |
| 18 | BGM (game, victory, lose) | Belum | masih akan dikembangkan |
| 19 | SFX dan BGM lainnya | Belum | masih akan dikembangkan  |
| 20 | Progression System | Belum | masih akan dikembangkan  |
| 21 | Collectibles | Belum | masih akan dikembangkan  |


**Catatan:** Saat Play di unity, jika drone mengenai objek apapun (selain dinding finish dan ring di mode time attack) akan memicu "no cameras rendering" di mode time attack dan free roam  

## Cara Menjalankan (kepada siapapun, TOLONG JALANIN JIKA HANYA MEMANG DIPERLUKAN, karena saya takut git lfs nya limit )

1. Install Unity Hub dan Editor sesuai versi di atas.
2. Clone repo ini, lalu buka foldernya lewat Unity Hub (**Add > Add project from disk**).
3. Buka scene `Assets/Scenes/SampleScene` lalu tekan **Play**. Klik di dalam Game view supaya input masuk.

## Kontrol



| Aksi | Gamepad (xbox) | Keyboard |
|---|---|---|
| Throttle | R2 | W / S |
| Yaw | Right Stick (Horizontal) | A / D |
| Pitch | Left Stick (Vertical) | Panah atas / bawah |
| Roll | Left Stick (Horizontal) | Panah kiri / kanan |
| Restart cepat | X (still under development) | R |
| Pause | Start (still under development) | ESC |



## Kendala dan Rencana


- Rencana: Rencana pengembangan selanjutnya akan difokuskan pada fitur-fitur dan aset utama game yang belum selesai. Serta membuat input contoller (Xbox) dapat berinteraksi dengan elemen-elemen UI