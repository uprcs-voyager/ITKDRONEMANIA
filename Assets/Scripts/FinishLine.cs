using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<FPVRacingDrone>() != null)
        {
            GameManager.Instance?.CheckFinishLine();
        }
    }
}