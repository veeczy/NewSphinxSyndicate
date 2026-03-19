using JetBrains.Annotations;
using NUnit.Framework;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

public class Slots : MonoBehaviour
{
    [Header("Game Settings")]
    public bool isTalking = false;
    public bool canMove = true;
    public bool playAgain = true;
    public bool gameActive = false;
    public bool playerNear = false;

    [Header("Slots Symbols")]
    public Sprite[] slotSymbols; //array of all the symbols on the slot machine
    public int[] slotValues; //array for the value of each symbol on the slot machine

    public Sprite[] creditSprites; //sprites for credits

    [Header("Slot UI - Base")]
    public GameObject slotScreen; //background screen
    //dialogue
    public GameObject dialogueUI; //background panel for dialogue
    public TMP_Text dialogueText; //plays jackpot text and any other flavor text
    public GameObject lever; //lever (make button)

    [Header("Slot UI - Credits")]
    public GameObject creditsPanel;
    public GameObject creditsNumber1;
    public GameObject creditsNumber2;
    public GameObject creditsNumber3;
    public GameObject creditsNumber4;

    [Header("Slot UI - JackPot")]
    public GameObject jackpotPanel;
    public TMP_Text jackPotText;


    //*SLOTS ITSELF* in rows and columns, 3X5 = 15 spaces//
    [Header("Slot UI - Row 1")]
    public GameObject r1c1; 
    public GameObject r1c2;
    public GameObject r1c3;
    public GameObject r1c4;
    public GameObject r1c5;

    [Header("Slot UI -  Row 2")]
    public GameObject r2c1;
    public GameObject r2c2;
    public GameObject r2c3;
    public GameObject r2c4;
    public GameObject r2c5;

    [Header("Slot UI - Row 3")]
    public GameObject r3c1;
    public GameObject r3c2;
    public GameObject r3c3;
    public GameObject r3c4;
    public GameObject r3c5;

    [Header("Slot UI - Combo Lines")]
    public GameObject straightAcrossTopLeft;
    public GameObject straightAcrossTopRight;

    public GameObject straightAcrossMiddleLeft;
    public GameObject straightAcrossMiddleRight;

    public GameObject straightAcrossBottomLeft;
    public GameObject straightAcrossBottomRight;

    public GameObject straightDown1;
    public GameObject straightDown2;
    public GameObject straightDown3;
    public GameObject straightDown4;
    public GameObject straightDown5;

    public GameObject acrossLeftUp;
    public GameObject acrossLeftDown;
    public GameObject acrossRightUp;
    public GameObject acrossRightDown;

    [Header("Slot UI - Bet Buttons")]
    public GameObject betGroup;
    public GameObject button1; //button for betx1
    public GameObject button2; //button for betx10
    public GameObject button3; //button for betx100
    public GameObject button4; //button for betxMAX

    [Header("Slots Minigame Data")]
    public string[] dialogueLines = new string[] { "", "JACKPOT!"}; //dialogue the minigame can say
    public int dialogueIndex = 0; //number to call what dialogue is said
    public int ranRoll;

    public bool rollTimer; //is timer running
    public bool timerEnded; //if timer has ended
    public float rollTimerEnd = 0.5f; //what it takes to end
    public float rollTimerDuration; //how far into timer it is

    [Header("Slots Minigame Data - Rolling")]
    private bool roll1 = false;
    private bool roll2 = false;
    private bool roll3 = false;
    private bool roll4 = false;
    private bool roll5 = false;

    public bool rolling = false;
    public bool isScored = false;

    public int multiplier = 1;
    public int jackpot;

    private bool straightTopLeft = false;
    private bool straightTopRight = false;
    private bool straightMiddleLeft = false;
    private bool straightMiddleRight = false;
    private bool straightBottomLeft = false;
    private bool straightBottomRight = false;

