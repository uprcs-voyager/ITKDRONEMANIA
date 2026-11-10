using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DroneAudioController : MonoBehaviour
{
    [Header("Referensi Drone")]
    [SerializeField] private FPVRacingDrone droneRef;

    [Header("Pengaturan Pitch & Volume")]
    [SerializeField] private float minPitch = 0.6f;  // Pitch saat idle (throttle 0%)
    [SerializeField] private float maxPitch = 2.2f;  // Pitch saat throttle 100%
    [SerializeField] private float minVolume = 0.2f; // Volume minimum
    [SerializeField] private float maxVolume = 1.0f; // Volume maksimum

    [Header("--- Pengaturan Suara Warning/Alert ---")]
    [Tooltip("Masukkan file audio alert/beep Anda di sini")]
    [SerializeField] private AudioClip warningClip;
    [Tooltip("Jeda minimal antar bunyi warning (dalam detik) agar tidak tumpang tindih")]
    [SerializeField] private float warningCooldown = 0.5f;
    [SerializeField] private float warningVolume = 1.0f;

    [Header("--- Pengaturan Deteksi Objek (Proximity) ---")] // <--- BARU
    [Tooltip("Jarak (radius) dalam meter saat warning mulai berbunyi")]
    [SerializeField] private float warningRadius = 3.0f;
    [Tooltip("Pilih Layer objek yang akan memicu warning (misal: Obstacle, Default, Walls)")]
    [SerializeField] private LayerMask obstacleLayer;

    // komponen audio
    private AudioSource motorAudio;
    private AudioSource warningAudio;

    // State Warning
    private float nextWarningTime = 0f;

    private void Awake()
    {
        motorAudio = GetComponent<AudioSource>();
        motorAudio.loop = true;
        motorAudio.playOnAwake = false;


        // 1. Setup Suara Warning (Dibuat otomatis sebagai child AudioSource terpisah)
        if (warningClip != null)
        {
            warningAudio = gameObject.AddComponent<AudioSource>();
            warningAudio.playOnAwake = false;
            warningAudio.loop = false;
            warningAudio.spatialBlend = 1.0f; // 3D Audio (mengikuti posisi drone)
            warningAudio.volume = warningVolume;
        }
        else
        {
            Debug.LogWarning("[DroneAudioController] Warning Clip belum diisi di Inspector!", this);
        }

        // Otomatis cari komponen FPVRacingDrone jika belum diisi di Inspector
        if (droneRef == null)
        {
            droneRef = GetComponentInParent<FPVRacingDrone>();
        }
    }


    private void Start()
    {
        // Mulai putar suara mesin saat game dimulai
        if (motorAudio != null && !motorAudio.isPlaying)
        {
            motorAudio.Play();
        }
    }

    private void Update()
    {
        if (droneRef == null || motorAudio == null) return;

        // Pause audio saat game di-pause atau jika permainan sudah selesai
        if (Time.timeScale == 0f || (GameManager.Instance != null && !GameManager.Instance.IsGameActive))
        {
            if (motorAudio.isPlaying) motorAudio.Pause();
            return;
        }

        if (!motorAudio.isPlaying) motorAudio.UnPause();

        // Ambil data throttle dari FPVRacingDrone
        float throttle = droneRef.CurrentThrottle01;

        // Ubah pitch dan volume secara transisi halus
        motorAudio.pitch = Mathf.Lerp(motorAudio.pitch, Mathf.Lerp(minPitch, maxPitch, throttle), Time.deltaTime * 10f);
        motorAudio.volume = Mathf.Lerp(motorAudio.volume, Mathf.Lerp(minVolume, maxVolume, throttle), Time.deltaTime * 10f);

        // 1. Logika Deteksi Objek Terdekat (BARU) // <--- BARU
        // Mengecek apakah ada Collider di dalam bola radius warningRadius
        bool isNearObstacle = Physics.CheckSphere(transform.position, warningRadius, obstacleLayer);

        if (isNearObstacle)
        {
            TriggerWarning();
        }
    }
        // Untuk memutar suara warning
    public void TriggerWarning()
    {
        if (warningAudio == null || warningClip == null) return;

        // Cek cooldown agar suara tidak menumpuk berisik setiap frame
        if (Time.time >= nextWarningTime)
        {
            warningAudio.PlayOneShot(warningClip);
            nextWarningTime = Time.time + warningCooldown;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Mengatur warna garis radius (misalnya kuning)
        Gizmos.color = Color.yellow;

        // Menggambar bola wireframe (garis luar saja) sesuai dengan radius warning
        Gizmos.DrawWireSphere(transform.position, warningRadius);
    }


}



