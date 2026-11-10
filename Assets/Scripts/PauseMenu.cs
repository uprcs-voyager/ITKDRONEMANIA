using UnityEngine;
using UnityEngine.SceneManagement; // buat reload scene (Retry)
using UnityEngine.UI;
using TMPro;                        // Buat TextMeshPro ini buat kata kata di menunya itu 

public class PauseMenu : MonoBehaviour
{
    [Header("UI Containers")]
    public GameObject pausecontainer;
    public GameObject settingscontainer;
    public GameObject sessioncontainer;
    public GameObject personalleaderboardcontainer;
    [SerializeField] private GameObject wincontainer;
    [SerializeField] private GameObject failcontainer;

    [Header("Post-Win UI Elements")]
    [SerializeField] private TMP_Text winTimeText;
    [SerializeField] private TMP_Text winScoreText;
    [SerializeField] private TMP_Text winAvgSpeedText;

    [Header("Post-Mortem UI Elements")]
    [SerializeField] private TMP_Text failReasonText;
    [SerializeField] private TMP_Text failImpactSpeedText;
    [SerializeField] private TMP_Text failAvgSpeedText;

    [Header("Leaderboard Display")]
    public TextMeshProUGUI leaderboardContentText;

    [Header("Settings Controls")]
    public FPV_OSD fpvOSD;
    public GameObject osdCanvas;
    public Slider timeLimitSlider;
    public Toggle osdToggle;
    public TextMeshProUGUI timeLimitText;

    [Header("OSD Settings")]
    public Slider osdTransparencySlider;
    public TextMeshProUGUI osdTransparencyText;

    private bool hasHandledGameOver = false;

    private void Update()
    {
        // 1. Jika permainan berakhir (Kalah/Menang), kunci input ESC dan tampilkan UI Game Over
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            if (!hasHandledGameOver)
            {
                HandleGameOverUI();
            }
            return; // Menghentikan eksekusi di sini agar tombol ESC tidak bisa membuka pause menu
        }