    private bool straightDownCol1 = false;
    private bool straightDownCol2 = false;
    private bool straightDownCol3 = false;
    private bool straightDownCol4 = false;
    private bool straightDownCol5 = false;

    private bool diagonalLeftUp = false;
    private bool diagonalRightUp = false;
    private bool diagonalLeftDown = false;
    private bool diagonalRightDown = false;

    //Collumns Feed **THESE HOLD THE GAMEOBJECTS**
    [Header("Slots Minigame Data - Collumns Feed")]
    public GameObject[] collumn1 = new GameObject[3];
    public GameObject[] collumn2 = new GameObject[3];
    public GameObject[] collumn3 = new GameObject[3];
    public GameObject[] collumn4 = new GameObject[3];
    public GameObject[] collumn5 = new GameObject[3];

    //Collumns Feed **THESE HOLD THE INDEX OF THE SPRITES**
    public int[] feed1 = new int[3]; //the feed for collumn 1
    public int[] feed2 = new int[3]; //the feed for collumn 2
    public int[] feed3 = new int[3]; //the feed for collumn 3
    public int[] feed4 = new int[3]; //the feed for collumn 4
    public int[] feed5 = new int[3]; //the feed for collumn 5

    //CREDITS
    [Header("Slots Minigame Data - Credits")]
    public int credits; //credits whole number
    public string creditsString;
    public int ones;
    public int tens;
    public int hundreds;
    public int thousands;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeGameObjects(); //check to see if everything is linked in inspector, if not find the named objects and link them
        InitializeFeed(); //set the gameobjects to be the rows in the arrays
        LoadFeed(feed1);
        LoadFeed(feed2);
        LoadFeed(feed3);
        LoadFeed(feed4);
        LoadFeed(feed5);

