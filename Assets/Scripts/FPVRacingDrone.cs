using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Rigidbody))]
public class FPVRacingDrone : MonoBehaviour
{
    public enum ThrottleInputMode
    {
        R2_Trigger,                  // R2 = throttle 0..1 (khusus controller yang R2 nya nggak boolean 1 ato 2)
        ThrottleStick_SpringCentered, // Stik throttle di tengah = hover
        ThrottleStick_Raw             // Bawah = 0% atas = 100% ini susah banger tapi mmm opsi terakhir aja 
    }

    [Header("Skema Kontrol Gamepad")]
    [SerializeField] private ThrottleInputMode throttleMode = ThrottleInputMode.ThrottleStick_SpringCentered;
    [Tooltip("Aktif: stik KIRI = pitch/roll, stik KANAN = throttle (atas/bawah) + yaw (kiri/kanan).\nNonaktif: layout standar Mode 2 (kiri = throttle/yaw, kanan = pitch/roll).")]
    [SerializeField] private bool swapSticks = true;
    [Tooltip("Zona mati stik. Output dihitung ulang dari 0 di luar zona ini, jadi nggak ada lompatan.")]
    [Range(0f, 0.3f)]
    [SerializeField] private float stickDeadband = 0.08f;

    [Header("Keyboard (stik virtual)")]
    [Tooltip("Seberapa cepat tombol mencapai defleksi penuh (per detik). 4 = sekitar 0.25 detik.")]
    [SerializeField] private float keyboardStickResponse = 4f;

    [Header("Propulsi & Air Mode")]
    [Tooltip("Daya dorong maksimum (Newton). ~65N untuk 0.65kg = TWR ~10:1")]
    [SerializeField] private float maxThrust = 65f;
    [Tooltip("Thrust = throttle ^ exponent. 2 mendekati motor asli (hover sekitar 30% throttle, bukan 10%).")]
    [Range(1f, 3f)]
    [SerializeField] private float thrustCurveExponent = 2f;
    [SerializeField] private float motorSpoolSpeed = 22f;
    [Range(0.02f, 0.1f)]
    [SerializeField] private float airModeIdleThrottle = 0.05f;

    [Header("Betaflight Rates: Pitch & Roll")]
    [Tooltip("Sensitivitas pusat stik (Standar Betaflight: 1.0 - 1.4)")]
    [Range(0.5f, 2.5f)]
    [SerializeField] private float rcRatePitchRoll = 1.15f;
    [Tooltip("Faktor lonjakan hiperbolik di ujung stik (Standar: 0.65 - 0.78). turunin kalau ujung stik terasa meledak.")]
    [Range(0f, 0.95f)]
    [SerializeField] private float superRatePitchRoll = 0.72f;
    [Tooltip("Kelembutan stik di area deadzone (Standar: 0.15 - 0.25)")]
    [Range(0f, 1f)]
    [SerializeField] private float rcExpoPitchRoll = 0.18f;

    [Header("Betaflight Rates: Yaw")]
    [Range(0.5f, 2.5f)]
    [SerializeField] private float rcRateYaw = 1.05f;
    [Range(0f, 0.95f)]
    [SerializeField] private float superRateYaw = 0.68f;
    [Range(0f, 1f)]
    [SerializeField] private float rcExpoYaw = 0.15f;

    [Header("Betaflight PID Controller")]
    [SerializeField] private float pGain = 25f;
    [SerializeField] private float iGain = 8f;
    [SerializeField] private float dGain = 0.3f;
    [SerializeField] private float maxIntegralTorque = 1f;

