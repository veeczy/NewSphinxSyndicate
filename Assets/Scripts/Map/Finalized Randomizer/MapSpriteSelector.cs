using UnityEngine;
using UnityEngine.U2D;

public class MapSpriteSelector : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] MapSprites;
    //basic rooms [type 1]
    public Sprite spriteUp, spriteDown, spriteRight, spriteLeft, spriteUpDown; // one ways
    public Sprite spriteLeftRight, spriteUpRight, spriteUpLeft, spriteDownRight, spriteDownLeft; // two ways
    public Sprite spriteUpDownLeft, spriteUpLeftRight, spriteUpDownRight, spriteDownLeftRight; // three ways
    public Sprite spriteUpDownLeftRight; // four ways

    //special rooms
    public Sprite spriteUpTreasure, spriteDownTreasure, spriteRightTreasure, spriteLeftTreasure; // treasure rooms (basic one ways) [type 3]
    public Sprite spriteUpBoss, spriteDownBoss, spriteRightBoss, spriteLeftBoss; // boss rooms [type 2]
    public Sprite spriteClosedRoom;

    //alt rooms
    //public Sprite spriteUpDownAlt, spriteUpDownLeftRightAlt; // desert alt rooms
    //public Sprite spriteDownRightAlt, spriteUpDownLeftAlt, spriteUpDownRightAlt, spriteUpLeftAlt, spriteUpLeftRightAlt; // city alt rooms 
    //public Sprite spriteLeftRightAlt, spriteUpRightAlt; // swamp alt rooms

    public Sprite spriteTreasureIcon, spriteBossIcon, spriteStartIcon;

    [Header("Map Assign Data")]
    public int type;
    public bool up, down, left, right;
    // room type 0 - entry/starting room
    // room type 1 - normal room
    // room type 2 - boss room 
    // room type 3 - treasure
    public string RoomSetup;

    public bool treasure = false, boss = false;

    [Header("Map Render Data")]
    public Color mainColor;
    public Color normalColor, enterColor;
    SpriteRenderer rend;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeSprites();
        //InitializeGameObjects();

        //temp
        rend = GetComponent<SpriteRenderer>();
        mainColor = normalColor;
        PickSprite();
        PickColor();
    }

    public void InitializeGameObjects()
    {
        //how to get the different gameobjects for the map
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

        // alt rooms


        //treausre, start, boss icons
        //spriteStartIcon = MapSprites[i];
        //spriteTreasureIcon = MapSprites[i];
        //spriteBossIcon = MapSprites[i];
    }

    void PickSprite()
    {
        if(type == 0 || type == 1) { treasure = false; boss = false; }
        if(type == 2) { treasure = false; boss = true; }
        if(type == 3) { treasure = true; boss = false; }

        //one ways
        if (up && !down && !left && !right) { if(treasure) { rend.sprite = spriteUpTreasure; RoomSetup = "Up Treasure"; } if(boss) { rend.sprite = spriteUpBoss; RoomSetup = "Up Boss"; } if(!treasure && !boss) { rend.sprite = spriteUp; RoomSetup = "Up"; } }
        if (down && !up && !left && !right) { if(treasure) { rend.sprite = spriteDownTreasure; RoomSetup = "Down Treasure"; } if(boss) { rend.sprite = spriteDownBoss; RoomSetup = "Down Boss"; } if(!treasure && !boss) { rend.sprite = spriteDown; RoomSetup = "Down"; } }
        if (left && !down && !up && !right) { if (treasure) { rend.sprite = spriteLeftTreasure; RoomSetup = "Left Treasure"; } if (boss) { rend.sprite = spriteLeftBoss; RoomSetup = "Left Boss"; } if(!treasure && !boss) { rend.sprite = spriteLeft; RoomSetup = "Left"; } }
        if (right && !down && !left && !up) { if (treasure) { rend.sprite = spriteRightTreasure; RoomSetup = "Right Treasure"; } if (boss) { rend.sprite = spriteRightBoss; RoomSetup = "Right Boss"; } if(!treasure && !boss) { rend.sprite = spriteRight; RoomSetup = "Right"; } }

        //two ways
        if (up && down && !left && !right) { rend.sprite = spriteUpDown; RoomSetup = "Up Down"; }
        if (!up && !down && left && right) { rend.sprite = spriteLeftRight; RoomSetup = "Left Right"; }

        if (up && !down && !left && right) { rend.sprite = spriteUpRight; RoomSetup = "Up Right"; }
        if (up && !down && left && !right) { rend.sprite = spriteUpLeft; RoomSetup = "Up Left"; }

        if (!up && down && !left && right) { rend.sprite = spriteDownRight; RoomSetup = "Down Right"; }
        if (!up && down && left && !right) { rend.sprite = spriteDownLeft; RoomSetup = "Down Left"; }

        //three ways
        if (up && down && !left && right) { rend.sprite = spriteUpDownRight; RoomSetup = "Up Down Right"; }
        if (up && down && left && !right) { rend.sprite = spriteUpDownLeft; RoomSetup = "Up Down Left"; }

        if (!up && down && left && right) { rend.sprite = spriteDownLeftRight; RoomSetup = "Down Left Right"; }
        if (up && !down && left && right) { rend.sprite = spriteUpLeftRight; RoomSetup = "Up Left Right"; }

        //four ways
        if (up && down && left && right) { rend.sprite = spriteUpDownLeftRight; RoomSetup = "Up Down Left Right"; }


        if(!up && !down && !left && !right) { rend.sprite = spriteClosedRoom; }
    }

    void PickColor()
    {
        // choose the color of the room on the map
        if(type == 0) { mainColor = enterColor; }
        if(type == 1) { mainColor = normalColor; } // normal room

        //this is where id put if it is the room arnolds in (for minimap) to change color to be darker

        // . . .

        rend.color = mainColor; // render the color
    }
}
