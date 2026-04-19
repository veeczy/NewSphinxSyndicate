using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;



public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public enum AreaType { Desert, City, Swamp }

    [Header("Current Area (set by wanted board)")]
    public AreaType currentArea = AreaType.Desert; //defaults to desert

    // MASTER arrays (edit in Inspector, never modified)
    [Header("Random Rooms MASTER (build indexes)")] 
    public string[] desertRoomMaster;
    public string[] cityRoomMaster;
    public string[] swampRoomMaster;

    [Header("Boss Rooms")]
    public string bossScene;

    [Header("Random Rooms (when you start Run)")]
    // RUNTIME POOLS (auto-built, these get modified) 
    public List<string> desertRoomPool = new List<string>();
    public List<string> cityRoomPool = new List<string>();
    public List<string> swampRoomPool = new List<string>();

    [Header("Map Progress")]
    public string currentRoom; //records which room youre in by name
    public int currentRoomIndex; //records which room youre in by index
    bool runStarted; //records if ongoing run in progress

    public List<string> mapDirectionList; // from Level Generation, records which doors are open in a scene so that..
    public List<string> mapNameList; // map name list can correct this to the scene names related to said door openings
    public List<Vector2> mapCoordsList; // from Level Generation, records list of coords for the scenes in map
    private string roomName;

    public int x; // x coordinate for where player is
    public int y; // y coordinate for where player is
    public Vector2 playerCoords;

    [Header("Enemies Defeated / Progress")]
    public bool enemiesDefeated;
    public bool[] roomsCleared;

    [Header("Reset when entering this scene index")]
    public string resetSceneName;

    [Header("Boss Defeat Save")]
    public int desertBossClear;
    public int cityBossClear;
    public int swampBossClear;
    public bool AllClear;

    private void Awake()
    {
        if (instance == null) //if there is not an instance of level manager
        {
            instance = this;
            DontDestroyOnLoad(gameObject); //bring level manager to next room

            //area you want to build the map pools
        }
        else //if there is an instance of level manager already
        {
            Destroy(gameObject); //destroy duplicate
        }

        //INITIALIZE SAVE DATA FROM PLAYERPREFS
        desertBossClear = PlayerPrefs.GetInt("desertBoss");
        cityBossClear = PlayerPrefs.GetInt("cityBoss");
        swampBossClear = PlayerPrefs.GetInt("swampBoss");
    }

    private void Update()
    {
        if(runStarted) // if in current run check if enemies defeated
        {
            enemiesDefeated = roomsCleared[currentRoomIndex]; //enemies defeated is whatever the state of the bool says currently
        }

        //CHECK PLAYER PREFS FOR SAVE DATA
        desertBossClear = PlayerPrefs.GetInt("desertBoss");
        cityBossClear = PlayerPrefs.GetInt("cityBoss");
        swampBossClear = PlayerPrefs.GetInt("swampBoss");

        x = PlayerPrefs.GetInt("X");
        y = PlayerPrefs.GetInt("Y");
        playerCoords = new Vector2(x, y);
    }

    public void LoadSceneByTrigger(string sceneName)
    {
        if (sceneName != "") //if sceneName is not empty
        {
            FadeManager.Instance.FadeAndLoadScene(sceneName); //new calls fade then load scene
            return;
        }

        LoadRoom(); //if sceneName is null just load next room
    }

    public void ReturnToTown()
    {
        FadeManager.Instance.FadeAndLoadScene("LoadingScreen"); //new calls fade then load scene
    }

    //*TRIGGERS WHEN SCENE IS LOADED*//
    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == resetSceneName)
        {
            //reset data
        }
    }
    //*END CHECK FOR RESET SCENE*//

    
    public void StartRunInCurrentArea() // Call this when leaving the biome start zone
    {
        
    }

    public void EnemiesDefeated() //called when all enemies are defeated, updates state of room to be cleared
    {
        roomsCleared[currentRoomIndex] = true;
    }

    public void LoadRoom() //loads scene for whichever room youre moving to
    {
        //using coords from player coords, go to scene with said coords found in mapCoordsList
        currentRoomIndex = mapCoordsList.IndexOf(playerCoords); //get index of destination coords
        currentRoom = mapNameList[currentRoomIndex]; //use index to get scene name for it

        FadeManager.Instance.FadeAndLoadScene(currentRoom); // go to scene using scene name
    }

    public void RebuildRoomPool()
    {
        switch (currentArea)
        {
            case AreaType.Desert:
                for (int i = 0; i < mapNameList.Count; i++)
                {
                    //one ways
                    if (roomName == "Up") { roomName = ""; }
                    if (roomName == "Down") { roomName = ""; }
                    if (roomName == "Left") { roomName = ""; }
                    if (roomName == "Right") { roomName = ""; }

                    //two ways
                    if (roomName == "Up Down") { roomName = ""; }
                    if (roomName == "Left Right") { roomName = ""; }

                    if (roomName == "Up Left") { roomName = ""; }
                    if (roomName == "Up Right") { roomName = ""; }

                    if (roomName == "Down Left") { roomName = ""; }
                    if (roomName == "Down Right") { roomName = ""; }

                    //three ways
                    if (roomName == "Up Down Left") { roomName = ""; }
                    if (roomName == "Up Down Right") { roomName = ""; }

                    if (roomName == "Down Left Right") { roomName = ""; }
                    if (roomName == "Up Left Right") { roomName = ""; }

                    //four ways
                    if (roomName == "Up Down Left Right") { roomName = ""; }

                    //boss rooms
                    if (roomName == "Up Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Down Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Left Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Right Boss") { roomName = ""; bossScene = roomName; }

                    //treasure rooms
                    if (roomName == "Up Treasure") { roomName = ""; }
                    if (roomName == "Down Treasure") { roomName = ""; }
                    if (roomName == "Left Treasure") { roomName = ""; }
                    if (roomName == "Right Treasure") { roomName = ""; }

                    //alt rooms

                    mapNameList[i] = roomName;
                }
                break;
            
            case AreaType.City:
                for (int i = 0; i < mapNameList.Count; i++)
                {
                    //one ways
                    if (roomName == "Up") { roomName = ""; }
                    if (roomName == "Down") { roomName = ""; }
                    if (roomName == "Left") { roomName = ""; }
                    if (roomName == "Right") { roomName = ""; }

                    //two ways
                    if (roomName == "Up Down") { roomName = ""; }
                    if (roomName == "Left Right") { roomName = ""; }

                    if (roomName == "Up Left") { roomName = ""; }
                    if (roomName == "Up Right") { roomName = ""; }

                    if (roomName == "Down Left") { roomName = ""; }
                    if (roomName == "Down Right") { roomName = ""; }

                    //three ways
                    if (roomName == "Up Down Left") { roomName = ""; }
                    if (roomName == "Up Down Right") { roomName = ""; }

                    if (roomName == "Down Left Right") { roomName = ""; }
                    if (roomName == "Up Left Right") { roomName = ""; }

                    //four ways
                    if (roomName == "Up Down Left Right") { roomName = ""; }

                    //boss rooms
                    if (roomName == "Up Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Down Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Left Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Right Boss") { roomName = ""; bossScene = roomName; }

                    //treasure rooms
                    if (roomName == "Up Treasure") { roomName = ""; }
                    if (roomName == "Down Treasure") { roomName = ""; }
                    if (roomName == "Left Treasure") { roomName = ""; }
                    if (roomName == "Right Treasure") { roomName = ""; }

                    //alt rooms

                    mapNameList[i] = roomName;
                }
                break;
            
            case AreaType.Swamp:
                for (int i = 0; i < mapNameList.Count; i++)
                {
                    //one ways
                    if (roomName == "Up") { roomName = ""; }
                    if (roomName == "Down") { roomName = ""; }
                    if (roomName == "Left") { roomName = ""; }
                    if (roomName == "Right") { roomName = ""; }

                    //two ways
                    if (roomName == "Up Down") { roomName = ""; }
                    if (roomName == "Left Right") { roomName = ""; }

                    if (roomName == "Up Left") { roomName = ""; }
                    if (roomName == "Up Right") { roomName = ""; }

                    if (roomName == "Down Left") { roomName = ""; }
                    if (roomName == "Down Right") { roomName = ""; }

                    //three ways
                    if (roomName == "Up Down Left") { roomName = ""; }
                    if (roomName == "Up Down Right") { roomName = ""; }

                    if (roomName == "Down Left Right") { roomName = ""; }
                    if (roomName == "Up Left Right") { roomName = ""; }

                    //four ways
                    if (roomName == "Up Down Left Right") { roomName = ""; }

                    //boss rooms
                    if (roomName == "Up Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Down Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Left Boss") { roomName = ""; bossScene = roomName; }
                    if (roomName == "Right Boss") { roomName = ""; bossScene = roomName; }

                    //treasure rooms
                    if (roomName == "Up Treasure") { roomName = ""; }
                    if (roomName == "Down Treasure") { roomName = ""; }
                    if (roomName == "Left Treasure") { roomName = ""; }
                    if (roomName == "Right Treasure") { roomName = ""; }

                    //alt rooms

                    mapNameList[i] = roomName;
                }
                break;
            
            default:
                //
                break;
        }
    }

    public void ResetRun()
    {
        //reset current room pool
        RebuildRoomPool();

        //reset enemy clear progress
        roomsCleared = new bool[mapCoordsList.Count];
        for (int i = 0; i < roomsCleared.Length; i++) { roomsCleared[i] = false; }

        //reset player coords
        PlayerPrefs.SetInt("X", 0);
        PlayerPrefs.SetInt("Y", 0);
        playerCoords = Vector2.zero;
    }

    public void SetArea(AreaType area)
    {
        currentArea = area;
    }

    // check all boss then loads victory scene
    public void CheckAllBossesDead()
    {
        int desertBoss = PlayerPrefs.GetInt("desertBoss", 0);
        int cityBoss = PlayerPrefs.GetInt("cityBoss", 0);
        int swampBoss = PlayerPrefs.GetInt("swampBoss", 0);

        Debug.Log("Desert: " + desertBoss + " City: " + cityBoss + " Swamp: " + swampBoss);

        if (desertBoss == 1 && cityBoss == 1 && swampBoss == 1)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("VictoryScene");
        }
    }
}
