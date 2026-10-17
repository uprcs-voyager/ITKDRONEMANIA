using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameMode
{
    TimeAttack,
    FreeRoam
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static GameMode SelectedMode = GameMode.TimeAttack;

    [Header("Pengaturan Time Attack")]
    [SerializeField] private float timeLimitSeconds = 45f;       // Batas waktu awal
    [SerializeField] private float extraTimePerCheckpoint = 5f;  // Bonus waktu per ring (+5 detik)
    [SerializeField] private int scorePerSecondRemaining = 100; 

    private float timeRemaining;
    private int totalCheckpoints;
    private int passedCheckpoints;

    public bool IsGameActive { get; private set; } = true;
    public bool IsWon { get; private set; } = false;
    public string FailReason { get; private set; } = "";
    public int FinalScore { get; private set; } = 0;

    public GameMode CurrentMode => SelectedMode;
    public float TimeRemaining => Mathf.Max(0f, timeRemaining);
    public int TotalCheckpoints => totalCheckpoints;
    public int PassedCheckpoints => passedCheckpoints;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        timeRemaining = timeLimitSeconds;
        // Hitung otomatis semua objek checkpoint yang ada tag "Checkpoint" nya di scene
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        FinishLine finish = FindAnyObjectByType<FinishLine>();

        if (SelectedMode == GameMode.FreeRoam)
        {
            // ngehilangin semua ring checkpoint
            foreach (var chk in checkpoints)
            {
                chk.SetActive(false);
            }

            //ngehilangin garis finih
            if (finish != null)
            {
                finish.gameObject.SetActive(false);
            }

            totalCheckpoints = 0;
        }
        else
        {
            totalCheckpoints = checkpoints.Length;
        }
    }

    public void SetTimeLimit(float seconds)
    {
        timeLimitSeconds = seconds;
        timeRemaining = seconds; // Mengubah batas waktu sekaligus mengupdate sisa waktu berjalan
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
            return;
        }

        if (!IsGameActive) return;

        // Waktu hanya berkurang pada mode Time Attack
        if (SelectedMode == GameMode.TimeAttack)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                OnGameLost("WAKTU HABIS!");
            }
        }
    }

    public void RegisterCheckpointPassed()
    {
        if (!IsGameActive) return;
        passedCheckpoints++;
        timeRemaining += extraTimePerCheckpoint; // TAMBAH EXTRA TIME
    }

    public void CheckFinishLine()
    {
        if (!IsGameActive) return;

        if (passedCheckpoints >= totalCheckpoints)
        {
            OnGameWon();
        }
        else
        {
            OnGameLost("FINISH GAGAL! Belum melewati semua Ring Checkpoint!");
        }
    }

    public void OnGameWon()
    {
        if (SelectedMode == GameMode.FreeRoam) return;

        IsGameActive = false;
        IsWon = true;
        FinalScore = Mathf.RoundToInt(timeRemaining * scorePerSecondRemaining);
    }

    public void OnGameLost(string reason)
    {
        IsGameActive = false;
        IsWon = false;
        FailReason = reason;
        FinalScore = 0;
    }

    public void SetModeAndStart(GameMode mode)
    {
        SelectedMode = mode;
        RestartGame();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}