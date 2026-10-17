using UnityEngine;

public class RingCheckpoint : MonoBehaviour
{
    private bool isPassed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isPassed) return;

        // Mendeteksi jika yang melewati ring adalah PlayerDrone
        if (other.GetComponentInParent<FPVRacingDrone>() != null)
        {
            isPassed = true;
            GameManager.Instance?.RegisterCheckpointPassed();
            
            // Opsional: Matikan collider atau beri visual ring selesai
            gameObject.SetActive(false); 
        }
    }
}