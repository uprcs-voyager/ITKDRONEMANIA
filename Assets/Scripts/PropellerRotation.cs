using UnityEngine;

public class PropellerRotation : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private FPVRacingDrone droneRef;

    [Header("Pengaturan Rotasi")]
    [Tooltip("Kecepatan putaran maksimum (derajat per detik) saat throttle 100%")]
    [SerializeField] private float maxRotationSpeed = 3500f;

    [Tooltip("Kecepatan putaran saat drone idle (throttle 0%)")]
    [SerializeField] private float idleRotationSpeed = 150f;

    [Tooltip("Seberapa cepat baling-baling berakselerasi/berdeselerasi")]
    [SerializeField] private float accelerationSmoothness = 5f;

    [Header("Arah Putaran")]
    [Tooltip("Centang jika baling-baling berputar searah jarum jam (Clockwise)")]
    [SerializeField] private bool spinClockwise = true;

    private float currentSpeed = 0f;

    private void Awake()
    {
        // Otomatis cari komponen FPVRacingDrone di parent (induk) jika belum diisi
        if (droneRef == null)
        {
            droneRef = GetComponentInParent<FPVRacingDrone>();
        }
    }

    private void Update()
    {
        if (droneRef == null) return;

        // --- Cek Pause / Game Over kalo pause berarti baling-balingnya juga harus ikut stop dong---
        if (Time.timeScale == 0f || (GameManager.Instance != null && !GameManager.Instance.IsGameActive))
        {
            return; // Hentikan perhitungan, baling-baling membeku
        }

        // 1. Ambil nilai throttle dari script utama drone
        float throttle = droneRef.CurrentThrottle01;

        // 2. Tentukan target kecepatan putaran
        float targetSpeed = Mathf.Lerp(idleRotationSpeed, maxRotationSpeed, throttle);

        // 3. Transisi kecepatan secara halus (simulasi motor spool-up)
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * accelerationSmoothness);

        // 4. Tentukan arah putaran (1 untuk CW, -1 untuk CCW)
        float direction = spinClockwise ? 1f : -1f;

        // 5. Putar objek pada sumbu Y lokal (atas/bawah)
        transform.Rotate(Vector3.up, currentSpeed * direction * Time.deltaTime, Space.Self);
    }
}