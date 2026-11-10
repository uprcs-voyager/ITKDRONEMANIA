//using UnityEngine;

//// pasang di Main Camera. jadiin Main Camera sebagai CHILD dari PlayerDrone,
//// habis itu atur Position dan Rotation-nya langsung di Transform (bebas, termasuk menoleh kiri/kanan).
//// Script ini NGGAK mengubah posisi/rotasi dasar kamera; hanya menambah getaran dan mengatur FOV.
//[RequireComponent(typeof(Camera))]
//public class FPVCameraRig : MonoBehaviour
//{
//    [Header("Target (boleh kosong, otomatis dicari dari parent)")]
//    [SerializeField] private FPVRacingDrone targetDrone;

//    [Header("Lensa")]
//    [Tooltip("Near clip kecil supaya tanah dan gate dekat tidak terpotong.")]
//    [SerializeField] private float nearClip = 0.05f;

//    [Header("Dynamic FOV (FOV Unity = vertikal)")]
//    [SerializeField] private float baseFOV = 90f;
//    [SerializeField] private float maxFOV = 105f;
//    [SerializeField] private float speedForMaxFOV = 100f; // km/jam

//    [Header("Vibrasi Motor & Prop Wash")]
//    [SerializeField] private float motorVibrationFrequency = 50f;
//    [SerializeField] private float motorVibrationAmplitude = 0.003f;
//    [SerializeField] private float propWashShakeMultiplier = 0.015f;


//    [Header("Stabilisasi Pitch (Inspector Toggle)")]
//    [Tooltip("Centang di Inspector untuk mengunci pitch kamera agar tidak tunduk/mendongak saat drone maju/mundur. Kemiringan roll (kiri/kanan) tetap aktif.")]
//    [SerializeField] private bool lockPitch = false;

//    private Camera cam;
//    private Vector3 appliedShake = Vector3.zero;
//    private Vector3 initialLocalEuler;

//    private void Awake()
//    {
//        cam = GetComponent<Camera>();
//        cam.nearClipPlane = nearClip;
//        cam.fieldOfView = baseFOV;

//        // Simpan rotasi lokal awal kamera dari Inspector
//        initialLocalEuler = transform.localEulerAngles;

//        if (targetDrone == null)
//            targetDrone = GetComponentInParent<FPVRacingDrone>();
//    }

//    private void LateUpdate()
//    {
//        if (targetDrone == null) return;

//        if (lockPitch)
//        {
//            // Ambil pitch drone dalam rentang -180 sampai 180 derajat
//            float dronePitch = targetDrone.transform.eulerAngles.x;
//            if (dronePitch > 180f) dronePitch -= 360f;

//            // Kompensasi rotasi X kamera agar sudut pandang horisontal tetap stabil
//            float targetLocalPitch = initialLocalEuler.x - dronePitch;
//            Quaternion targetLocalRot = Quaternion.Euler(targetLocalPitch, initialLocalEuler.y, initialLocalEuler.z);

//            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRot, Time.deltaTime * 20f);
//        }
//        else
//        {
//            // Mode FPV Murni: Kembalikan ke rotasi lokal awal mengikuti bodi drone secara penuh
//            Quaternion defaultLocalRot = Quaternion.Euler(initialLocalEuler);
//            transform.localRotation = Quaternion.Slerp(transform.localRotation, defaultLocalRot, Time.deltaTime * 20f);
//        }

//        // 1. Getaran motor frekuensi tinggi (makin besar saat throttle tinggi)
//        float throttle = targetDrone.CurrentThrottle01;
//        float motorShakeX = (Mathf.PerlinNoise(Time.time * motorVibrationFrequency, 0f) - 0.5f) * motorVibrationAmplitude * throttle;
//        float motorShakeY = (Mathf.PerlinNoise(0f, Time.time * motorVibrationFrequency) - 0.5f) * motorVibrationAmplitude * throttle;

