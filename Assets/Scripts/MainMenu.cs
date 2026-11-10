using TMPro;
using UnityEngine;
using UnityEngine.EventSystems; // Wajib untuk navigasi Gamepad / Xbox Controller
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panel Navigation")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject sessionPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject wipPanel; // Panel untuk fitur On Progress

    [Header("Start Session")]
    [Tooltip("Teks yang menampilkan mode terpilih, contoh: MODE: TIME ATTACK")]
    [SerializeField] private TextMeshProUGUI modeLabel;
    [Tooltip("Nama scene gameplay di Build Profiles.")]
    [SerializeField] private string gameSceneName = "MainScene";

    [Header("Leaderboard")]
    [SerializeField] private TextMeshProUGUI leaderboardText;

    [Header("Settings: Transparansi OSD")]
    [SerializeField] private Slider osdAlphaSlider;
    [SerializeField] private TextMeshProUGUI osdAlphaText;

    [Header("Work In Progress UI")]
    [SerializeField] private TextMeshProUGUI wipTitleText;

    [Header("Controller Focus Defaults (Wajib untuk Xbox Controller)")]
    [Tooltip("Tentukan tombol pertama yang disorot Gamepad saat panel dibuka")]
    [SerializeField] private GameObject mainFirstSelected;
    [SerializeField] private GameObject sessionFirstSelected;
    [SerializeField] private GameObject settingsFirstSelected;
    [SerializeField] private GameObject leaderboardFirstSelected;
    [SerializeField] private GameObject wipFirstSelected;

    [Header("Extra Settings Controls")]
    [SerializeField] private Toggle osdToggle;
    [SerializeField] private Slider timeLimitSlider;
    [SerializeField] private TextMeshProUGUI timeLimitText;

    private GameMode selectedMode = GameMode.TimeAttack;




    private void Start()
    {
        // Memastikan fisika/waktu game berjalan normal saat kembali dari gameplay
        Time.timeScale = 1f;

        SyncOsdAlphaSlider();
        SyncExtraSettings(); 
        UpdateModeLabel();
        ShowMain();
    }

    // ---------------- Navigasi Panel ----------------

    public void ShowMain()
    {
        ShowOnly(mainPanel);
        SetControllerFocus(mainFirstSelected);
    }

    public void ShowSession()
    {
        ShowOnly(sessionPanel);
        SetControllerFocus(sessionFirstSelected);
    }

    public void ShowSettings()
    {
        ShowOnly(settingsPanel);
        SetControllerFocus(settingsFirstSelected);
    }

    public void ShowLeaderboard()
    {
        ShowOnly(leaderboardPanel);
        UpdateLeaderboardDisplay();
        SetControllerFocus(leaderboardFirstSelected);
    }

    // --- Tombol Fitur GDD (On Progress) ---

    public void ShowSkinSelects() => ShowWorkInProgress("SKIN SELECTS");
    public void ShowCollectibles() => ShowWorkInProgress("COLLECTIBLES");
    public void ShowProgression() => ShowWorkInProgress("PROGRESSION");

    private void ShowWorkInProgress(string featureName)
    {
        ShowOnly(wipPanel);
        if (wipTitleText != null)
        {
            wipTitleText.text = $"<b>{featureName}</b>\n\n<size=18><color=#FFCC00>FITUR SEDANG DALAM TAHAP PENGEMBANGAN</color></size>\n<size=14><color=#A0A0A0>(WORK IN PROGRESS)</color></size>";
        }
        SetControllerFocus(wipFirstSelected);
    }

    private void ShowOnly(GameObject target)
    {
        SetActive(mainPanel, target == mainPanel);
        SetActive(sessionPanel, target == sessionPanel);
        SetActive(settingsPanel, target == settingsPanel);
        SetActive(leaderboardPanel, target == leaderboardPanel);
        SetActive(wipPanel, target == wipPanel);
    }

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    // ---------------- Navigasi Gamepad / Xbox Controller ----------------

    /// <summary>
    /// Memaksa EventSystem menyorot tombol pertama saat panel berganti.
    /// Tanpa fungsi ini, D-Pad / Left Stick Gamepad tidak bisa memilih tombol.
    /// </summary>
    private void SetControllerFocus(GameObject targetButton)
    {
        if (EventSystem.current != null && targetButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Reset fokus lama
            EventSystem.current.SetSelectedGameObject(targetButton); // Set fokus baru
        }
    }

    // ---------------- Start Session ----------------

    public void SelectTimeAttack()
    {
        selectedMode = GameMode.TimeAttack;
        UpdateModeLabel();
    }

    public void SelectFreeRoam()
    {
        selectedMode = GameMode.FreeRoam;
        UpdateModeLabel();
    }

    public void StartGame()
    {
        GameManager.SelectedMode = selectedMode;
        SceneManager.LoadScene(gameSceneName);
    }

    private void UpdateModeLabel()
    {
        if (modeLabel == null) return;
        modeLabel.text = selectedMode == GameMode.TimeAttack ? "MODE: TIME ATTACK" : "MODE: FREE ROAM";
    }

    // ---------------- Leaderboard Sync ----------------

    private void UpdateLeaderboardDisplay()
    {
        if (leaderboardText == null) return;

        if (GameManager.LeaderboardData == null || GameManager.LeaderboardData.Count == 0)
        {
            leaderboardText.text = "Belum Ada Catatan Rekor\n<size=12><color=#A0A0A0>(Khusus Time Attack 60s)</color></size>";
            return;
        }

        string displayText = "<b>TOP RECORDS (TIME ATTACK 60S)</b>\n\n";
        for (int i = 0; i < GameManager.LeaderboardData.Count; i++)
        {
            var record = GameManager.LeaderboardData[i];
            displayText += $"#{i + 1}  |  Skor: <b>{record.score}</b> Points  |  Sisa Waktu: <b>{record.timeRemaining:0.00}s</b>  [{record.timestamp}]\n";
        }

        leaderboardText.text = displayText;
    }

    // ---------------- Settings ----------------

    public void OnOSDAlphaChanged(float value)
    {
        float alpha01 = value > 1f ? value / 100f : value;
        alpha01 = Mathf.Clamp(alpha01, 0.1f, 1f); // Batasi transparansi minimal 10% (0.1)

        PlayerPrefs.SetFloat("OSD_Alpha", alpha01);
        PlayerPrefs.Save();

        if (osdAlphaText != null)
        {
            osdAlphaText.text = Mathf.RoundToInt(alpha01 * 100f) + "%";
        }
    }

    private void SyncOsdAlphaSlider()
    {
        float saved = PlayerPrefs.GetFloat("OSD_Alpha", 0.80f);
        if (saved < 0.1f) saved = 0.80f; // Safeguard reset ke 80% jika 0

        if (osdAlphaSlider != null)
        {
            osdAlphaSlider.value = osdAlphaSlider.maxValue > 1f ? saved * 100f : saved;
        }

        if (osdAlphaText != null)
        {
            osdAlphaText.text = Mathf.RoundToInt(saved * 100f) + "%";
        }
    }

    private void SyncExtraSettings()
    {
        // 1. Load OSD Toggle (Default: 1 / Aktif)
        bool isOsdOn = PlayerPrefs.GetInt("OSD_Enabled", 1) == 1;
        if (osdToggle != null) osdToggle.isOn = isOsdOn;

        // 2. Load Time Limit (Default: 60 detik)
        float savedTimeLimit = PlayerPrefs.GetFloat("TimeLimit", 60f);
        if (timeLimitSlider != null)
        {
            timeLimitSlider.value = savedTimeLimit;
            if (timeLimitText != null) timeLimitText.text = $"{savedTimeLimit:0}s";
        }
    }

    // Event Handler Toggle OSD
    public void OnOSDToggleChanged(bool isEnabled)
    {
        PlayerPrefs.SetInt("OSD_Enabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Event Handler Time Limit Slider
    public void OnTimeLimitChanged(float value)
    {
        PlayerPrefs.SetFloat("TimeLimit", value);
        PlayerPrefs.Save();
        if (timeLimitText != null)
        {
            timeLimitText.text = $"{value:0}s";
        }
    }

    // ---------------- Keluar ----------------

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}