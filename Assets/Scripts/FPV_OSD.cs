using UnityEngine;

// ini di pasang di objek yang sama dengan Camera (Main Camera).
[RequireComponent(typeof(Camera))]


public class FPV_OSD : MonoBehaviour
{
    [SerializeField] private FPVRacingDrone droneRef;
    [SerializeField] private Color osdColor = new Color(0.1f, 1f, 0.2f, 0.85f); // Warna hijau OSD standar
    [Tooltip("Tampilkan nilai input stik di kiri bawah (berguna untuk mengecek gamepad).")]
    [SerializeField] private Color warningColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    [SerializeField] private bool showInputDebug = true;

    [Header("Font")]
    [Tooltip("Font khusus OSD ")]
    [SerializeField] private Font osdFont;

    [Header("Proximity Warning Alert")]
    [Tooltip("Masukkan gambar PNG warning di sini")]
    [SerializeField] private Texture2D warningIcon;

    [Tooltip("Jarak deteksi dasar (dalam meter)")]
    [SerializeField] private float baseWarningDistance = 3f;

    //[Tooltip("Jarak deteksi (dalam meter)")]
    //[SerializeField] private float warningDistance = 1.5f;

    [Tooltip("Tambahan jarak berdasarkan kecepatan. (Misal 0.1 = tiap 10 km/h, jarak deteksi nambah 1 meter)")]
    [SerializeField] private float speedDistanceMultiplier = 0.08f;

    [Tooltip("Kecepatan kedap-kedip")]
    [SerializeField] private float blinkSpeed = 8f;

    [Tooltip("Layer mask objek yang memicu warning. Pilih 'Everything' atau buat custom layer")]
    [SerializeField] private LayerMask warningLayerMask = ~0;


    [Header("Warning UI Settings")]
    [Tooltip("Ukuran gambar warning di layar (pixel)")]
    [SerializeField] private float warningIconSize = 100f;

    [Tooltip("Jarak gambar dari ujung kanan atas layar (pixel)")]
    [SerializeField] private float warningPadding = 30f;

    [Tooltip("Centang jika ingin abaikan warning saat di lantai (opsional)")]
    [SerializeField] private bool ignoreFloor = true;

    private bool isWarningActive = false;
    private float currentBlinkAlpha = 1f;
    private float blinkTimer = 1f;

