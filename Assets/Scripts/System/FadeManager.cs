using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Fade UI")]
    public Canvas fadeCanvas;
    public Image fadeImage;
    public float fadeSpeed = 2f;

    public bool isFading = false;
    public bool canMove = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvas != null)
                DontDestroyOnLoad(fadeCanvas.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetAlpha(1f);
        StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void FadeAndLoadScene(string sceneName)
    {
        if (!isFading)
            StartCoroutine(FadeOutAndLoad(sceneName));
    }

    public void FadeAndLoadScene(int sceneIndex)
    {
        if (!isFading)
            StartCoroutine(FadeOutAndLoad(sceneIndex));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isFading = true;
        canMove = false;
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);
        canMove = true;
    }

    private IEnumerator FadeOutAndLoad(int sceneIndex)
    {
        isFading = true;
        canMove = false;
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneIndex);
        canMove = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1f, 0f));
        canMove = true;
        isFading = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        Color c = fadeImage.color;
        float alpha = startAlpha;
        c.a = alpha;
        fadeImage.color = c;

        while (!Mathf.Approximately(alpha, endAlpha))
        {
            alpha = Mathf.MoveTowards(alpha, endAlpha, fadeSpeed * Time.deltaTime);
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;
    }

    private void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}