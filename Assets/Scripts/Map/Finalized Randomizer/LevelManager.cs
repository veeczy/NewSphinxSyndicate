using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Minimap Data")]
    public bool minimap;
    public Sprite temporarySprite;
    public Sprite[] MapSprites;
    public GameObject[] minimapGrid;

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
        if (Ax1 == null) { Ax1 = FindInactiveObjectByName("Ax1"); }
        if (Ax2 == null) { Ax2 = FindInactiveObjectByName("Ax2"); }
        if (Ax3 == null) { Ax3 = FindInactiveObjectByName("Ax3"); }
        if (Ax4 == null) { Ax4 = FindInactiveObjectByName("Ax4"); }
        if (Ax5 == null) { Ax5 = FindInactiveObjectByName("Ax5"); }
        if (Ax6 == null) { Ax6 = FindInactiveObjectByName("Ax6"); }
        if (Ax7 == null) { Ax7 = FindInactiveObjectByName("Ax7"); }
        if (Ax8 == null) { Ax8 = FindInactiveObjectByName("Ax8"); }

        if (Bx1 == null) { Bx1 = FindInactiveObjectByName("Bx1"); }
        if (Bx2 == null) { Bx2 = FindInactiveObjectByName("Bx2"); }
        if (Bx3 == null) { Bx3 = FindInactiveObjectByName("Bx3"); }
        if (Bx4 == null) { Bx4 = FindInactiveObjectByName("Bx4"); }
        if (Bx5 == null) { Bx5 = FindInactiveObjectByName("Bx5"); }
        if (Bx6 == null) { Bx6 = FindInactiveObjectByName("Bx6"); }
        if (Bx7 == null) { Bx7 = FindInactiveObjectByName("Bx7"); }
        if (Bx8 == null) { Bx8 = FindInactiveObjectByName("Bx8"); }



    }

    public void InitializeMinimap()
    {
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
        // for each coord, check if on mapCoordsList,

        // if so, set name as MapCoordsName

        // then grab sprite using said name and update gameobject

    }

    public void NameToSprite()
    {
        //take name and change it to sprite
    }
}