    private GUIStyle textStyle;
    private GUIStyle bannerStyle;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        float savedAlpha = PlayerPrefs.GetFloat("OSD_Alpha", 0.80f);
        osdColor.a = savedAlpha;
        // Safeguard: Jika terlanjur bernilai 0, reset ke default 0.80f agar OSD tidak hilang
        if (savedAlpha < 0.1f) savedAlpha = 0.80f;
        osdColor.a = savedAlpha;
    }

    private void Start()
    {
        // Load status Toggle OSD (Default: 1 / Aktif)
        bool isOsdEnabled = PlayerPrefs.GetInt("OSD_Enabled", 1) == 1;
        this.enabled = isOsdEnabled;

        // Pastikan transparansi tidak 0 saat Start
        float savedAlpha = PlayerPrefs.GetFloat("OSD_Alpha", 0.80f);
        if (savedAlpha < 0.1f) savedAlpha = 0.80f;
        osdColor.a = savedAlpha;
    }



    private void Update()
    {
        // Jangan jalankan jika game tidak aktif atau pause
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (Time.timeScale == 0f) return;

        // Deteksi collider di sekitar drone (menggunakan posisi drone)
        Vector3 dronePosition = droneRef != null ? droneRef.transform.position : transform.position;

        // Hitung jarak deteksi dinamis (Semakin cepat drone, semakin jauh radar warning)
        float currentSpeed = droneRef != null ? droneRef.CurrentSpeedKmh : 0f;
        float dynamicWarningDistance = baseWarningDistance + (currentSpeed * speedDistanceMultiplier);

        // Mengecek objek dalam radius tertentu
        Collider[] hitColliders = Physics.OverlapSphere(dronePosition, baseWarningDistance, warningLayerMask);  
        isWarningActive = false;

        foreach (var hit in hitColliders)
        {
            // 1. Abaikan jika objeknya adalah drone itu sendiri
            if (droneRef != null && (hit.gameObject == droneRef.gameObject || hit.transform.IsChildOf(droneRef.transform)))
                continue;

            // 2. Abaikan jika objek memiliki script RingCheckpoint atau FinishLine
            if (hit.GetComponentInParent<RingCheckpoint>() != null) continue;
            if (hit.GetComponentInParent<FinishLine>() != null) continue;

            // 3. (Opsional) Abaikan lantai jika terlalu mengganggu
            // if (ignoreFloor && hit.gameObject.CompareTag("Floor")) continue;

            // Jika lolos filter, berarti ada objek rintangan/dinding di dekat drone
            isWarningActive = true;
            break;
        }

        // Hitung animasi kedap-kedip menggunakan Sinus (nilai antara 0 dan 1)
        // Animasi Kedap-kedip
        if (isWarningActive)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            // PingPong menghasilkan nilai bolak-balik 0 -> 1 -> 0
            currentBlinkAlpha = Mathf.PingPong(blinkTimer, 1f);
        }
        else
        {
            // Reset timer ke 1 agar saat warning muncul lagi, gambarnya langsung terlihat penuh (Alpha 1)
            blinkTimer = 1f;
            currentBlinkAlpha = 1f;
        }
    }



    private void OnGUI()
    {
        if (droneRef == null || cam == null) return;
        if (Event.current.type != EventType.Repaint) return;
        if (Time.timeScale == 0f) return;

        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            //textStyle.normal.textColor = osdColor;
        }

        if (bannerStyle == null)
        {
            bannerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }




        Color currentWarningColor = new Color(warningColor.r, warningColor.g, warningColor.b, osdColor.a);
        float screenW = Screen.width;
        float screenH = Screen.height;
        Vector2 center = new Vector2(screenW * 0.5f, screenH * 0.5f);



        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {

            //if (bannerStyle == null)
            //{
            //    bannerStyle = new GUIStyle(GUI.skin.label)
            //    {
            //        fontStyle = FontStyle.Bold,
            //        alignment = TextAnchor.MiddleCenter,
            //        // Reset padding & margin bawaan supaya nggak buat offset aneh
            //        padding = new RectOffset(0, 0, 0, 0),
            //        margin = new RectOffset(0, 0, 0, 0)
            //    };
            //}


            //if (GameManager.Instance.IsWon)
            //{
            //    bannerStyle.normal.textColor = osdColor;
            //    GUI.Label(new Rect(0, center.y - 85, screenW, 40), "MENANGGGGGGG", bannerStyle);
            //    bannerStyle.fontSize = 15;
            //    GUI.Label(new Rect(0, center.y - 40, screenW, 30), $"FINAL SCORE: {GameManager.Instance.FinalScore} PTS", bannerStyle);
            //    GUI.Label(new Rect(0, center.y - 10, screenW, 30), $"TIME LEFT: {GameManager.Instance.TimeRemaining:0.00}s", bannerStyle);
            //}
            //else
            //{
            //    bannerStyle.normal.textColor = warningColor;
         
            //    GUI.Label(new Rect(0, center.y - 30, screenW, 40), $"MISSION FAILED\n[{GameManager.Instance.FailReason}]", bannerStyle);
            //}

            bannerStyle.fontSize = 15;
            bannerStyle.fontSize = 14;
            return;
        }

        GUI.color = osdColor;
        textStyle.normal.textColor = osdColor;


        //Crosshair tengah
        DrawRect(new Rect(center.x - 12, center.y - 1, 8, 2));
        DrawRect(new Rect(center.x + 4, center.y - 1, 8, 2));
        DrawRect(new Rect(center.x - 1, center.y - 12, 2, 8));
        DrawRect(new Rect(center.x - 1, center.y + 4, 2, 8));

        //  Garis cakrawala (artificial horizon)
        Transform camT = cam.transform;

        // Pitch kamera: positif = kamera menatap ke atas
        float pitchDeg = Mathf.Asin(Mathf.Clamp(camT.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        // Roll kamera: positif = miring ke kanan (sayap kanan turun)
        float rollDeg = Mathf.Atan2(-camT.right.y, camT.up.y) * Mathf.Rad2Deg;

        // Posisi horizon di layar mengikuti proyeksi kamera (FOV Unity = vertikal)
        float halfFovRad = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float pitchRad = Mathf.Clamp(pitchDeg, -80f, 80f) * Mathf.Deg2Rad;
        float horizonOffset = Mathf.Tan(pitchRad) / Mathf.Tan(halfFovRad) * (screenH * 0.5f);
        float horizonY = center.y + horizonOffset; // kamera menatap ke atas -> horizon turun di layar

        Matrix4x4 originalMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(-rollDeg, center); // dunia berputar berlawanan dengan roll kamera

        DrawRect(new Rect(center.x - 140, horizonY - 1, 90, 2));
        DrawRect(new Rect(center.x + 50, horizonY - 1, 90, 2));

        GUI.matrix = originalMatrix;

        // TAMPILKAN WARNING ICON DI KANAN ATAS
        // ==========================================
        if (isWarningActive && warningIcon != null)
        {
            Color originalGuiColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, currentBlinkAlpha);

            // Ukuran dan padding sekarang diambil dari Inspector
            Rect iconRect = new Rect(screenW - warningIconSize - warningPadding, warningPadding, warningIconSize, warningIconSize);
            GUI.DrawTexture(iconRect, warningIcon, ScaleMode.ScaleToFit);

            GUI.color = originalGuiColor;
        }




        // 3. Telemetri
        string speedText = $"SPD: {droneRef.CurrentSpeedKmh:0.0} KM/H";
        string throttleText = $"THR: {droneRef.CurrentThrottle01 * 100f:0}%";
        string modeText = (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.FreeRoam)
    ? "MODE: FREE ROAM"
    : "MODE: Time Attack";

        GUI.Label(new Rect(30, screenH - 60, 220, 25), speedText, textStyle);
        GUI.Label(new Rect(30, screenH - 40, 220, 25), modeText, textStyle);
        GUI.Label(new Rect(screenW - 140, screenH - 50, 120, 25), throttleText, textStyle);


        if (GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.TimeAttack)
        {
            float time = GameManager.Instance.TimeRemaining;
            int mins = Mathf.FloorToInt(time / 60f);
            int secs = Mathf.FloorToInt(time % 60f);
            int fraction = Mathf.FloorToInt((time * 100f) % 100f);

            // Warna merah jika waktu tersisa < 10 detik
            if (time <= 10f) textStyle.normal.textColor = warningColor;

            string timeText = $"TIME: {mins:00}:{secs:00}.{fraction:00}";
            string chkText = $"RING: {GameManager.Instance.PassedCheckpoints}/{GameManager.Instance.TotalCheckpoints}";

            GUI.Label(new Rect(center.x - 70, 20, 180, 25), timeText, textStyle);
            textStyle.normal.textColor = osdColor; // Reset warna
            GUI.Label(new Rect(center.x - 50, 42, 180, 25), chkText, textStyle);
        }


        if (showInputDebug)
        {
            string inputText = $"IN  T {droneRef.InputThrottle:0.00}  Y {droneRef.InputYaw:0.00}  P {droneRef.InputPitch:0.00}  R {droneRef.InputRoll:0.00}";
            GUI.Label(new Rect(30, screenH - 85, 460, 25), inputText, textStyle);
        }

        // Bar throttle vertikal di sisi kanan
        float barHeight = 100f;
        float barWidth = 6f;
        float currentBarFill = barHeight * droneRef.CurrentThrottle01;
        DrawRect(new Rect(screenW - 30, screenH * 0.5f - (barHeight * 0.5f), barWidth, barHeight));
        DrawRect(new Rect(screenW - 32, (screenH * 0.5f + (barHeight * 0.5f)) - currentBarFill, barWidth + 4, 3));
    }

    private void DrawRect(Rect rect)
    {
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }

    public void SetOSDAlpha(float alpha01)
    {
        // ngeclamp nilai supaya berada di rentang 0.0f sampai 1.0f
        osdColor.a = Mathf.Clamp01(alpha01);

        // ngeupdate warna textStyle kalo sudah ter inisialisasi
        if (textStyle != null)
        {
            textStyle.normal.textColor = osdColor;
        }
        PlayerPrefs.SetFloat("OSD_Alpha", osdColor.a);
        PlayerPrefs.Save();

    }
}
