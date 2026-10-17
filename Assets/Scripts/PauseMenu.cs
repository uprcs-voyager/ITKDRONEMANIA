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

    [Header("Settings Controls")]
    public FPV_OSD fpvOSD;
    public GameObject osdCanvas;
    public Slider timeLimitSlider;
    public TextMeshProUGUI timeLimitText;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Kalo panel setting lagi kebuka, tutup settings dan terus kembali ke  pause menu
            if (settingscontainer != null && settingscontainer.activeSelf)
            {
                backbutton();
            }

            else if (sessioncontainer != null && sessioncontainer.activeSelf)
            {
                backbutton();
            }

            // kalo yang kebuka pause menu, tutup (Resume)
            else if (pausecontainer.activeSelf)
            {
                resumebutton();
            }
            // kalo lagi main, buka pause menu
            else
            {
                pausecontainer.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }



    public void resumebutton() {
        pausecontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(false);
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
    }

    public void sessionbutton()
    {
        pausecontainer.SetActive(false);
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(true);
    }


    // SEETINGS MENU BUTTON AND CONTROLLER
    public void backbutton()
    {
        if (settingscontainer != null) settingscontainer.SetActive(false);
        if (sessioncontainer != null) sessioncontainer.SetActive(false);
        pausecontainer.SetActive(true);
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
    }



   

    //public void leaderboardbutton()
    //{

    //}



}
