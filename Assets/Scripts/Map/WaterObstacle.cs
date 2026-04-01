using UnityEngine;

public class WaterObstacle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMove = other.GetComponent<PlayerMovement>();

            if (playerMove != null)
            {
                playerMove.inWater = true;
                Debug.Log("IN WATER");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMove = other.GetComponent<PlayerMovement>();

            if (playerMove != null)
            {
                playerMove.inWater = false;
                Debug.Log("OUT WATER");
            }
        }
    }
}