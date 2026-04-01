using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image[] hearts; // should be length 3

    // 5 states per heart
    public Sprite fullHeart;        // 4/4
    public Sprite threeQuarterHeart;// 3/4
    public Sprite halfHeart;        // 2/4
    public Sprite quarterHeart;     // 1/4
    public Sprite emptyHeart;       // 0/4

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        FindPlayer();
        FindHearts();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        FindHearts();
    }

    void FindPlayer()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    // reconnects the heart image
    void FindHearts()
    {
        hearts = new Image[3]; 

        hearts[0] = GameObject.Find("Heart")?.GetComponent<Image>();
        hearts[1] = GameObject.Find("Heart (1)")?.GetComponent<Image>();
        hearts[2] = GameObject.Find("Heart (2)")?.GetComponent<Image>();
    }

    void Update()
    {
        if (playerHealth == null)
        {
            // reconnects the player
            FindPlayer();
            if (playerHealth == null) return;
        }

        if (hearts == null || hearts.Length < 3) return;
        if (hearts[0] == null || hearts[1] == null || hearts[2] == null) return;

        int hp = playerHealth.currentHealth; // now 0–12

        for (int i = 0; i < hearts.Length; i++)
        {
            // each heart represents 4 hp
            int heartHP = hp - (i * 4);
            heartHP = Mathf.Clamp(heartHP, 0, 4);

            if (heartHP == 4) hearts[i].sprite = fullHeart;
            else if (heartHP == 3) hearts[i].sprite = threeQuarterHeart;
            else if (heartHP == 2) hearts[i].sprite = halfHeart;
            else if (heartHP == 1) hearts[i].sprite = quarterHeart;
            else hearts[i].sprite = emptyHeart;
        }
    }
}