    [Header("TPA (Throttle PID Attenuation)")]
    [Tooltip("Batas persentase throttle di mana P-Gain mulai direduksi")]
    [Range(0.4f, 0.9f)]
    [SerializeField] private float tpaBreakpoint = 0.65f;
    [Tooltip("Persentase reduksi gain saat 100% throttle (0.2 = reduksi 20%)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float tpaRate = 0.25f;

    [Header("Aerodinamika Multi-Sumbu")]
    [SerializeField] private float forwardDragCoeff = 0.03f;
    [SerializeField] private float sideDragCoeff = 0.08f;
    [SerializeField] private float discVerticalDragCoeff = 0.10f;

    [Header("Bantalan Udara Lantai (Ground Effect)")]
    [SerializeField] private float groundEffectDistance = 0.5f;
    [SerializeField] private float groundEffectIntensity = 12f;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Simulasi Turbulensi (Prop Wash)")]
    [SerializeField] private float propWashIntensity = 3.5f;

    [Header("Benturan Fatal")]
    [SerializeField] private float fatalImpactVelocity = 13f;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private float rawThrottleInput = 0f;
    private float yawInput = 0f;
    private Vector2 pitchRollInput = Vector2.zero; // X: Roll, Y: Pitch

    // control nuat keyboard untuk keyboard (kiri: X=yaw, Y=throttle, kanan: X=roll, Y=pitch)
    private Vector2 keyboardLeft = Vector2.zero;
    private Vector2 keyboardRight = Vector2.zero;

    private float currentSmoothedThrottle = 0f;
    private float currentPropWashFactor = 0f;
    private bool isDestroyed = false;

    // Memori internal PID
    private Vector3 rateIntegralError = Vector3.zero;
    private Vector3 lastRateMeasured = Vector3.zero;

    // Interface publik untuk FPVCameraRig, FPV_OSD, dan DroneAudioFeedback
    public float CurrentThrottle01 => currentSmoothedThrottle;
    public float CurrentSpeedKmh => rb != null ? (rb.linearVelocity.magnitude * 3.6f) : 0f;
    public float PropWashIntensity => currentPropWashFactor;
    public Rigidbody DroneRigidbody => rb;

    // Input mentah (buat debug di OSD aowkwkwk)
    public float InputThrottle => rawThrottleInput;
    public float InputYaw => yawInput;
    public float InputPitch => pitchRollInput.y;
    public float InputRoll => pitchRollInput.x;

    /// Throttle (0..1) yang menghasilkan gaya dorong sama dengan berat drone.
    private float HoverThrottle
    {
        get
        {
            float weight = rb.mass * Physics.gravity.magnitude;
            return Mathf.Pow(Mathf.Clamp01(weight / maxThrust), 1f / thrustCurveExponent);
        }
    }

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 50f;
        rb.mass = 0.65f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Update()
    {
        if (isDestroyed) return;
        ReadInputs();
    }

    private void FixedUpdate()
    {
        if (isDestroyed) return;

        UpdateMotorThrust();
        ApplyBetaflightSuperRatesPID();
        ApplyDirectionalAerodynamics();
        ApplyGroundEffect();
        CalculatePropWashTurbulence();
    }

    // =====================================================================
    // INPUT
    // =====================================================================

    private void ReadInputs()
    {
        bool keyboardActive = UpdateKeyboardSticks();
        Gamepad pad = Gamepad.current;

        if (pad != null && !keyboardActive)
        {
            Vector2 left = pad.leftStick.ReadValue();
            Vector2 right = pad.rightStick.ReadValue();

            // Tentukan stik mana yang berperan sebagai "throttle + yaw" dan mana "pitch + roll".
            // swapSticks = true  -> kiri = pitch/roll, kanan = throttle/yaw
            // swapSticks = false -> kiri = throttle/yaw, kanan = pitch/roll (Mode 2 standar)
            Vector2 throttleYawStick = swapSticks ? right : left;
            Vector2 pitchRollStick = swapSticks ? left : right;

            float throttleStickY = ApplyDeadband(throttleYawStick.y, stickDeadband);

            yawInput = ApplyDeadband(throttleYawStick.x, stickDeadband);
            pitchRollInput = new Vector2(
                ApplyDeadband(pitchRollStick.x, stickDeadband),
                ApplyDeadband(pitchRollStick.y, stickDeadband));

            switch (throttleMode)
            {
                case ThrottleInputMode.R2_Trigger:
                    rawThrottleInput = pad.rightTrigger.ReadValue();
                    break;
                case ThrottleInputMode.ThrottleStick_SpringCentered:
                    rawThrottleInput = SpringCenteredThrottle(throttleStickY);
                    break;
                case ThrottleInputMode.ThrottleStick_Raw:
                    rawThrottleInput = Mathf.Clamp01((throttleStickY + 1f) * 0.5f);
                    break;
            }
        }
        else
        {
            // Keyboard (atau nggak ada input sama sekali): stik virtual, tengah = hover
            yawInput = keyboardLeft.x;
            pitchRollInput = keyboardRight;
            rawThrottleInput = SpringCenteredThrottle(keyboardLeft.y);
        }
    }

    /// 
    /// Keyboard dijadikan stik virtual yang bergerak halus menuju defleksi penuh
    /// dan kembali ke tengah saat tombol dilepas.
    /// W/S = throttle, A/D = yaw, Panah atas/bawah = pitch, Panah kiri/kanan = roll.
    /// 
    private bool UpdateKeyboardSticks()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            keyboardLeft = Vector2.zero;
            keyboardRight = Vector2.zero;
            return false;
        }

        Vector2 targetLeft = new Vector2(KeyAxis(kb.dKey, kb.aKey), KeyAxis(kb.wKey, kb.sKey));
        Vector2 targetRight = new Vector2(KeyAxis(kb.rightArrowKey, kb.leftArrowKey), KeyAxis(kb.upArrowKey, kb.downArrowKey));

        float step = keyboardStickResponse * Time.deltaTime;
        keyboardLeft = Vector2.MoveTowards(keyboardLeft, targetLeft, step);
        keyboardRight = Vector2.MoveTowards(keyboardRight, targetRight, step);

        return targetLeft != Vector2.zero || targetRight != Vector2.zero
            || keyboardLeft != Vector2.zero || keyboardRight != Vector2.zero;
    }

