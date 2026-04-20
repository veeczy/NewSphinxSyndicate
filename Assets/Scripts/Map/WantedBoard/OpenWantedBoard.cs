using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenWantedBoard : MonoBehaviour
{
    public string wantedBoardSceneName = "Wanted Board";
    //public KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    private void Update()
    {
        // new e and A button work now
        if (playerInRange && Input.GetButtonDown("Interact"))
        {
            // save where we came from
            PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
            SceneManager.LoadScene(wantedBoardSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}


