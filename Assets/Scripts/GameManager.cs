using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public enum GameMode
{
    TimeAttack,
    FreeRoam
}

[System.Serializable]
public class LeaderboardEntry
{
    public float timeRemaining;  // Sisa waktu pas finish
    public int score;           // skor akhir
    public string timestamp;     // Jam pencapaian (misal: 22:45)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static GameMode SelectedMode = GameMode.TimeAttack;

    // list statis supaya data leaderboard nggak hilang pas game nya di restart (scene reload)
    public static List<LeaderboardEntry> LeaderboardData = new List<LeaderboardEntry>();

    [Header("Pengaturan Time Attack")]
    [SerializeField] private float timeLimitSeconds = 60f;       // Batas waktu awal 60 detik
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
    public float TimeLimitSeconds => timeLimitSeconds;
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
        if (!IsGameActive || SelectedMode == GameMode.FreeRoam) return;
        passedCheckpoints++;
        timeRemaining += extraTimePerCheckpoint; // TAMBAH EXTRA TIME
    }

    public void CheckFinishLine()
    {
        if (!IsGameActive || SelectedMode == GameMode.FreeRoam) return;

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

        if (SelectedMode == GameMode.TimeAttack && Mathf.Approximately(timeLimitSeconds, 60f))
        {
            LeaderboardEntry newEntry = new LeaderboardEntry
            {
                timeRemaining = timeRemaining,
                score = FinalScore,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            };

            LeaderboardData.Add(newEntry);

            // urutan skor dari yang tertinggi ke terendah
            LeaderboardData.Sort((a, b) => b.score.CompareTo(a.score));

            // simpan maksimal 10 rekor terbaik aja
            if (LeaderboardData.Count > 10)
            {
                LeaderboardData.RemoveAt(LeaderboardData.Count - 1);
            }
        }
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