    private static float KeyAxis(KeyControl positive, KeyControl negative)
    {
        return (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
    }

    /// Stik tengah = throttle hover, atas = 100%, bawah = 0%.
    private float SpringCenteredThrottle(float stickY)
    {
        float hover = HoverThrottle;
        return stickY >= 0f
            ? Mathf.Lerp(hover, 1f, stickY)
            : Mathf.Lerp(hover, 0f, -stickY);
    }

    private float ApplyDeadband(float value, float deadband)
    {
        float abs = Mathf.Abs(value);
        if (abs <= deadband) return 0f;
        return Mathf.Sign(value) * Mathf.Min((abs - deadband) / (1f - deadband), 1f);
    }

    // ------------------------------------------------------------------
    // FISIKA BACK TO THIS LATER BRO
    // ------------------------------------------------------------------

    private void UpdateMotorThrust()
    {
        float targetThrottle = Mathf.Max(rawThrottleInput, airModeIdleThrottle);

        currentSmoothedThrottle = Mathf.MoveTowards(
            currentSmoothedThrottle,
            targetThrottle,
            motorSpoolSpeed * Time.fixedDeltaTime
        );

        // Gaya dorong motor nyata kira-kira sebanding dengan throttle kuadrat
        float thrustFraction = Mathf.Pow(currentSmoothedThrottle, thrustCurveExponent);
        rb.AddForce(transform.up * (thrustFraction * maxThrust), ForceMode.Force);
    }

    private void ApplyBetaflightSuperRatesPID()
    {
        // 1. Hitung Target Laju Rotasi menggunakan Rumus Betaflight Super Rates Asli (BACK TO THIS LTATWER
        float targetPitchDegPerSec = CalculateBetaflightRate(pitchRollInput.y, rcRatePitchRoll, superRatePitchRoll, rcExpoPitchRoll);
        float targetRollDegPerSec = -CalculateBetaflightRate(pitchRollInput.x, rcRatePitchRoll, superRatePitchRoll, rcExpoPitchRoll);
        float targetYawDegPerSec = CalculateBetaflightRate(yawInput, rcRateYaw, superRateYaw, rcExpoYaw);

        Vector3 targetRateRad = new Vector3(targetPitchDegPerSec, targetYawDegPerSec, targetRollDegPerSec) * Mathf.Deg2Rad;

        // 2. Baca kecepatan putaran sudut aktual dari Gyro
        Vector3 currentRateRad = transform.InverseTransformDirection(rb.angularVelocity);
        Vector3 rateError = targetRateRad - currentRateRad;

        // 3. Akumulasi I-Term
        rateIntegralError += rateError * Time.fixedDeltaTime;
        rateIntegralError = Vector3.ClampMagnitude(rateIntegralError, maxIntegralTorque);

        // 4. D-Term dari pengukuran gyro (bukan dari error) supaya tidak ada derivative kick
        Vector3 rateDerivative = -(currentRateRad - lastRateMeasured) / Time.fixedDeltaTime;
        lastRateMeasured = currentRateRad;

        // 5. Kalkulasi TPA (Throttle PID Attenuation)
        float tpaFactor = 1.0f;
        if (currentSmoothedThrottle > tpaBreakpoint)
        {
            float tpaProgress = (currentSmoothedThrottle - tpaBreakpoint) / (1.0f - tpaBreakpoint);
            tpaFactor = 1.0f - (tpaProgress * tpaRate);
        }

        float effectivePGain = pGain * tpaFactor;
        float effectiveDGain = dGain * tpaFactor;

        Vector3 pidTorque = (rateError * effectivePGain) + (rateIntegralError * iGain) + (rateDerivative * effectiveDGain);
        rb.AddRelativeTorque(pidTorque, ForceMode.Acceleration);
    }

    /// 
    /// Rumus Asli Betaflight Super Rates: Cubic Expo + Hyperbolic Super Rate
    /// 
    private float CalculateBetaflightRate(float stickDeflection, float rcRate, float superRate, float rcExpo)
    {
        if (Mathf.Abs(stickDeflection) < 0.001f) return 0f;

        float absDeflection = Mathf.Abs(stickDeflection);

        float expoCurve = stickDeflection * (Mathf.Pow(absDeflection, 3) * rcExpo + absDeflection * (1f - rcExpo));

        float clampedSuperRate = Mathf.Clamp(superRate, 0f, 0.95f);
        float superFactor = 1f / Mathf.Max(1f - (absDeflection * clampedSuperRate), 0.01f);

        return 200f * rcRate * expoCurve * superFactor;
    }

    private void ApplyDirectionalAerodynamics()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        float dragX = -Mathf.Sign(localVel.x) * (localVel.x * localVel.x) * sideDragCoeff;
        float dragY = -Mathf.Sign(localVel.y) * (localVel.y * localVel.y) * discVerticalDragCoeff;
        float dragZ = -Mathf.Sign(localVel.z) * (localVel.z * localVel.z) * forwardDragCoeff;

        rb.AddForce(transform.TransformDirection(new Vector3(dragX, dragY, dragZ)), ForceMode.Force);
    }

