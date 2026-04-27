using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance; //any script can now be read by music manager without references

    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;

    [Header("Menu Tracks")]  //streamlines everything to just typing the name of tracks rather than dragging.
    public string mainMenuTrack = "MainMenu";
    public string winTrackName = "WinMusic";
    public string gameOverTrackName = "GameOver";

    [Header("Desert Tracks")]
    public string desertTownTrack = "TavernMusic";
    public string desertRoomTrack = "DesertRoomMusic";
    public string desertBossTrack = "DesertBossMusic";

    [Header("City Tracks")]
    public string cityTownTrack = "GasStationMusic";
    public string cityRoomTrack = "CityRoomMusic";
    public string cityBossTrack = "CityBossMusic";

    [Header("Swamp Tracks")]
    public string swampTownTrack = "SwampShopMusic";
    public string swampRoomTrack = "SwampRoomMusic";
    public string swampBossTrack = "SwampBossMusic";

    private void Awake()
    {
        if (Instance != null) //make sure only ONE music manager is ever active
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        musicSource.loop = true; //verifies music is on loop
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sName = scene.name;

        //1 Menus
        if (sName == "MainMenu" || sName == "LoadingScreen") { PlayMusic(mainMenuTrack); return; }
        if (sName == "GameOver") { PlayMusic(gameOverTrackName); return; }
        if (sName == "WinScene") { PlayMusic(winTrackName); return; }

        //2 Detects the level music

        string trackToPlay = "";

        //Desert

        if (sName.Contains("Tavern")) trackToPlay = desertTownTrack;
        else if (sName.StartsWith("Desert_"))
        {
            if (sName.Contains("Boss")) trackToPlay = desertBossTrack;
            else trackToPlay = desertRoomTrack;
        }

        //City
        else if (sName.Contains("Gas_Station")) trackToPlay = cityTownTrack;
        else if (sName.StartsWith("City_"))
        {
            if (sName.Contains("Boss")) trackToPlay = cityBossTrack;
            else trackToPlay = cityRoomTrack;
        }

        //Swamp
        else if (sName.Contains("Swamp_Shop")) trackToPlay = swampTownTrack;
        else if (sName.StartsWith("Swamp_"))
        {
            if (sName.Contains("Boss")) trackToPlay = swampBossTrack;
            else trackToPlay = swampRoomTrack;
        }

        if (!string.IsNullOrEmpty(trackToPlay)) PlayMusic(trackToPlay);


        if (LevelManager.instance == null) return; //wait for level manager to load first

    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        AudioClip nextClip = musicLibrary.GetClipFromName(trackName); //allows the music manager to pull stuff from the music library

        if (nextClip == null) //debug
        {
            Debug.LogWarning("music can't be found: " + trackName);
            return;
        }


        if (musicSource.clip == nextClip)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(AnimateMusicCrossfade(nextClip, fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.25f) //all this is fades and transitions, I added a musicSource.Stop 
    {
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * (1 / fadeDuration);
            musicSource.volume = Mathf.Lerp(1f, 0, percent);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * (1 / fadeDuration);
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
    }
}