//        // 2. Turbulensi prop wash
//        float washFactor = targetDrone.PropWashIntensity;
//        float washShakeX = (Mathf.PerlinNoise(Time.time * 18f, 2f) - 0.5f) * propWashShakeMultiplier * washFactor;
//        float washShakeY = (Mathf.PerlinNoise(2f, Time.time * 18f) - 0.5f) * propWashShakeMultiplier * washFactor;

//        // 3. Terapkan getaran sebagai selisih terhadap getaran frame sebelumnya,
//        //    jadi posisi yang diatur atur di Inspector tetap di pake
//        Vector3 newShake = new Vector3(motorShakeX + washShakeX, motorShakeY + washShakeY, 0f);
//        transform.localPosition += newShake - appliedShake;
//        appliedShake = newShake;

//        // 4. Dynamic FOV
//        float currentSpeed = targetDrone.CurrentSpeedKmh;
//        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, currentSpeed / speedForMaxFOV);
//        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 6f);
//    }
//}



using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FPVCameraRig : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private FPVRacingDrone targetDrone;

    [Header("FPV Camera Tilt (Uptilt)")]
    [Tooltip("Sudut mendongak kamera terhadap frame drone. Nilai 25 - 35 derajat ideal untuk FPV racing.")]
    [Range(0f, 60f)]
    [SerializeField] private float cameraTiltAngle = 30f;

    [Header("Lensa")]
    [SerializeField] private float nearClip = 0.05f;

    [Header("Dynamic FOV")]
    [SerializeField] private float baseFOV = 90f;
    [SerializeField] private float maxFOV = 105f;
    [SerializeField] private float speedForMaxFOV = 100f; // km/jam

    [Header("Vibrasi Motor & Prop Wash")]
    [SerializeField] private float motorVibrationFrequency = 50f;
    [SerializeField] private float motorVibrationAmplitude = 0.003f;
    [SerializeField] private float propWashShakeMultiplier = 0.015f;

    private Camera cam;
    private Vector3 appliedShake = Vector3.zero;

    public float CameraTiltAngle
    {
        get => cameraTiltAngle;
        set
        {
            cameraTiltAngle = Mathf.Clamp(value, 0f, 60f);
            ApplyCameraTilt();
        }
    }

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.nearClipPlane = nearClip;
        cam.fieldOfView = baseFOV;

        if (targetDrone == null)
            targetDrone = GetComponentInParent<FPVRacingDrone>();

        ApplyCameraTilt();
    }

    private void OnValidate()
    {
        // Memastikan perubahan angka di Inspector langsung mengupdate sudut kamera di Scene View
        ApplyCameraTilt();
    }

    private void ApplyCameraTilt()
    {
        // Gunakan tanda minus (-) agar rotasi X mendongak ke atas (skywards)
        transform.localRotation = Quaternion.Euler(-cameraTiltAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (targetDrone == null) return;

        // 1. Getaran motor frekuensi tinggi
        float throttle = targetDrone.CurrentThrottle01;
        float motorShakeX = (Mathf.PerlinNoise(Time.time * motorVibrationFrequency, 0f) - 0.5f) * motorVibrationAmplitude * throttle;
        float motorShakeY = (Mathf.PerlinNoise(0f, Time.time * motorVibrationFrequency) - 0.5f) * motorVibrationAmplitude * throttle;

        // 2. Turbulensi prop wash
        float washFactor = targetDrone.PropWashIntensity;
        float washShakeX = (Mathf.PerlinNoise(Time.time * 18f, 2f) - 0.5f) * propWashShakeMultiplier * washFactor;
        float washShakeY = (Mathf.PerlinNoise(2f, Time.time * 18f) - 0.5f) * propWashShakeMultiplier * washFactor;

        // 3. Terapkan getaran vibrasi sebagai offset
        Vector3 newShake = new Vector3(motorShakeX + washShakeX, motorShakeY + washShakeY, 0f);
        transform.localPosition += newShake - appliedShake;
        appliedShake = newShake;

        // 4. Dynamic FOV berdasarkan kecepatan
        float currentSpeed = targetDrone.CurrentSpeedKmh;
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, currentSpeed / speedForMaxFOV);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 6f);
    }
}