    private void ApplyGroundEffect()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundEffectDistance, groundLayer))
        {
            float proximity = 1f - (hit.distance / groundEffectDistance);
            float cushion = proximity * proximity * groundEffectIntensity * currentSmoothedThrottle;
            rb.AddForce(Vector3.up * cushion, ForceMode.Force);
        }
    }

    private void CalculatePropWashTurbulence()
    {
        float verticalDescentRate = -Vector3.Dot(rb.linearVelocity, transform.up);

        if (verticalDescentRate > 2.5f && currentSmoothedThrottle > 0.15f)
        {
            currentPropWashFactor = Mathf.Clamp01((verticalDescentRate - 2.5f) / 6f) * currentSmoothedThrottle;

            float noiseX = (Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f) * propWashIntensity * currentPropWashFactor;
            float noiseZ = (Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f) * propWashIntensity * currentPropWashFactor;

            rb.AddRelativeTorque(new Vector3(noiseX, 0f, noiseZ), ForceMode.Acceleration);
        }
        else
        {
            currentPropWashFactor = Mathf.MoveTowards(currentPropWashFactor, 0f, Time.fixedDeltaTime * 4f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Jika game sudah tidak aktif atau di mode Free Roam, abaikan tabrakan
        if (GameManager.Instance == null || !GameManager.Instance.IsGameActive) return;

        // Khusus Mode Time Attack: Menabrak objek apapun = KALAH
        if (GameManager.Instance.CurrentMode == GameMode.TimeAttack)
        {
            //Pemicu state kalah di GameManager
            string objectName = collision.gameObject.name;
            GameManager.Instance.OnGameLost($"TABRAKAN DENGAN {objectName.ToUpper()}!");

            //Hentikan pergerakan drone supaya nggak tembus/melayang
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // matiin simulasi fisika
            }
        }
    }

    private void Detonate()
    {
        isDestroyed = true;

        // Jika slot explosionEffectPrefab diisi, efek ledakan bakal muncul.
        // kalo kosong (null), baris ini dilewati tanpa error.
        // belum di isi soalnya belum buat Effect nya awokwkww
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Sembunyikan drone
        gameObject.SetActive(false);
    }
}
