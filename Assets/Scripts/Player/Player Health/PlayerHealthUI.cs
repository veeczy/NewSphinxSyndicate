using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public Image[] hearts; // should be length 3

    // 5 states per heart
    public Sprite fullHeart;         // 4/4
    public Sprite threeQuarterHeart; // 3/4
    public Sprite halfHeart;         // 2/4
    public Sprite quarterHeart;      // 1/4
    public Sprite emptyHeart;        // 0/4

    void OnEnable()
    {
        //Debug.Log("PHUI OnEnable");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        //Debug.Log("PHUI OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        //Debug.Log("PHUI Start");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("PHUI OnSceneLoaded: " + scene.name);

        if (scene.name == "Bootstrap")
        {
            //Debug.Log("PHUI skipping Bootstrap");
            return;
        }

        FindPlayer();
        FindHearts();
        DebugHearts("OnSceneLoaded");
    }

    void FindPlayer()
    {
        //Debug.Log("PHUI FindPlayer called");

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        //if (playerHealth == null)
            //Debug.Log("PHUI FindPlayer = NULL");
        //else
            //Debug.Log("PHUI FindPlayer = FOUND " + playerHealth.gameObject.name);
    }

    // reconnects the heart image
    void FindHearts()
    {
        //Debug.Log("PHUI FindHearts called");

        hearts = new Image[3];

        GameObject h0 = GameObject.Find("Heart");
        GameObject h1 = GameObject.Find("Heart (1)");
        GameObject h2 = GameObject.Find("Heart (2)");

        //Debug.Log("PHUI FindHearts Heart = " + (h0 != null ? h0.name : "NULL"));
        //Debug.Log("PHUI FindHearts Heart (1) = " + (h1 != null ? h1.name : "NULL"));
        //Debug.Log("PHUI FindHearts Heart (2) = " + (h2 != null ? h2.name : "NULL"));

        if (h0 != null) hearts[0] = h0.GetComponent<Image>();
        if (h1 != null) hearts[1] = h1.GetComponent<Image>();
        if (h2 != null) hearts[2] = h2.GetComponent<Image>();
    }

    void Update()
    {
        if (playerHealth == null)
        {
            //Debug.Log("PHUI Update playerHealth NULL");

            FindPlayer();
            if (playerHealth == null) return;
        }

        if (hearts == null || hearts.Length < 3)
        {
            //Debug.Log("PHUI Update hearts array bad");
            return;
        }

        if (hearts[0] == null || hearts[1] == null || hearts[2] == null)
        {
            //Debug.Log("PHUI Update one or more hearts NULL");
            DebugHearts("Update");
            return;
        }

        int hp = playerHealth.currentHealth; // now 0–12
        //Debug.Log("PHUI Update hp = " + hp);

        for (int i = 0; i < hearts.Length; i++)
        {
            // each heart represents 4 hp
            int heartHP = hp - (i * 4);
            heartHP = Mathf.Clamp(heartHP, 0, 4);

            //Debug.Log("PHUI heart " + i + " value = " + heartHP);

            if (heartHP == 4) hearts[i].sprite = fullHeart;
            else if (heartHP == 3) hearts[i].sprite = threeQuarterHeart;
            else if (heartHP == 2) hearts[i].sprite = halfHeart;
            else if (heartHP == 1) hearts[i].sprite = quarterHeart;
            else hearts[i].sprite = emptyHeart;
        }
    }

    void DebugHearts(string fromWhere)
    {
        //Debug.Log("PHUI DebugHearts from " + fromWhere);

        if (hearts == null)
        {
            //Debug.Log("PHUI hearts array is NULL");
            return;
        }

        Debug.Log("PHUI hearts length = " + hearts.Length);

        for (int i = 0; i < hearts.Length; i++)
        {
            //if (hearts[i] == null)
                //Debug.Log("PHUI hearts[" + i + "] = NULL");
            //else
                //Debug.Log("PHUI hearts[" + i + "] = " + hearts[i].gameObject.name);
        }
    }
}