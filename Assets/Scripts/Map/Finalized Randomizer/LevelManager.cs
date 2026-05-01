using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    public bool runStarted; //records if ongoing run in progress

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

    [Header("Minimap Data")]
    public bool minimap = false;
    bool initialize = false;
    public Sprite temporarySprite;
    public Sprite[] MapSprites;
    public GameObject[] minimapGrid;
    public Vector2[] minimapCoords;
    Image rend;

    public Color filledColor;
    public Color normalColor;

    //basic rooms [type 1]
    public Sprite spriteUp, spriteDown, spriteRight, spriteLeft, spriteUpDown; // one ways
    public Sprite spriteLeftRight, spriteUpRight, spriteUpLeft, spriteDownRight, spriteDownLeft; // two ways
    public Sprite spriteUpDownLeft, spriteUpLeftRight, spriteUpDownRight, spriteDownLeftRight; // three ways
    public Sprite spriteUpDownLeftRight; // four ways

    //special rooms
    public Sprite spriteUpTreasure, spriteDownTreasure, spriteRightTreasure, spriteLeftTreasure; // treasure rooms (basic one ways) [type 3]
    public Sprite spriteUpBoss, spriteDownBoss, spriteRightBoss, spriteLeftBoss; // boss rooms [type 2]
    public Sprite spriteClosedRoom;

    //individual grid pieces 
    public GameObject Ax1, Ax2, Ax3, Ax4, Ax5, Ax6, Ax7, Ax8;
    public GameObject Bx1, Bx2, Bx3, Bx4, Bx5, Bx6, Bx7, Bx8;
    public GameObject Cx1, Cx2, Cx3, Cx4, Cx5, Cx6, Cx7, Cx8;
    public GameObject Dx1, Dx2, Dx3, Dx4, Dx5, Dx6, Dx7, Dx8;
    public GameObject Ex1, Ex2, Ex3, Ex4, Ex5, Ex6, Ex7, Ex8;
    public GameObject Fx1, Fx2, Fx3, Fx4, Fx5, Fx6, Fx7, Fx8;
    public GameObject Gx1, Gx2, Gx3, Gx4, Gx5, Gx6, Gx7, Gx8;
    public GameObject Hx1, Hx2, Hx3, Hx4, Hx5, Hx6, Hx7, Hx8;


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

        initialize = false;

        
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
        if (enemiesDefeated == false) { minimap = false; }
        if (runStarted && !minimap) // if in a scene after the run has started and the minimap is not rendered
        {
            InitializeGameObjects(); // grab ui game objects
            Debug.Log("Initialize Game Objects");

            InitializeSprites(); // grab ui sprites
            Debug.Log("Initialize UI Sprites");

            InitializeMinimap(); // load coordinates and gameobjects into array
            Debug.Log("Initialize Minimap");

            //ManualLoadMapSprites();
            LoadMapSprites(); // update gameobjects to sprites and render
            Debug.Log("Minimap Rendered");

            minimap = true; // set minimap to true because minimap is now set up in scene
            Debug.Log("Minimap Done");
        }
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
        runStarted = true;
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
        runStarted = false;

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

    private GameObject FindInactiveObjectByName(string name)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in objects)
        {
            if (obj.name == name && obj.scene.isLoaded)
                return obj;
        }

        return null;
    }

    public void InitializeGameObjects()
    {
        Ax1 = FindInactiveObjectByName("Ax1");
        Ax2 = FindInactiveObjectByName("Ax2"); 
        Ax3 = FindInactiveObjectByName("Ax3"); 
        Ax4 = FindInactiveObjectByName("Ax4");
        Ax5 = FindInactiveObjectByName("Ax5");
        Ax6 = FindInactiveObjectByName("Ax6");
        Ax7 = FindInactiveObjectByName("Ax7");
        Ax8 = FindInactiveObjectByName("Ax8");

        Bx1 = FindInactiveObjectByName("Bx1");
        Bx2 = FindInactiveObjectByName("Bx2");
        Bx3 = FindInactiveObjectByName("Bx3");
        Bx4 = FindInactiveObjectByName("Bx4");
        Bx5 = FindInactiveObjectByName("Bx5");
        Bx6 = FindInactiveObjectByName("Bx6");
        Bx7 = FindInactiveObjectByName("Bx7");
        Bx8 = FindInactiveObjectByName("Bx8");

        Cx1 = FindInactiveObjectByName("Cx1");
        Cx2 = FindInactiveObjectByName("Cx2");
        Cx3 = FindInactiveObjectByName("Cx3");
        Cx4 = FindInactiveObjectByName("Cx4");
        Cx5 = FindInactiveObjectByName("Cx5");
        Cx6 = FindInactiveObjectByName("Cx6");
        Cx7 = FindInactiveObjectByName("Cx7");
        Cx8 = FindInactiveObjectByName("Cx8");

        Dx1 = FindInactiveObjectByName("Dx1");
        Dx2 = FindInactiveObjectByName("Dx2");
        Dx3 = FindInactiveObjectByName("Dx3");
        Dx4 = FindInactiveObjectByName("Dx4");
        Dx5 = FindInactiveObjectByName("Dx5");
        Dx6 = FindInactiveObjectByName("Dx6");
        Dx7 = FindInactiveObjectByName("Dx7");
        Dx8 = FindInactiveObjectByName("Dx8");

        Ex1 = FindInactiveObjectByName("Ex1");
        Ex2 = FindInactiveObjectByName("Ex2");
        Ex3 = FindInactiveObjectByName("Ex3");
        Ex4 = FindInactiveObjectByName("Ex4");
        Ex5 = FindInactiveObjectByName("Ex5");
        Ex6 = FindInactiveObjectByName("Ex6");
        Ex7 = FindInactiveObjectByName("Ex7");
        Ex8 = FindInactiveObjectByName("Ex8");

        Fx1 = FindInactiveObjectByName("Fx1");
        Fx2 = FindInactiveObjectByName("Fx2");
        Fx3 = FindInactiveObjectByName("Fx3");
        Fx4 = FindInactiveObjectByName("Fx4");
        Fx5 = FindInactiveObjectByName("Fx5");
        Fx6 = FindInactiveObjectByName("Fx6");
        Fx7 = FindInactiveObjectByName("Fx7");
        Fx8 = FindInactiveObjectByName("Fx8");

        Gx1 = FindInactiveObjectByName("Gx1");
        Gx2 = FindInactiveObjectByName("Gx2");
        Gx3 = FindInactiveObjectByName("Gx3");
        Gx4 = FindInactiveObjectByName("Gx4");
        Gx5 = FindInactiveObjectByName("Gx5");
        Gx6 = FindInactiveObjectByName("Gx6");
        Gx7 = FindInactiveObjectByName("Gx7");
        Gx8 = FindInactiveObjectByName("Gx8");

        Hx1 = FindInactiveObjectByName("Hx1");
        Hx2 = FindInactiveObjectByName("Hx2");
        Hx3 = FindInactiveObjectByName("Hx3");
        Hx4 = FindInactiveObjectByName("Hx4");
        Hx5 = FindInactiveObjectByName("Hx5");
        Hx6 = FindInactiveObjectByName("Hx6");
        Hx7 = FindInactiveObjectByName("Hx7");
        Hx8 = FindInactiveObjectByName("Hx8");

    }

    public void InitializeMinimap()
    {
        //game objects
        minimapGrid[0] = Ax1; //(-4,-4)
        minimapGrid[1] = Ax2; //(-4,-3)
        minimapGrid[2] = Ax3; //(-4,-2)
        minimapGrid[3] = Ax4; //(-4,-1)
        minimapGrid[4] = Ax5; //(-4,0)
        minimapGrid[5] = Ax6; //(-4,1)
        minimapGrid[6] = Ax7; //(-4,2)
        minimapGrid[7] = Ax8; //(-4,3)

        minimapGrid[8] = Bx1; //(-3,-4)
        minimapGrid[9] = Bx2; //(-3,-3)
        minimapGrid[10] = Bx3; //(-3,-2)
        minimapGrid[11] = Bx4; //(-3,-1)
        minimapGrid[12] = Bx5; //(-3,0)
        minimapGrid[13] = Bx6; //(-3,1)
        minimapGrid[14] = Bx7; //(-3,2)
        minimapGrid[15] = Bx8; //(-3,3)

        minimapGrid[16] = Cx1; //(-2,-4)
        minimapGrid[17] = Cx2; //(-2,-3)
        minimapGrid[18] = Cx3; //(-2,-2)
        minimapGrid[19] = Cx4; //(-2,-1)
        minimapGrid[20] = Cx5; //(-2,0)
        minimapGrid[21] = Cx6; //(-2,1)
        minimapGrid[22] = Cx7; //(-2,2)
        minimapGrid[23] = Cx8; //(-2,3)

        minimapGrid[24] = Dx1; //(-1,-4)
        minimapGrid[25] = Dx2; //(-1,-3)
        minimapGrid[26] = Dx3; //(-1,-2)
        minimapGrid[27] = Dx4; //(-1,-1)
        minimapGrid[28] = Dx5; //(-1,0)
        minimapGrid[29] = Dx6; //(-1,1)
        minimapGrid[30] = Dx7; //(-1,2)
        minimapGrid[31] = Dx8; //(-1,3)

        minimapGrid[32] = Ex1; // (0,-4)
        minimapGrid[33] = Ex2; // (0,-3)
        minimapGrid[34] = Ex3; // (0,-2)
        minimapGrid[35] = Ex4; // (0,-1)
        minimapGrid[36] = Ex5; // (0,0)
        minimapGrid[37] = Ex6; // (0,1)
        minimapGrid[38] = Ex7; // (0,2)
        minimapGrid[39] = Ex8; // (0,3)

        minimapGrid[40] = Fx1; // (1,-4)
        minimapGrid[41] = Fx2; // (1,-3)
        minimapGrid[42] = Fx3; // (1,-2)
        minimapGrid[43] = Fx4; // (1,-1)
        minimapGrid[44] = Fx5; // (1,0)
        minimapGrid[45] = Fx6; // (1,1)
        minimapGrid[46] = Fx7; // (1,2)
        minimapGrid[47] = Fx8; // (1,3)

        minimapGrid[48] = Gx1; // (2,-4)
        minimapGrid[49] = Gx2; // (2,-3)
        minimapGrid[50] = Gx3; // (2,-2)
        minimapGrid[51] = Gx4; // (2,-1)
        minimapGrid[52] = Gx5; // (2,0)
        minimapGrid[53] = Gx6; // (2,1)
        minimapGrid[54] = Gx7; // (2,2)
        minimapGrid[55] = Gx8; // (2,3)

        minimapGrid[56] = Hx1; // (3,-4)
        minimapGrid[57] = Hx2; // (3,-3)
        minimapGrid[58] = Hx3; // (3,-2)
        minimapGrid[59] = Hx4; // (3,-1)
        minimapGrid[60] = Hx5; // (3,0)
        minimapGrid[61] = Hx6; // (3,1)
        minimapGrid[62] = Hx7; // (3,2)
        minimapGrid[63] = Hx8; // (3,3)

        //coords 
        minimapCoords[0] = new Vector2(-4, 4); //AX1
        minimapCoords[1] = new Vector2(-3, 4); //AX2
        minimapCoords[2] = new Vector2(-2, 4); //AX3
        minimapCoords[3] = new Vector2(-1, 4); //AX4
        minimapCoords[4] = new Vector2(0, 4); //AX5
        minimapCoords[5] = new Vector2(1, 4); //AX6
        minimapCoords[6] = new Vector2(2, 4); //AX7
        minimapCoords[7] = new Vector2(3, 4); //AX8

        minimapCoords[8] = new Vector2(-4, 3);
        minimapCoords[9] = new Vector2(-3, 3);
        minimapCoords[10] = new Vector2(-2, 3);
        minimapCoords[11] = new Vector2(-1, 3);
        minimapCoords[12] = new Vector2(0, 3);
        minimapCoords[13] = new Vector2(1, 3);
        minimapCoords[14] = new Vector2(2, 3);
        minimapCoords[15] = new Vector2(3, 3);

        minimapCoords[16] = new Vector2(-4, 2);
        minimapCoords[17] = new Vector2(-3, 2);
        minimapCoords[18] = new Vector2(-2, 2);
        minimapCoords[19] = new Vector2(-1, 2);
        minimapCoords[20] = new Vector2(0, 2);
        minimapCoords[21] = new Vector2(1, 2);
        minimapCoords[22] = new Vector2(2, 2);
        minimapCoords[23] = new Vector2(3, 2);

        minimapCoords[24] = new Vector2(-4, 1);
        minimapCoords[25] = new Vector2(-3, 1);
        minimapCoords[26] = new Vector2(-2, 1);
        minimapCoords[27] = new Vector2(-1, 1);
        minimapCoords[28] = new Vector2(0, 1);
        minimapCoords[29] = new Vector2(1, 1);
        minimapCoords[30] = new Vector2(2, 1);
        minimapCoords[31] = new Vector2(3, 1);

        minimapCoords[32] = new Vector2(-4, 0);
        minimapCoords[33] = new Vector2(-3, 0);
        minimapCoords[34] = new Vector2(-2, 0);
        minimapCoords[35] = new Vector2(-1, 0);
        minimapCoords[36] = new Vector2(0, 0);
        minimapCoords[37] = new Vector2(1, 0);
        minimapCoords[38] = new Vector2(2, 0);
        minimapCoords[39] = new Vector2(3, 0);

        minimapCoords[40] = new Vector2(-4, -1);
        minimapCoords[41] = new Vector2(-3, -1);
        minimapCoords[42] = new Vector2(-2, -1);
        minimapCoords[43] = new Vector2(-1, -1);
        minimapCoords[44] = new Vector2(0, -1);
        minimapCoords[45] = new Vector2(1, -1);
        minimapCoords[46] = new Vector2(2, -1);
        minimapCoords[47] = new Vector2(3, -1);

        minimapCoords[48] = new Vector2(-4, -2);
        minimapCoords[49] = new Vector2(-3, -2);
        minimapCoords[50] = new Vector2(-2, -2);
        minimapCoords[51] = new Vector2(-1, -2);
        minimapCoords[52] = new Vector2(0, -2);
        minimapCoords[53] = new Vector2(1, -2);
        minimapCoords[54] = new Vector2(2, -2);
        minimapCoords[55] = new Vector2(2, -3);

        minimapCoords[56] = new Vector2(-4, -3);
        minimapCoords[57] = new Vector2(-3, -3);
        minimapCoords[58] = new Vector2(-2, -3);
        minimapCoords[59] = new Vector2(-1, -3);
        minimapCoords[60] = new Vector2(0, -3);
        minimapCoords[61] = new Vector2(1, -3);
        minimapCoords[62] = new Vector2(2, -3);
        minimapCoords[63] = new Vector2(3, -3);
    }

    public void InitializeSprites()
    {
        // one ways
        spriteUp = MapSprites[0];
        spriteDown = MapSprites[1];
        spriteLeft = MapSprites[2];
        spriteRight = MapSprites[3];

        // two ways
        spriteUpDown = MapSprites[4];
        spriteLeftRight = MapSprites[5];

        spriteUpRight = MapSprites[6];
        spriteUpLeft = MapSprites[7];

        spriteDownRight = MapSprites[8];
        spriteDownLeft = MapSprites[9];

        // three ways
        spriteUpDownLeft = MapSprites[10];
        spriteUpDownRight = MapSprites[11];

        spriteDownLeftRight = MapSprites[12];
        spriteUpLeftRight = MapSprites[13];

        // four ways
        spriteUpDownLeftRight = MapSprites[14];

        // treasure rooms
        spriteUpTreasure = MapSprites[15];
        spriteDownTreasure = MapSprites[16];
        spriteLeftTreasure = MapSprites[17];
        spriteRightTreasure = MapSprites[18];

        // boss rooms
        spriteUpBoss = MapSprites[19];
        spriteDownBoss = MapSprites[20];
        spriteLeftBoss = MapSprites[21];
        spriteRightBoss = MapSprites[22];
    }

    public void LoadMapSprites()
    {
        int index; //index of room
        bool coordsContained = false; // bool if the coords are contained
        Vector2 coordsCheck; //placeholder coords
        GameObject placeholder;
        // for each coord, check if on mapCoordsList
        // if so take direction from mapDirectionList
        // ...then grab sprite using said name and update gameobject

        for(int i = 0; i < minimapCoords.Length; i++)
        {
            coordsCheck = minimapCoords[i];
            coordsContained = mapCoordsList.Contains(coordsCheck);
            if (coordsContained)
            {
                Debug.Log("GameObject " + minimapGrid[i]);
                index = mapCoordsList.IndexOf(coordsCheck);

                placeholder = minimapGrid[i];
                rend = placeholder.GetComponent<Image>(); //grab image 

                rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

                if (coordsCheck == playerCoords) { rend.color = filledColor; }
                else { rend.color = normalColor; }

                Debug.Log("The room at coords: " + coordsCheck + " is supposed to be marked as " + mapDirectionList[index] + " which relates to the coords at " + mapCoordsList[index]);
                minimapGrid[i].SetActive(true);
            }
            coordsContained = false;
        }

        //now entire minimap should be loaded
    }

    public Sprite NameToSprite(string roomName)
    {
        //take name and change it to sprite...

        //one ways
        if(roomName == "Up") { temporarySprite = spriteUp; }
        if (roomName == "Up Treasure") { temporarySprite = spriteUpTreasure; }
        if (roomName == "Up Boss") { temporarySprite = spriteUpBoss; }

        if (roomName == "Down") { temporarySprite = spriteDown; }
        if (roomName == "Down Treasure") { temporarySprite = spriteDownTreasure; }
        if (roomName == "Down Boss") { temporarySprite = spriteDownBoss; }

        if (roomName == "Left") { temporarySprite = spriteLeft; }
        if (roomName == "Left Treasure") { temporarySprite = spriteLeftTreasure; }
        if (roomName == "Left Boss") { temporarySprite = spriteLeftBoss; }

        if (roomName == "Right") { temporarySprite = spriteRight; }
        if (roomName == "Right Treasure") { temporarySprite = spriteRightTreasure; }
        if (roomName == "Right Boss") { temporarySprite = spriteRightBoss; }


        //two ways
        if (roomName == "Up Down") { temporarySprite = spriteUpDown; }
        if (roomName == "Left Right") { temporarySprite = spriteLeftRight; }

        if (roomName == "Up Right") { temporarySprite = spriteUpRight; }
        if (roomName == "Up Left") { temporarySprite = spriteUpLeft; }

        if (roomName == "Down Right") { temporarySprite = spriteDownRight; }
        if (roomName == "Down Left") { temporarySprite = spriteDownLeft; }


        //three ways
        if (roomName == "Up Down Right") { temporarySprite = spriteUpDownRight; }
        if (roomName == "Up Down Left") { temporarySprite = spriteUpDownLeft; }

        if (roomName == "Up Left Right") { temporarySprite = spriteUpLeftRight; }
        if (roomName == "Down Left Right") { temporarySprite = spriteDownLeftRight; }


        //four ways
        if (roomName == "Up Down Left Right") { temporarySprite = spriteUpDownLeftRight; }

        //else
        if (roomName == "") { temporarySprite = spriteClosedRoom; }
        Debug.Log("Sprite is " + roomName + " and renders " + temporarySprite);
        return temporarySprite;
    }

    public void ManualLoadMapSprites()
    {

        int index; //index of room
        bool coordsContained = false; // bool if the coords are contained
        Vector2 coordsCheck; //placeholder coords
        GameObject placeholder;

        //A ROW
        coordsCheck = minimapCoords[0];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[0]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax1.SetActive(true);
        }

        coordsCheck = minimapCoords[1];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[1]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax2.SetActive(true);
        }

        coordsCheck = minimapCoords[2];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[2]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax3.SetActive(true);
        }

        coordsCheck = minimapCoords[3];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[3]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax4.SetActive(true);
        }

        coordsCheck = minimapCoords[4];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[4]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax5.SetActive(true);
        }

        coordsCheck = minimapCoords[5];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[5]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax6.SetActive(true);
        }

        coordsCheck = minimapCoords[6];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[6]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax7.SetActive(true);
        }

        coordsCheck = minimapCoords[7];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[7]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ax8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ax8.SetActive(true);
        }

        //B ROW
        coordsCheck = minimapCoords[8];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[8]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx1.SetActive(true);
        }

        coordsCheck = minimapCoords[9];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[9]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx2.SetActive(true);
        }

        coordsCheck = minimapCoords[10];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[10]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx3.SetActive(true);
        }

        coordsCheck = minimapCoords[11];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[11]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx4.SetActive(true);
        }

        coordsCheck = minimapCoords[12];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[12]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx5.SetActive(true);
        }

        coordsCheck = minimapCoords[13];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[13]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx6.SetActive(true);
        }

        coordsCheck = minimapCoords[14];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[14]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx7.SetActive(true);
        }

        coordsCheck = minimapCoords[15];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[15]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Bx8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Bx8.SetActive(true);
        }

        //C ROW
        coordsCheck = minimapCoords[16];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[16]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx1.SetActive(true);
        }

        coordsCheck = minimapCoords[17];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[17]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx2.SetActive(true);
        }

        coordsCheck = minimapCoords[18];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[18]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx3.SetActive(true);
        }

        coordsCheck = minimapCoords[19];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[19]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx4.SetActive(true);
        }

        coordsCheck = minimapCoords[20];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[20]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx5.SetActive(true);
        }

        coordsCheck = minimapCoords[21];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[22]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx6.SetActive(true);
        }

        coordsCheck = minimapCoords[22];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[22]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx7.SetActive(true);
        }

        coordsCheck = minimapCoords[23];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[23]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Cx8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Cx8.SetActive(true);
        }

        //D ROW
        coordsCheck = minimapCoords[24];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[24]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx1.SetActive(true);
        }

        coordsCheck = minimapCoords[25];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[25]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx2.SetActive(true);
        }

        coordsCheck = minimapCoords[26];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[26]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx3.SetActive(true);
        }

        coordsCheck = minimapCoords[27];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[27]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx4.SetActive(true);
        }

        coordsCheck = minimapCoords[28];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[28]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx5.SetActive(true);
        }

        coordsCheck = minimapCoords[29];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[29]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx6.SetActive(true);
        }

        coordsCheck = minimapCoords[30];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[30]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx7.SetActive(true);
        }

        coordsCheck = minimapCoords[31];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[31]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Dx8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Dx8.SetActive(true);
        }

        //E ROW
        coordsCheck = minimapCoords[32];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[32]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex1.SetActive(true);
        }

        coordsCheck = minimapCoords[33];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[33]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex2.SetActive(true);
        }

        coordsCheck = minimapCoords[34];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[34]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex3.SetActive(true);
        }

        coordsCheck = minimapCoords[35];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[35]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex4.SetActive(true);
        }

        coordsCheck = minimapCoords[36];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[36]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex5.SetActive(true);
        }

        coordsCheck = minimapCoords[37];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[37]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex6.SetActive(true);
        }

        coordsCheck = minimapCoords[38];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[38]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex7.SetActive(true);
        }

        coordsCheck = minimapCoords[39];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[39]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Ex8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Ex8.SetActive(true);
        }

        //F ROW
        coordsCheck = minimapCoords[40];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[40]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx1.SetActive(true);
        }

        coordsCheck = minimapCoords[41];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[41]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx2.SetActive(true);
        }

        coordsCheck = minimapCoords[42];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[42]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx3.SetActive(true);
        }

        coordsCheck = minimapCoords[43];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[43]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx4.SetActive(true);
        }

        coordsCheck = minimapCoords[44];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[44]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx5.SetActive(true);
        }

        coordsCheck = minimapCoords[45];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[45]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx6.SetActive(true);
        }

        coordsCheck = minimapCoords[46];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[46]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx7.SetActive(true);
        }

        coordsCheck = minimapCoords[47];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[47]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Fx8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Fx8.SetActive(true);
        }

        //G ROW
        coordsCheck = minimapCoords[48];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[48]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx1.SetActive(true);
        }

        coordsCheck = minimapCoords[49];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[49]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx2.SetActive(true);
        }

        coordsCheck = minimapCoords[50];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[50]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx3.SetActive(true);
        }

        coordsCheck = minimapCoords[51];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[51]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx4.SetActive(true);
        }

        coordsCheck = minimapCoords[52];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[52]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx5.SetActive(true);
        }

        coordsCheck = minimapCoords[53];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[53]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx6.SetActive(true);
        }

        coordsCheck = minimapCoords[54];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[54]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx7.SetActive(true);
        }

        coordsCheck = minimapCoords[55];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[55]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Gx8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Gx8.SetActive(true);
        }

        //H ROW
        coordsCheck = minimapCoords[56];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[56]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx1;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx1.SetActive(true);
        }

        coordsCheck = minimapCoords[57];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[57]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx2;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx2.SetActive(true);
        }

        coordsCheck = minimapCoords[58];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[58]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx3;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx3.SetActive(true);
        }

        coordsCheck = minimapCoords[59];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[59]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx4;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx4.SetActive(true);
        }

        coordsCheck = minimapCoords[60];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[60]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx5;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx5.SetActive(true);
        }

        coordsCheck = minimapCoords[61];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[61]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx6;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx6.SetActive(true);
        }

        coordsCheck = minimapCoords[62];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[62]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx7;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx7.SetActive(true);
        }

        coordsCheck = minimapCoords[63];
        coordsContained = mapCoordsList.Contains(coordsCheck);
        if (coordsContained && (coordsCheck == minimapCoords[63]))
        {
            index = mapCoordsList.IndexOf(coordsCheck);

            placeholder = Hx8;
            rend = placeholder.GetComponent<Image>(); //grab image 

            rend.sprite = NameToSprite(mapDirectionList[index]); // update sprite

            if (coordsCheck == playerCoords) { rend.color = filledColor; }
            else { rend.color = normalColor; }

            Hx8.SetActive(true);
        }
    }

}
