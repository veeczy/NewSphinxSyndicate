using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pauseMenuUI;   // PausedCanvas root
    public GameObject pausePanelUI;  // PAUSED child panel
    public bool isPaused = false;

    private string lastGameplayScene;

    private const string PauseCanvasName = "PausedCanvas";
    private const string PausePanelName = "PAUSED";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Track last gameplay scene
        if (scene.name != "Settings" && scene.name != "HowToPlay" && scene.name != "MainMenu")
        {
            lastGameplayScene = scene.name;
        }

        pauseMenuUI = FindInactiveObjectByName(PauseCanvasName);
        pausePanelUI = null;

        if (pauseMenuUI != null)
        {
            // Ensure canvas is active for UI interaction
            pauseMenuUI.SetActive(true);

            Transform panel = FindChildRecursive(pauseMenuUI.transform, PausePanelName);
            if (panel != null)
            {
                pausePanelUI = panel.gameObject;
                pausePanelUI.SetActive(false);

                // Wire pause menu buttons
                Button[] pauseButtons = pausePanelUI.GetComponentsInChildren<Button>(true);

                foreach (Button btn in pauseButtons)
                {
                    btn.onClick.RemoveAllListeners();

                    string buttonName = btn.name.ToLower();

                    if (buttonName.Contains("retry"))
                        btn.onClick.AddListener(RetryLevel);
                    else if (buttonName.Contains("quit"))
                        btn.onClick.AddListener(QuitToMenu);
                    else if (buttonName.Contains("howtoplay"))
                        btn.onClick.AddListener(OpenHowToPlay);
                    else if (buttonName.Contains("settings"))
                        btn.onClick.AddListener(OpenSettings);
                }
            }
        }

        // Wire Back buttons only in Settings / HowToPlay scenes
        if (scene.name == "HowToPlay" || scene.name == "Settings")
        {
            Button[] allButtons = FindObjectsOfType<Button>(true);

            foreach (Button btn in allButtons)
            {
                if (btn.name.ToLower().Contains("back"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(BackToGame);
                }
            }
        }

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Update()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        // Only allow pause in gameplay scenes
        if (currentScene.name != "Settings" &&
            currentScene.name != "HowToPlay" &&
            currentScene.name != "MainMenu")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }
    }

    public void Pause()
    {
        if (pausePanelUI == null)
            return;

        pausePanelUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        if (pausePanelUI == null)
            return;

        pausePanelUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RetryLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        Time.timeScale = 1f;
        isPaused = false;
        //SceneManager.LoadScene(currentScene.name); //this just resets whatever scene youre already on

        //send back to tavern so they can reset run if they desire by retry
        PlayerPrefs.SetInt("health", 12); //reset health
        //LevelManager.instance.LoadSceneByTrigger("Tavern Upstairs");
        SceneManager.LoadScene("Tavern Upstairs");
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenHowToPlay()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("HowToPlay");
    }

    public void OpenSettings()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("Settings");
    }

    public void BackToGame()
    {
        if (string.IsNullOrEmpty(lastGameplayScene))
            return;

        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(lastGameplayScene);
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private GameObject FindInactiveObjectByName(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj.name == objectName && obj.scene.isLoaded)
                return obj;
        }

        return null;
    }
}