        // 2. Kontrol Input Tombol ESC (Hanya aktif selama sesi penerbangan berlangsung)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingscontainer != null && settingscontainer.activeSelf)
            {
                backbutton();
            }
            else if (sessioncontainer != null && sessioncontainer.activeSelf)
            {
                backbutton();
            }
            else if (personalleaderboardcontainer != null && personalleaderboardcontainer.activeSelf)
            {
                backbutton();
            }
            else if (pausecontainer.activeSelf)
            {
                resumebutton();
            }
            else
            {
                pausecontainer.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    private void Start()
    {

        if (wincontainer != null) wincontainer.SetActive(false);
        if (failcontainer != null) failcontainer.SetActive(false);

        //ngeload dan sinkronkan nilai Transparansi OSD
        float savedAlpha = PlayerPrefs.GetFloat("OSD_Alpha", 0.80f);

        if (osdTransparencySlider != null)
        {
            float sliderValue = osdTransparencySlider.maxValue > 1f ? savedAlpha * 100f : savedAlpha;
            osdTransparencySlider.value = sliderValue;
        }

        if (osdTransparencyText != null)
        {
            osdTransparencyText.text = Mathf.RoundToInt(savedAlpha * 100f) + "%";
        }

        if (fpvOSD != null)
        {
            fpvOSD.SetOSDAlpha(savedAlpha);
        }

        // 2. Load & Sync OSD Toggle
        bool isOsdEnabled = PlayerPrefs.GetInt("OSD_Enabled", 1) == 1;
        if (osdToggle != null)
        {
            osdToggle.isOn = isOsdEnabled;
        }
        if (fpvOSD != null)
        {
            fpvOSD.enabled = isOsdEnabled;
        }

        // 3. Load & Sync Time Limit
        float savedTimeLimit = PlayerPrefs.GetFloat("TimeLimit", 60f);
        if (timeLimitSlider != null)
        {
            timeLimitSlider.value = savedTimeLimit;
        }
        if (timeLimitText != null)
        {
            timeLimitText.text = "Time Limit: " + savedTimeLimit.ToString("0") + " s";
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetTimeLimit(savedTimeLimit);
        }
    }

    private void HandleGameOverUI()
    {
        hasHandledGameOver = true;

        // Tutup menu pause biasa jika sedang aktif
        if (pausecontainer != null) pausecontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(false);
        if (personalleaderboardcontainer != null) personalleaderboardcontainer.SetActive(false);

        // Hentikan fisika game saat game over
        Time.timeScale = 0f;

        if (GameManager.Instance.IsWon)
        {
            // Tampilkan Panel Post-Win
            if (wincontainer != null) wincontainer.SetActive(true);

            if (winTimeText != null) 
                winTimeText.text = $"Waktu Tempuh: {GameManager.Instance.TimeSpent:F2}s";
            
            if (winScoreText != null) 
                winScoreText.text = $"Skor Akhir: {GameManager.Instance.FinalScore}";
            
            if (winAvgSpeedText != null) 
                winAvgSpeedText.text = $"Rata-rata Kecepatan: {GameManager.Instance.AverageSpeedKmh:F1} km/jam";
        }
        else
        {
            // Tampilkan Panel Post-Mortem
            if (failcontainer != null) failcontainer.SetActive(true);

            if (failReasonText != null) 
                failReasonText.text = $"{GameManager.Instance.FailReason}";

            if (failImpactSpeedText != null)
            {
                // Kecepatan benturan hanya tampil jika kekalahan karena tabrakan
                if (GameManager.Instance.ImpactSpeedKmh > 0f)
                {
                    failImpactSpeedText.gameObject.SetActive(true);
                    failImpactSpeedText.text = $"{GameManager.Instance.ImpactSpeedKmh:F1} km/jam";
                }
                else
                {
                    failImpactSpeedText.gameObject.SetActive(false);
                }
            }

            if (failAvgSpeedText != null) 
                failAvgSpeedText.text = $"{GameManager.Instance.AverageSpeedKmh:F1} km/jam";
        }
    }

    public void resumebutton() {
        pausecontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(false);
        if (personalleaderboardcontainer != null) personalleaderboardcontainer.SetActive(false);
        Time.timeScale = 1f;
    }

    public void retrybutton()
    {
        Time.timeScale = 1f; // Selalu kembaliin timeScale ke 1 sebelum reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void settingsbutton()
    {
        pausecontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(true);
        if (personalleaderboardcontainer != null) personalleaderboardcontainer.SetActive(false);
    }



    public void sessionbutton()
    {
        pausecontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (personalleaderboardcontainer != null) personalleaderboardcontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(true);
    }

    public void leaderboardbutton()
    {
        if (pausecontainer != null) pausecontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(false);

        // Matikan win & fail container agar tidak menumpuk dengan leaderboard
        if (wincontainer != null) wincontainer.SetActive(false);
        if (failcontainer != null) failcontainer.SetActive(false);

        if (personalleaderboardcontainer != null)
        {
            personalleaderboardcontainer.SetActive(true);
            UpdateLeaderboardDisplay();
        }
    }

    public void mainmenubutton()
    {
        // Best Practice: Selalu kembalikan timeScale ke 1f sebelum berpindah scene.
        // Jika tidak, waktu pada scene MainMenu akan ikut beku (freeze) karena timeScale 0f dari state pause.
        Time.timeScale = 1f;

        // Memuat scene MainMenu berdasarkan nama scene di Build Profiles
        SceneManager.LoadScene("MainMenu");
    }


    // SEETINGS MENU BUTTON AND CONTROLLER
    public void backbutton()
    {
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(false);
        if (personalleaderboardcontainer != null) personalleaderboardcontainer.SetActive(false);

        // Jika permainan sudah selesai (Win/Fail), kembali ke panel Post-Win / Post-Mortem
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            if (GameManager.Instance.IsWon && wincontainer != null)
                wincontainer.SetActive(true);
            else if (!GameManager.Instance.IsWon && failcontainer != null)
                failcontainer.SetActive(true);
        }
        else
        {
            // Jika sesi biasa (Pause), kembali ke Pause Menu
            if (pausecontainer != null) pausecontainer.SetActive(true);
        }
    }

    public void UpdateLeaderboardDisplay()
    {
        if (leaderboardContentText == null) return;

        if (GameManager.LeaderboardData == null || GameManager.LeaderboardData.Count == 0)
        {
            leaderboardContentText.text = "Belum Ada Catatan Rekor\n<size=12><color=#A0A0A0>(Khusus Time Attack 60s)</color></size>";
            return;
        }

        string displayText = "<b>TOP RECORDS (TIME ATTACK 60S)</b>\n\n";
        for (int i = 0; i < GameManager.LeaderboardData.Count; i++)
        {
            var record = GameManager.LeaderboardData[i];
            displayText += $"#{i + 1}  |  Skor: <b>{record.score}</b> Points  |  Sisa Waktu: <b>{record.timeRemaining:0.00}s</b>  [{record.timestamp}]\n";
        }

        leaderboardContentText.text = displayText;
    }

    // HANDLER PEMILIHAN MODE
    public void SelectTimeAttack()
    {
        resumebutton();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetModeAndStart(GameMode.TimeAttack);
        }
    }

    public void SelectFreeRoam()
    {
        resumebutton();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetModeAndStart(GameMode.FreeRoam);
        }
    }

    public void ToggleOSD(bool isEnabled)
    {
        if (fpvOSD != null)
        {
            fpvOSD.enabled = isEnabled; // Mematikan atau menyalakan fungsi OSD (buat milih OSD nya mau dikasi liat apa enggak)
        }
        PlayerPrefs.SetInt("OSD_Enabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnOSDTransparencySliderChanged(float value)
    {
        // kalo slider bernilai 0 - 100, konversi ke skala 0.0 - 1.0
        float alpha01 = value > 1f ? value / 100f : value;

        if (fpvOSD != null)
        {
            fpvOSD.SetOSDAlpha(alpha01);
        }

        if (osdTransparencyText != null)
        {
            osdTransparencyText.text = Mathf.RoundToInt(alpha01 * 100f) + "%";
        }
    }

    public void OnTimeLimitSliderChanged(float value)
    {
        if (timeLimitText != null)
        {
            timeLimitText.text = "Time Limit: " + value.ToString("0") + " s";
        }

        // Ngirim nilai dari Slider ke GameManager (custom time)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetTimeLimit(value);
        }
        PlayerPrefs.SetFloat("TimeLimit", value);
        PlayerPrefs.Save();
    }



   

    //public void leaderboardbutton()
    //{

    //}



}
