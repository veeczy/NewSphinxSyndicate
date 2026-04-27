using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;



public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public enum AreaType { Desert, City, Swamp }
    private GameObject LevelGenerator;

    [Header("Current Area (set by wanted board)")]
    public AreaType currentArea = AreaType.Desert; //defaults to desert

    [Header("Boss Rooms")]
    public string bossScene;

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

        //update coords
        //currentRoomIndex = mapCoordsList.IndexOf(playerCoords); //get index of destination coords
        //currentRoom = mapNameList[currentRoomIndex]; //use index to get scene name for it
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
        Debug.Log("Start Run in Current Area");
        ResetRun();
        LoadRoom();
    }

    public void EnemiesDefeated() //called when all enemies are defeated, updates state of room to be cleared
    {
        roomsCleared[currentRoomIndex] = true;
    }

    public void LoadRoom() //loads scene for whichever room youre moving to
    {
        x = PlayerPrefs.GetInt("X");
        y = PlayerPrefs.GetInt("Y");
        playerCoords = new Vector2(x, y);

        //using coords from player coords, go to scene with said coords found in mapCoordsList
        currentRoomIndex = mapCoordsList.IndexOf(playerCoords); //get index of destination coords
        currentRoom = mapNameList[currentRoomIndex]; //use index to get scene name for it

        Debug.Log("Player Coords are at: (" + x + ", " + y + ")");
        Debug.Log("The room index is currently: " + currentRoomIndex);
        Debug.Log("The Scene name we are moving to should be: " + currentRoom);

        //FadeManager.Instance.FadeAndLoadScene(currentRoom); // go to scene using scene name
        SceneManager.LoadScene(currentRoom);

       
    }

    public void RebuildRoomPool()
    {
        Debug.Log("Rebuild Room Pool");
        switch (currentArea)
        {
            case AreaType.Desert:
                for (int i = 0; i < mapNameList.Count; i++)
                {
                    bool altChance = Random.value < 0.5f;
                    roomName = mapDirectionList[i];
                    //one ways
                    if (roomName == "Up") { if (!altChance) { roomName = "Desert_Up"; } if (altChance) { roomName = "Desert_Up_Alt"; } }
                    if (roomName == "Down") { roomName = "Desert_Down"; }
                    if (roomName == "Left") { roomName = "Desert_Left"; }
                    if (roomName == "Right") { roomName = "Desert_Right"; }

                    //two ways
                    if (roomName == "Up Down") { if (!altChance) { roomName = "Desert_Up_Down"; } if (altChance) { roomName = "Desert_Up_Down_Alt"; } }
                    if (roomName == "Left Right") { roomName = "Desert_Left_Right"; }

                    if (roomName == "Up Left") { if (!altChance) { roomName = "Desert_Up_Left"; } if (altChance) { roomName = "Desert_Up_Left_Alt"; } }
                    if (roomName == "Up Right") { roomName = "Desert_Up_Right"; }

                    if (roomName == "Down Left") { roomName = "Desert_Down_Left"; }
                    if (roomName == "Down Right") { roomName = "Desert_Down_Right"; }

                    //three ways
                    if (roomName == "Up Down Left") { roomName = "Desert_Up_Down_Left"; }
                    if (roomName == "Up Down Right") { if (!altChance) { roomName = "Desert_Up_Down_Right"; } if (altChance) { roomName = "Desert_Up_Down_Right_Alt"; } }

                    if (roomName == "Down Left Right") { roomName = "Desert_Down_Left_Right"; }
                    if (roomName == "Up Left Right") { roomName = "Desert_Up_Left_Right"; }

                    //four ways
                    if (roomName == "Up Down Left Right") { if (!altChance) { roomName = "Desert_Up_Down_Left_Right"; } if (altChance) { roomName = "Desert_Up_Down_Left_Right_Alt"; } }

                    //boss rooms
                    if (roomName == "Up Boss") { roomName = "Desert_Boss_Up"; bossScene = roomName; }
                    if (roomName == "Down Boss") { roomName = "Desert_Boss_Down"; bossScene = roomName; }
                    if (roomName == "Left Boss") { roomName = "Desert_Boss_Left"; bossScene = roomName; }
                    if (roomName == "Right Boss") { roomName = "Desert_Boss_Right"; bossScene = roomName; }

                    //treasure rooms
                    if (roomName == "Up Treasure") { roomName = "Desert_Treasure_Up"; }
                    if (roomName == "Down Treasure") { roomName = "Desert_Treasure_Down"; }
                    if (roomName == "Left Treasure") { roomName = "Desert_Treasure_Left"; }
                    if (roomName == "Right Treasure") { roomName = "Desert_Treasure_Right"; }

                    mapNameList[i] = roomName;
                    Debug.Log("Room filled in Level Manager");
                }
                break;
            
            case AreaType.City:
                for (int i = 0; i < mapNameList.Count; i++)
                {
                    bool altChance = Random.value < 0.5f;
                    roomName = mapDirectionList[i];
                    //one ways
                    if (roomName == "Up") { roomName = "City_Up"; }
                    if (roomName == "Down") { roomName = "City_Down"; }
                    if (roomName == "Left") { roomName = "City_Left"; }
                    if (roomName == "Right") { roomName = "City_Right"; }

                    //two ways
                    if (roomName == "Up Down") { roomName = "City_Up_Down"; }
                    if (roomName == "Left Right") { roomName = "City_Left_Right"; }

                    if (roomName == "Up Left") { roomName = "City_Up_Left"; }
                    if (roomName == "Up Right") { roomName = "City_Up_Right"; }

                    if (roomName == "Down Left") { roomName = "City_Down_Left"; }
                    if (roomName == "Down Right") { roomName = "City_Down_Right"; }

                    //three ways
                    if (roomName == "Up Down Left") { roomName = "City_Up_Down_Left"; }
                    if (roomName == "Up Down Right") { roomName = "City_Up_Down_Right"; }

                    if (roomName == "Down Left Right") { roomName = "City_Down_Left_Right"; }
                    if (roomName == "Up Left Right") { roomName = "City_Up_Left_Right"; }

                    //four ways
                    if (roomName == "Up Down Left Right") { roomName = "City_Up_Down_Left_Right"; }

                    //boss rooms
                    if (roomName == "Up Boss") { roomName = "City_Boss_Up"; bossScene = roomName; }
                    if (roomName == "Down Boss") { roomName = "City_Boss_Down"; bossScene = roomName; }
                    if (roomName == "Left Boss") { roomName = "City_Boss_Left"; bossScene = roomName; }
                    if (roomName == "Right Boss") { roomName = "City_Boss_Right"; bossScene = roomName; }

                    //treasure rooms
                    if (roomName == "Up Treasure") { roomName = "City_Treasure_Up"; }
                    if (roomName == "Down Treasure") { roomName = "City_Treasure_Down"; }
                    if (roomName == "Left Treasure") { roomName = "City_Treasure_Left"; }
                    if (roomName == "Right Treasure") { roomName = "City_Treasure_Right"; }

                    mapNameList[i] = roomName;
                }
                break;
            
            case AreaType.Swamp:
                for (int i = 0; i < mapNameList.Count; i++)
                {
                    bool altChance = Random.value < 0.5f;
                    roomName = mapDirectionList[i];
                    //one ways
                    if (roomName == "Up") { roomName = "Swamp_Up"; }
                    if (roomName == "Down") { roomName = "Swamp_Down"; }
                    if (roomName == "Left") { roomName = "Swamp_Left"; }
                    if (roomName == "Right") { roomName = "Swamp_Right"; }

                    //two ways
                    if (roomName == "Up Down") { roomName = "Swamp_Up_Down"; }
                    if (roomName == "Left Right") { roomName = "Swamp_Left_Right"; }

                    if (roomName == "Up Left") { roomName = "Swamp_Up_Left"; }
                    if (roomName == "Up Right") { roomName = "Swamp_Up_Right"; }

                    if (roomName == "Down Left") { roomName = "Swamp_Down_Left"; }
                    if (roomName == "Down Right") { roomName = "Swamp_Down_Right"; }

                    //three ways
                    if (roomName == "Up Down Left") { roomName = "Swamp_Up_Down_Left"; }
                    if (roomName == "Up Down Right") { roomName = "Swamp_Up_Down_Right"; }

                    if (roomName == "Down Left Right") { roomName = "Swamp_Down_Left_Right"; }
                    if (roomName == "Up Left Right") { roomName = "Swamp_Up_Left_Right"; }

                    //four ways
                    if (roomName == "Up Down Left Right") { roomName = "Swamp_Up_Down_Left_Right"; }

                    //boss rooms
                    if (roomName == "Up Boss") { roomName = "Swamp_Boss_Up"; bossScene = roomName; }
                    if (roomName == "Down Boss") { roomName = "Swamp_Boss_Down"; bossScene = roomName; }
                    if (roomName == "Left Boss") { roomName = "Swamp_Boss_Left"; bossScene = roomName; }
                    if (roomName == "Right Boss") { roomName = "Swamp_Boss_Right"; bossScene = roomName; }

                    //treasure rooms
                    if (roomName == "Up Treasure") { roomName = "Swamp_Treasure_Up"; }
                    if (roomName == "Down Treasure") { roomName = "Swamp_Treasure_Down"; }
                    if (roomName == "Left Treasure") { roomName = "Swamp_Treasure_Left"; }
                    if (roomName == "Right Treasure") { roomName = "Swamp_Treasure_Right"; }

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
        Debug.Log("Data Reset");
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
