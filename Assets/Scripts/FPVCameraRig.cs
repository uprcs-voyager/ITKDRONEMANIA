using UnityEngine;

// pasang di Main Camera. jadiin Main Camera sebagai CHILD dari PlayerDrone,
// habis itu atur Position dan Rotation-nya langsung di Transform (bebas, termasuk menoleh kiri/kanan).
// Script ini NGGAK mengubah posisi/rotasi dasar kamera; hanya menambah getaran dan mengatur FOV.
[RequireComponent(typeof(Camera))]
public class FPVCameraRig : MonoBehaviour
{
    [Header("Target (boleh kosong, otomatis dicari dari parent)")]
    [SerializeField] private FPVRacingDrone targetDrone;

    [Header("Lensa")]
    [Tooltip("Near clip kecil supaya tanah dan gate dekat tidak terpotong.")]
    [SerializeField] private float nearClip = 0.05f;

    [Header("Dynamic FOV (FOV Unity = vertikal)")]
    [SerializeField] private float baseFOV = 90f;
    [SerializeField] private float maxFOV = 105f;
    [SerializeField] private float speedForMaxFOV = 100f; // km/jam

    [Header("Vibrasi Motor & Prop Wash")]
    [SerializeField] private float motorVibrationFrequency = 50f;
    [SerializeField] private float motorVibrationAmplitude = 0.003f;
    [SerializeField] private float propWashShakeMultiplier = 0.015f;

    private Camera cam;
    private Vector3 appliedShake = Vector3.zero;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.nearClipPlane = nearClip;
        cam.fieldOfView = baseFOV;

        if (targetDrone == null)
            targetDrone = GetComponentInParent<FPVRacingDrone>();
    }

    private void LateUpdate()
    {
        if (targetDrone == null) return;

        // 1. Getaran motor frekuensi tinggi (makin besar saat throttle tinggi)
        float throttle = targetDrone.CurrentThrottle01;
        float motorShakeX = (Mathf.PerlinNoise(Time.time * motorVibrationFrequency, 0f) - 0.5f) * motorVibrationAmplitude * throttle;
        float motorShakeY = (Mathf.PerlinNoise(0f, Time.time * motorVibrationFrequency) - 0.5f) * motorVibrationAmplitude * throttle;

        // 2. Turbulensi prop wash
        float washFactor = targetDrone.PropWashIntensity;
        float washShakeX = (Mathf.PerlinNoise(Time.time * 18f, 2f) - 0.5f) * propWashShakeMultiplier * washFactor;
        float washShakeY = (Mathf.PerlinNoise(2f, Time.time * 18f) - 0.5f) * propWashShakeMultiplier * washFactor;

        // 3. Terapkan getaran sebagai selisih terhadap getaran frame sebelumnya,
        //    jadi posisi yang diatur atur di Inspector tetap di pake
        Vector3 newShake = new Vector3(motorShakeX + washShakeX, motorShakeY + washShakeY, 0f);
        transform.localPosition += newShake - appliedShake;
        appliedShake = newShake;

        // 4. Dynamic FOV
        float currentSpeed = targetDrone.CurrentSpeedKmh;
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, currentSpeed / speedForMaxFOV);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 6f);
    }
}
