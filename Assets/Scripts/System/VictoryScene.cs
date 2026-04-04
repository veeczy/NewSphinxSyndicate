using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScene : MonoBehaviour
{
    public void RestartGame()
    {
        PlayerPrefs.SetInt("desertBoss", 0);
        PlayerPrefs.SetInt("cityBoss", 0);
        PlayerPrefs.SetInt("swampBoss", 0);
        PlayerPrefs.SetInt("bossCounter", 0);

        SceneManager.LoadScene("MainMenu"); 
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}