        credits = PlayerPrefs.GetInt("credits");
        jackpot = PlayerPrefs.GetInt("jackpot");
    }

    // Update is called once per frame
    void Update()
    {
        PlayerPrefs.SetInt("credits", credits);
        PlayerPrefs.SetInt("jackpot", jackpot);
        jackPotText.text = jackpot.ToString();

        LoadCreditsUI();
        UpdateFeedSprites();

        if(Input.GetButtonDown("Cancel")) { CloseGame(); }

        if (playerNear && Input.GetButtonDown("Interact"))
        {
            isTalking = true;
        }

        //if talking to the Slot NPC
        if (isTalking && !gameActive)
        {
            playAgain = true;
            canMove = false; // you don't want player to be able to move around while Gambling so need to freeze movement
            ShowUI(slotScreen); //shows all ui related to slots
            dialogueIndex = 0;

            if (!playAgain) { CloseGame(); } //hides all ui related to slots
        }
        if (!isTalking) { canMove = true; } // return movement if not talking to minigame npc

        if(gameActive)
        {
            //this is where stuff that happens inside the game goes
            if(rolling)
            {
                if (!roll1)
                {
                    if (!timerEnded) { RollTimerStart(); }
                    if (rollTimer)
                    {
                        LoadFeed(feed1);
                    }
                    if (!rollTimer) { roll1 = true; timerEnded = true; }
                }

                if (!roll2)
                {
                    if (!timerEnded) { RollTimerStart(); }
                    if (rollTimer)
                    {
                        LoadFeed(feed2);
                    }
                    if (!rollTimer) { roll2 = true; timerEnded = true; }
                }

                if (!roll3)
                {
                    if (!timerEnded) { RollTimerStart(); }
                    if (rollTimer)
                    {
                        LoadFeed(feed3);
                    }
                    if (!rollTimer) { roll3 = true; timerEnded = true; }
                }

                if (!roll4)
                {
                    if (!timerEnded) { RollTimerStart(); }
                    if (rollTimer)
                    {
                        LoadFeed(feed4);
                    }
                    if (!rollTimer) { roll4 = true; timerEnded = true; }
                }

                if (!roll5)
                {
                    if (!timerEnded) { RollTimerStart(); }
                    if (rollTimer)
                    {
                        LoadFeed(feed5);
                    }
                    if (!rollTimer) { roll5 = true; timerEnded = true; }
                }

                if (roll1 && roll2 && roll3 && roll4 && roll5) { rolling = false; }
            }
            
            if(!rolling)
            {
                timerEnded = false;
                if(!isScored ) //start to measure scorelines
                {
                    ScoreLines();
                }
                if (isScored) { gameActive = false; }
            }

        }

        if(!gameActive) { roll1 = false; roll2 = false; roll3 = false; roll4 = false; roll5 = false; isScored = false; }

        if (rollTimer)
        {
            rollTimerDuration += Time.deltaTime;
            if (rollTimerDuration >= rollTimerEnd) { rollTimer = false; }
        }
        
    }

    //**SYSTEM FUNCTIONS** 
    public void ShowUI(GameObject UI)
    {
        UI.SetActive(true);
    }

    public void HideUI(GameObject UI)
    {
        UI.SetActive(false);
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

    public void InitializeGameObjects() //if anything is not linked in inspector, it will now be found
    {
        //SLOT UI BASE
        if (slotScreen == null) { slotScreen = FindInactiveObjectByName("SlotsScreen"); }
        if (dialogueUI == null) { dialogueUI = FindInactiveObjectByName("DialogueBG-Slots"); }
        if (dialogueText == null)
        {
            GameObject placeholder = FindInactiveObjectByName("DialogueBG-Slot-Text");
            dialogueText = placeholder.GetComponent<TextMeshProUGUI>();
        }
        if (lever == null) { lever = FindInactiveObjectByName("SlotsBG-Lever"); }

        //SLOT UI - CREDITS
        if (creditsPanel == null) { creditsPanel = FindInactiveObjectByName("CreditsBG"); }
        if (creditsNumber1 == null) { creditsNumber1 = FindInactiveObjectByName("Credits-Number1"); }
        if (creditsNumber2 == null) { creditsNumber2 = FindInactiveObjectByName("Credits-Number2"); }
        if (creditsNumber3 == null) { creditsNumber3 = FindInactiveObjectByName("Credits-Number3"); }
        if (creditsNumber4 == null) { creditsNumber4 = FindInactiveObjectByName("Credits-Number4"); }

        //SLOT UI - JACKPOT
        if (jackpotPanel == null) { creditsPanel = FindInactiveObjectByName("JackpotBG"); }
        if (jackPotText == null)
        {
            GameObject placeholder = FindInactiveObjectByName("Jackpot-Text");
            jackPotText = placeholder.GetComponent<TextMeshProUGUI>();
        }

        //SLOT UI - ROW 1
        if (r1c1 == null) { r1c1 = FindInactiveObjectByName("r1c1"); }
        if (r1c2 == null) { r1c2 = FindInactiveObjectByName("r1c2"); }
        if (r1c3 == null) { r1c3 = FindInactiveObjectByName("r1c3"); }
        if (r1c4 == null) { r1c4 = FindInactiveObjectByName("r1c4"); }
        if (r1c5 == null) { r1c1 = FindInactiveObjectByName("r1c1"); }

        //SLOT UI - ROW 2
        if (r2c1 == null) { r2c1 = FindInactiveObjectByName("r2c1"); }
        if (r2c2 == null) { r2c2 = FindInactiveObjectByName("r2c2"); }
        if (r2c3 == null) { r2c3 = FindInactiveObjectByName("r2c3"); }
        if (r2c4 == null) { r2c4 = FindInactiveObjectByName("r2c4"); }
        if (r2c5 == null) { r2c1 = FindInactiveObjectByName("r2c1"); }

        //SLOT UI - ROW 3
        if (r3c1 == null) { r3c1 = FindInactiveObjectByName("r3c1"); }
        if (r3c2 == null) { r3c2 = FindInactiveObjectByName("r3c2"); }
        if (r3c3 == null) { r3c3 = FindInactiveObjectByName("r3c3"); }
        if (r3c4 == null) { r3c4 = FindInactiveObjectByName("r3c4"); }
        if (r3c5 == null) { r3c1 = FindInactiveObjectByName("r3c1"); }

        //SLOT UI - COMBO LINES
        if (straightAcrossTopLeft == null) { straightAcrossTopLeft = FindInactiveObjectByName("StraightAcross-1-Left"); }
        if (straightAcrossTopRight == null) { straightAcrossTopRight = FindInactiveObjectByName("StraightAcross-1-Right"); }

        if (straightAcrossMiddleLeft == null) { straightAcrossMiddleLeft = FindInactiveObjectByName("StraightAcross-2-Left"); }
        if (straightAcrossMiddleRight == null) { straightAcrossMiddleRight = FindInactiveObjectByName("StraightAcross-2-Right"); }

        if (straightAcrossBottomLeft == null) { straightAcrossBottomLeft = FindInactiveObjectByName("StraightAcross-3-Left"); }
        if (straightAcrossBottomRight == null) { straightAcrossBottomRight = FindInactiveObjectByName("StraightAcross-3-Right"); }

        if (straightDown1 == null) { straightDown1 = FindInactiveObjectByName("StraightDown-1"); }
        if (straightDown2 == null) { straightDown2 = FindInactiveObjectByName("StraightDown-2"); }
        if (straightDown3 == null) { straightDown3 = FindInactiveObjectByName("StraightDown-3"); }
        if (straightDown4 == null) { straightDown4 = FindInactiveObjectByName("StraightDown-4"); }
        if (straightDown5 == null) { straightDown5 = FindInactiveObjectByName("StraightDown-5"); }

        if (acrossLeftUp == null) { acrossLeftUp = FindInactiveObjectByName("Across-Left-Up"); }
        if (acrossLeftDown == null) { acrossLeftDown = FindInactiveObjectByName("Across-Left-Down"); }
        if (acrossRightUp == null) { acrossRightUp = FindInactiveObjectByName("Across-Right-Up"); }
        if (acrossRightDown == null) { acrossRightDown = FindInactiveObjectByName("Across-Right-Down"); }

        //BlackJack UI - Betting Buttons
        if (betGroup == null) { betGroup = FindInactiveObjectByName("BET"); }
        if (button1 == null) { button1 = FindInactiveObjectByName("SlotsBG-Bet1"); }
        if (button2 == null) { button2 = FindInactiveObjectByName("SlotsBG-Bet10"); }
        if (button3 == null) { button3 = FindInactiveObjectByName("SlotsBG-Bet100"); }
        if (button4 == null) { button4 = FindInactiveObjectByName("SlotsBG-BetAll"); }
    }

    public void InitializeFeed()
    {
        //collumn 1
        collumn1[0] = r1c1;
        collumn1[1] = r2c1;
        collumn1[2] = r3c1;

        //collumn 2
        collumn2[0] = r1c2;
        collumn2[1] = r2c2;
        collumn2[2] = r3c2;

        //collumn 3
        collumn3[0] = r1c3;
        collumn3[1] = r2c3;
        collumn3[2] = r3c3;

        //collumn 4
        collumn4[0] = r1c4;
        collumn4[1] = r2c4;
        collumn4[2] = r3c4;

        //collumn 5
        collumn5[0] = r1c5;
        collumn5[1] = r2c5;
        collumn5[2] = r3c5;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
        Debug.Log("OnCollisionEnter2D");
        Debug.Log(other.gameObject.name + " : " + gameObject.name + " : " + Time.time);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
        Debug.Log("OnCollisionExit2D");
        Debug.Log(other.gameObject.name + " : " + gameObject.name + " : " + Time.time);
    }
    //**END SYSTEM FUNCTIONS**

    public void CloseGame() //hide all screens related to slots and save data
    {
        HideUI(slotScreen); //hide UI related
        gameActive = false;
        playAgain = false;
        isTalking = false;
        canMove = true;
    }

    //**CREDITS FUNCTIONS**
    public void LoadCreditsUI()
    {
        UpdateCreditsSprites(); //update the sprites for UI before showing and revealing them

        if (credits <= -100) { ShowUI(creditsNumber4); ShowUI(creditsNumber3); } //if credits in hundred or over but negative

        if (credits <= -10 && credits > -100) { ShowUI(creditsNumber3); ShowUI(creditsNumber2); HideUI(creditsNumber4); } //if credits in tens space but negative

        if (credits < 0 && credits > -10) { ShowUI(creditsNumber1); ShowUI(creditsNumber2); HideUI(creditsNumber3); } //if credits is in ones space but negative

        if (credits >= 0 && credits < 10) { ShowUI(creditsNumber1); }//if credits is in ones space

        if (credits >= 10 && credits < 100) { ShowUI(creditsNumber2); } //if credits is in tens space show second digit
        if (credits < 10 && credits > 0) { HideUI(creditsNumber2); } //if it is below that then hide it

        if (credits >= 100) { ShowUI(creditsNumber3); }  //if credits is in hundreds space show third digit
        if (credits < 100 && credits > 10) { HideUI(creditsNumber3); } //if it is below that then hide it

        if (credits >= 1000) { ShowUI(creditsNumber4); } //if credits is in thousands space
        if (credits < 1000 && credits > 100) { HideUI(creditsNumber4); } //if it is below that then hide it
    }

    public void UpdateCreditsSprites()
    {
        credits = PlayerPrefs.GetInt("credits");
        UpdateCreditsDigits();

        if (credits <= -100) //if credits in hundred or over but negative
        {
            creditsNumber4.GetComponent<Image>().sprite = creditSprites[10];
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[tens];
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[hundreds];
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[thousands];
        }

        if (credits <= -10 && credits > -100) //if credits in tens space but negative
        {
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[10];
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[tens];
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[hundreds];
        }

        if (credits < 0 && credits >= -9) //if credits is in ones space but negative
        {
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[10];
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[tens];
        }

        if (credits < 10 && credits >= 0) 
        {
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
        }

        if (credits > 9 && credits < 100)
        {
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[tens]; //update credit number sprite
        }

        if (credits > 99 && credits < 1000)
        {
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[tens]; //update credit number sprite
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[hundreds]; //update credit number sprite
        }

        if (credits > 999)
        {
            creditsNumber4.GetComponent<Image>().sprite = creditSprites[ones]; //update credit number sprite
            creditsNumber3.GetComponent<Image>().sprite = creditSprites[tens]; //update credit number sprite
            creditsNumber2.GetComponent<Image>().sprite = creditSprites[hundreds]; //update credit number sprite
            creditsNumber1.GetComponent<Image>().sprite = creditSprites[thousands]; //update credit number sprite
        }

    }

    public void UpdateCreditsDigits()
    {
        creditsString = credits.ToString(); //convert to string

        //**IF POSITIVE**//
        if(creditsString.Length == 4) //thousands
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            tens = creditsString[1];
            tens = CharToInt(tens);

            hundreds = creditsString[2];
            hundreds = CharToInt(hundreds);

            thousands = creditsString[3];
            thousands = CharToInt(thousands);
        }

        if (creditsString.Length == 3) //hundreds
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            tens = creditsString[1];
            tens = CharToInt(tens);

            hundreds = creditsString[2];
            hundreds = CharToInt(hundreds);

            //thousands = 0;
        }

        if (creditsString.Length == 2) //tens
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            tens = creditsString[1];
            tens = CharToInt(tens);

            //hundreds = 0;
            thousands = 0;
        }

        if (creditsString.Length == 1) //ones
        {
            ones = creditsString[0];
            ones = CharToInt(ones);

            //tens = 0;
            hundreds = 0;
            thousands = 0;
        }
    }

    public int CharToInt(int character)
    {
        if (character == 48) { return 0; }
        if (character == 49) { return 1; }
        if (character == 50) { return 2; }
        if (character == 51) { return 3; }
        if (character == 52) { return 4; }
        if (character == 53) { return 5; }
        if (character == 54) { return 6; }
        if (character == 55) { return 7; }
        if (character == 56) { return 8; }
        if (character == 57) { return 9; }
        if (character == 2212) { return 10; }
        else { return 0; }
    }
    //**END CREDITS FUNCTIONS**

    public void PullLever() //function called when player hits lever
    {
        if(gameActive == false) //if game not currently active, then it is now active
        {
            HideScoreLines();
            HideDialogue();

            //ensure you cant bet more than you have
            if (credits < 100 && multiplier >= 100) { multiplier = 1; }
            if (credits < 10 && multiplier >= 10) { multiplier = 1; }
            if (credits < 1 && multiplier >= 1) { multiplier = 1; }

            credits = credits - multiplier; //cost to play
            gameActive = true;
            rolling = true;
            Debug.Log("Lever Pulled & GameActive was False.");
        } 
        

        Debug.Log("Lever Pulled.");
        //else if true can maybe play a sound that shows you cant spin again yet
    }

    public void UpdateFeedSprites() //whatever value is saved in feed is an index used to update the sprite
    {
        //feed 1
        collumn1[0].GetComponent<Image>().sprite = slotSymbols[feed1[0]];
        collumn1[1].GetComponent<Image>().sprite = slotSymbols[feed1[1]];
        collumn1[2].GetComponent<Image>().sprite = slotSymbols[feed1[2]];

        //feed 2
        collumn2[0].GetComponent<Image>().sprite = slotSymbols[feed2[0]];
        collumn2[1].GetComponent<Image>().sprite = slotSymbols[feed2[1]];
        collumn2[2].GetComponent<Image>().sprite = slotSymbols[feed2[2]];

        //feed 3
        collumn3[0].GetComponent<Image>().sprite = slotSymbols[feed3[0]];
        collumn3[1].GetComponent<Image>().sprite = slotSymbols[feed3[1]];
        collumn3[2].GetComponent<Image>().sprite = slotSymbols[feed3[2]];

        //feed 4
        collumn4[0].GetComponent<Image>().sprite = slotSymbols[feed4[0]];
        collumn4[1].GetComponent<Image>().sprite = slotSymbols[feed4[1]];
        collumn4[2].GetComponent<Image>().sprite = slotSymbols[feed4[2]];

        //feed 5
        collumn5[0].GetComponent<Image>().sprite = slotSymbols[feed5[0]];
        collumn5[1].GetComponent<Image>().sprite = slotSymbols[feed5[1]];
        collumn5[2].GetComponent<Image>().sprite = slotSymbols[feed5[2]];
    }
    public void RandomRoll()
    {
        ranRoll = Random.Range(0, 11);
    }

    public void RollTimerStart()
    {
        rollTimerDuration = 0;
        //rollTimerEnd = Random.Range(0, .3f);
        rollTimer = true;
        timerEnded = true;
    }

    public void LoadFeed(int[] feed) 
    {
        // INDEX - SYMBOL NAME - POINT VALUE
        // 0 - apple - 10 points
        // 1 - apple GOLD - 50 points
        // 2 - banana - 10 points
        // 3 - blueberry - 10 points
        // 4 - cherry - 10 points
        // 5 - grapes - 10 points
        // 6 - lemon - 10 points
        // 7 - lime - 20 points
        // 8 - melon - 20 points
        // 9 - orange - 10 points
        // 10 - pear - 10 points
        // 11 - strawberry - 25 points

        // WHERE IS IT SAVED:
        // feed[i] - inspector - slotValues[i]

        //all feed should be index 0-11
        
        for (int i = 0; i < feed.Length; i++)
        {
            RandomRoll();
            feed[i] = ranRoll;
        }
    }

    public void ScoreLines()
    {
        //**JACKPOT**//
        if(straightTopLeft && straightTopRight && straightMiddleLeft && straightMiddleRight && straightBottomLeft && straightBottomRight && straightDownCol1 && straightDownCol2 && straightDownCol3 && straightDownCol4 && straightDownCol5 && diagonalLeftUp && diagonalLeftDown && diagonalRightUp && diagonalRightDown)
        {
            dialogueIndex = 1;
            StartDialogue();
            credits = credits + (multiplier * jackpot);
            jackpot = 500; //reset jackpot
        }

        //**IF NO WIN**
        if (!straightTopLeft && !straightTopRight && !straightMiddleLeft && !straightMiddleRight && !straightBottomLeft && !straightBottomRight && !straightDownCol1 && !straightDownCol2 && !straightDownCol3 && !straightDownCol4 && !straightDownCol5 && !diagonalLeftUp && !diagonalLeftDown && !diagonalRightUp && !diagonalRightDown)
        {
            jackpot = jackpot + multiplier; //add whatever you wagered to existing jackpot
        }


        //**STRAIGHT ACROSS**//
        if ((feed1[0] == feed2[0]) && (feed2[0] == feed3[0])) //Straight Across Top Left
        {
            ShowUI(straightAcrossTopLeft);
            credits = credits + (multiplier * slotValues[feed1[0]]) + multiplier;
            straightTopLeft = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed3[0] == feed4[0]) && (feed4[0] == feed5[0])) //Straight Across Top Right
        {
            ShowUI(straightAcrossTopRight);
            credits = credits + (multiplier * slotValues[feed3[0]]) + multiplier;
            straightTopRight = true;
            jackpot = 500; //reset jackpot
        }


        if ((feed1[1] == feed2[1]) && (feed2[1] == feed3[1])) //Straight Across Middle Left
        {
            ShowUI(straightAcrossMiddleLeft);
            credits = credits + (multiplier * slotValues[feed1[1]]) + multiplier;
            straightMiddleLeft = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed3[1] == feed4[1]) && (feed4[1] == feed5[1])) //Straight Across Middle Right
        {
            ShowUI(straightAcrossMiddleRight);
            credits = credits + (multiplier * slotValues[feed3[1]]) + multiplier;
            straightMiddleRight = true;
            jackpot = 500; //reset jackpot
        }


        if ((feed1[2] == feed2[2]) && (feed2[2] == feed3[2])) //Straight Across Bottom Left
        {
            ShowUI(straightAcrossBottomLeft);
            credits = credits + (multiplier * slotValues[feed1[2]]) + multiplier;
            straightBottomLeft = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed3[2] == feed4[2]) && (feed4[2] == feed5[2])) //Straight Across Bottom Right
        {
            ShowUI(straightAcrossBottomRight);
            credits = credits + (multiplier * slotValues[feed3[2]]) + multiplier;
            straightBottomRight = true;
            jackpot = 500; //reset jackpot
        }


        //**STRAIGHT DOWN**//
        if ((feed1[0] == feed1[1]) && (feed1[1] == feed1[2])) //Straight Down Collumn 1
        {
            ShowUI(straightDown1);
            credits = credits + (multiplier * slotValues[feed1[0]]) + multiplier;
            straightDownCol1 = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed2[0] == feed2[1]) && (feed2[1] == feed2[2])) //Straight Down Collumn 2
        {
            ShowUI(straightDown2);
            credits = credits + (multiplier * slotValues[feed2[0]]) + multiplier;
            straightDownCol2 = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed3[0] == feed3[1]) && (feed3[1] == feed3[2])) //Straight Down Collumn 3
        {
            ShowUI(straightDown3);
            credits = credits + (multiplier * slotValues[feed3[0]]) + multiplier;
            straightDownCol3 = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed4[0] == feed4[1]) && (feed4[1] == feed4[2])) //Straight Down Collumn 4
        {
            ShowUI(straightDown4);
            credits = credits + (multiplier * slotValues[feed4[0]]) + multiplier;
            straightDownCol4 = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed5[0] == feed5[1]) && (feed5[1] == feed5[2])) //Straight Down Collumn 5
        {
            ShowUI(straightDown5);
            credits = credits + (multiplier * slotValues[feed5[0]]) + multiplier;
            straightDownCol5 = true;
            jackpot = 500; //reset jackpot
        }


        //**ACROSS LEFT**//
        if ((feed1[2] == feed2[1]) && (feed2[1] == feed3[0])) //Across Left Up
        {
            ShowUI(acrossLeftUp);
            credits = credits + (multiplier * slotValues[feed1[2]]) + multiplier;
            diagonalLeftUp = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed1[0] == feed2[1]) && (feed2[1] == feed3[2])) //Across Left Down
        {
            ShowUI(acrossLeftDown);
            credits = credits + (multiplier * slotValues[feed1[0]]) + multiplier;
            diagonalLeftDown = true;
            jackpot = 500; //reset jackpot
        }

        //**ACROSS RIGHT**//
        if ((feed3[0] == feed4[1]) && (feed4[1] == feed5[2])) //Across Right Down
        {
            ShowUI(acrossRightDown);
            credits = credits + (multiplier * slotValues[feed3[0]]) + multiplier;
            diagonalRightDown = true;
            jackpot = 500; //reset jackpot
        }
        if ((feed3[2] == feed4[1]) && (feed4[1] == feed5[0])) //Across Right Up
        {
            ShowUI(acrossRightUp);
            credits = credits + (multiplier * slotValues[feed3[2]]) + multiplier;
            diagonalRightUp = true;
            jackpot = 500; //reset jackpot
        }
        
        isScored = true;
    }

    public void BetX1()
    {
        multiplier = 1;
    }
    public void BetX10()
    {
        if (credits >= 10) { multiplier = 10; }
        else { multiplier = 1; }
    }
    public void BetX100()
    {
        if (credits >= 100) { multiplier = 100; } 
        else { multiplier = 1; }
    }
    public void BetMax()
    {
        if(credits > 0) { multiplier = credits; }
        else { multiplier = 1; }
    }

    public void StartDialogue()
    {
        dialogueText.text = dialogueLines[dialogueIndex];
        ShowUI(dialogueUI);
    }

    public void HideDialogue()
    {
        HideUI(dialogueUI);
        dialogueIndex = 0;
    }
    public void HideScoreLines()
    {
        HideUI(straightAcrossTopLeft);
        HideUI(straightAcrossTopRight);
        HideUI(straightAcrossMiddleLeft);
        HideUI(straightAcrossMiddleRight);
        HideUI(straightAcrossBottomLeft);
        HideUI(straightAcrossBottomRight);

        HideUI(straightDown1);
        HideUI(straightDown2);
        HideUI(straightDown3);
        HideUI(straightDown4);
        HideUI(straightDown5);

        HideUI(acrossLeftUp);
        HideUI(acrossLeftDown);
        HideUI(acrossRightUp);
        HideUI(acrossRightDown);
        
        straightTopLeft = false;
        straightTopRight = false;
        straightMiddleLeft = false;
        straightMiddleRight = false;
        straightBottomLeft = false;
        straightBottomRight = false;

        straightDownCol1 = false;
        straightDownCol2 = false;
        straightDownCol3 = false;
        straightDownCol4 = false;
        straightDownCol5 = false;

        diagonalLeftUp = false;
        diagonalRightUp = false;
        diagonalLeftDown = false;
        diagonalRightDown = false;
}
}
