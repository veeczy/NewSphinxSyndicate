using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPrefsManager : MonoBehaviour
{
    [Header("Player Data")]
    public int playerHealth;
    public int playerMaxHealth = 12;

    [Header("Boss Data")]
    public int bossCounter;
    public int desertBoss;
    public int cityBoss;
    public int swampBoss;

    [Header("Minigame Data")]
    public int credits;
    public int jackpot;

    [Header("Game Settings")]
    public float gamma;
    public string debugSceneChange; //fill this is you want to debug a specific scene

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //PLAYER DATA
        PlayerPrefs.SetInt("health", playerMaxHealth); //at start player prefs memory of health is reset to max health

        //BOSS DATA
        PlayerPrefs.SetInt("desertBoss", 0);
        PlayerPrefs.SetInt("cityBoss", 0);
        PlayerPrefs.SetInt("swampBoss", 0);
        PlayerPrefs.SetInt("bossCounter", 0); //reset amount of bosses beaten to 0

        //MINIGAME DATA
        PlayerPrefs.SetInt("credits", credits);
        PlayerPrefs.SetInt("jackpot", jackpot);
        PlayerPrefs.SetFloat("gamma", 0.5f);

        //SETTINGS DATA
        if (debugSceneChange == "") { debugSceneChange = "MainMenu"; }
        SceneManager.LoadScene(debugSceneChange);

    }

    // Update is called once per frame
    void Update()
    {
        //PLAYER DATA
        playerHealth = PlayerPrefs.GetInt("health");

        //BOSS DATA
        bossCounter = PlayerPrefs.GetInt("bossCounter");
        desertBoss = PlayerPrefs.GetInt("desertBoss");
        cityBoss = PlayerPrefs.GetInt("cityBoss");
        swampBoss = PlayerPrefs.GetInt("swampBoss");

        //MINIGAMES DATA
        credits = PlayerPrefs.GetInt("credits");
        jackpot = PlayerPrefs.GetInt("jackpot");

        //SETTINGS DATA
        gamma = PlayerPrefs.GetFloat("gamma");
    }
}
