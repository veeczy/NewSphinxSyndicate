using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
//using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Fishing : MonoBehaviour
{
    [Header("Game Settings")]
    public bool isTalking = false;
    public bool canMove = true;
    public bool playAgain = true;
    public bool gameActive = false;
    public bool playerNear = false;
    private GameObject player; 


    [Header("Fish UI - Base")]
    public GameObject fishScreen; //background screen / underneath everything
    public GameObject waitingScreen; //screen for when waiting for fish to spawn / until you catch it
    public GameObject splashScreen; //screen for when a fish is caught - decorative
    public GameObject minigameScreen; //screen for the minigame portion, where you deal with the minigame itself
    public GameObject catchScreen; //screen for when fish is caught / win screen

    [Header("Fish UI - Dialogue")]
    public GameObject dialogueUI; //background panel for dialogue
    public TMP_Text dialogueText; //plays jackpot text and any other flavor text

    [Header("Fish UI - Timer")]
    public GameObject timerGroup; //the parent for the timer objects
    public GameObject UITimer; //timer (the background)
    public GameObject UITimerFill; //this is the fill for the timer as it progresses

    [Header("Fish UI - Game (WaitingScreen)")]
    public GameObject water;
    public GameObject reel;
    public GameObject spool;
    public GameObject fishingRod;
    public GameObject fishShadowLarge;
    public GameObject fishShadowMedium;
    public GameObject fishShadowSmall;

    [Header("Fish UI - Game (MinigameScreen)")]
    public GameObject trackArea;
    public GameObject successZone;
    public GameObject tracker;

    [Header("Fish UI - Buttons")]
    public GameObject button1; // yes
    public GameObject button2; // no

    [Header("Fishing Minigame Data")]
    public string[] dialogueLines = new string[] { "", "Cast your line?", "HOOKED!" }; //dialogue the minigame can say
    public int dialogueIndex = 0; //number to call what dialogue is said

    [Header("Fishing Minigame Data - Timer")]
    public float catchDuration; //this is the total duration for the timer for catching the fish - how long should you wait before a fish gets away
    public float remainingDuration; //this is the measure for how far you are in the timer itself

    [Header("Fishing Minigame Data - Reel")]
    public float angle;
    public Vector2 aimDir;
    public Vector2 aimPos;
    public Vector2 reelPos;
    //public bool controller;
    public float controllerTurnSpeed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeGameObjects();  
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            //angle = player.GetComponent<PlayerMovement>().angle;
            //aimDir = player.GetComponent<PlayerMovement>().aimDir;
            aimPos = player.GetComponent<PlayerMovement>().aimPos;
            controllerTurnSpeed = player.GetComponent<PlayerMovement>().controllerTurnSpeed;
        }

        if (Input.GetButtonDown("Cancel")) { CloseGame(); }

        if (playerNear && Input.GetButtonDown("Interact"))
        {
            isTalking = true;
        }

        //if talking to the Slot NPC
        if (isTalking && !gameActive)
        {
            playAgain = true;
            canMove = false; // you don't want player to be able to move around while Gambling so need to freeze movement
            FishScreen(); //set up screen

            if (!playAgain) { CloseGame(); } //hides all ui related to slots
        }
        if (!isTalking) { canMove = true; } // return movement if not talking to minigame npc

        if (gameActive)
        {
            //this is where stuff that happens inside the game goes

            //REELING
            reelPos = Camera.main.ScreenToWorldPoint(reel.transform.position); // get the reel position from the camera (its UI so it is converted to world space)
            aimDir = (Vector2)aimPos - (Vector2)reelPos; // recalculate the aim dir using the reel position
            angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg; // recalculate angle using the reel position
            reel.transform.rotation = Quaternion.Lerp(reel.transform.rotation, Quaternion.Euler(0, 0, angle), controllerTurnSpeed * Time.deltaTime); //this should rotate the reel
        }

        if (!gameActive)
        {
            //reset data when game is not active
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
        //FISH UI BASE
        if (fishScreen == null) { fishScreen = FindInactiveObjectByName("FishingScreen"); }
        if (dialogueUI == null) { dialogueUI = FindInactiveObjectByName("DialogueBG-Fish"); }
        if (dialogueText == null)
        {
            GameObject placeholder = FindInactiveObjectByName("DialogueBG-Fish-Text");
            dialogueText = placeholder.GetComponent<TextMeshProUGUI>();
        }

        //FISH UI - TIMER
        if (timerGroup == null) { timerGroup = FindInactiveObjectByName("timer"); } //the parent for the timer objects
        if (UITimer == null) { UITimer = FindInactiveObjectByName("timerBackground"); } //timer (the background)
        if (UITimerFill == null) { UITimerFill = FindInactiveObjectByName("timerFill"); } //this is the fill for the timer as it progresses

        //FISH UI - Waiting Screen
        if (waitingScreen == null) { waitingScreen = FindInactiveObjectByName("WaitingScreen"); } //the parent for entire waiting screen
        if (water == null) { water = FindInactiveObjectByName("water"); } //the parent for the water

        //FISH UI - Waiting Screen - Fishing Rod
        if (fishingRod == null) { fishingRod = FindInactiveObjectByName("FishingRod"); } //the fishing rod parent object - includes all decorative for the pole
        if (reel == null) { reel = FindInactiveObjectByName("reel"); } //the reel of the fishing rod
        if (spool == null) { spool = FindInactiveObjectByName("spool"); } //the spool of the fishing rod

        //FISH UI - Waiting Screen - Fish Shadow
        if (fishShadowLarge == null) { fishShadowLarge = FindInactiveObjectByName("fishShadow-Large"); }
        if (fishShadowMedium == null) { fishShadowMedium = FindInactiveObjectByName("fishShadow-Medium"); }
        if (fishShadowSmall == null) { fishShadowSmall = FindInactiveObjectByName("fishShadow-Small"); }

        //FISH UI - Splash Screen
        if (splashScreen == null) { splashScreen = FindInactiveObjectByName("SplashScreen"); }

        //FISH UI - Minigame Screen
        if (minigameScreen == null) { minigameScreen = FindInactiveObjectByName("MinigameScreen"); }
        if (trackArea == null) { trackArea = FindInactiveObjectByName("trackArea"); }
        if (successZone == null) { successZone = FindInactiveObjectByName("successZone"); }
        if (tracker == null) { tracker = FindInactiveObjectByName("tracker"); }

        //FISH UI - Catch Screen
        if (catchScreen == null) { catchScreen = FindInactiveObjectByName("CatchScreen"); }

        //FISH UI - Buttons
        if (button1 == null) { button1 = FindInactiveObjectByName("Button1"); }
        if (button2 == null) { button2 = FindInactiveObjectByName("Button2"); }

        //Player
        if (player == null) { player = GameObject.Find("Player"); }
    }

    public bool ReturnMovement()
    {
        return canMove;
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

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            player = other.gameObject;
        }
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

    public void SpawnFish()
    {

    }

    public void FishScreen() //trigger base screen - asks if you want to play
    {
        ShowUI(fishScreen); //shows all ui related to fish
        ShowUI(water);
        //hide all other existing screens in case any is open to reset sceen
        HideUI(waitingScreen); 
        HideUI(splashScreen);
        HideUI(minigameScreen);
        HideUI(catchScreen);

        dialogueIndex = 1; //ask if they want to cast a line
        StartDialogue();

        ShowUI(button1); //show button for clicking yes - hitting this should call WaitingScreen()
        ShowUI(button2); //show button for clicking no - hitting this should call CloseGame()
    }

    public void WaitingScreen() //trigger screen for waiting for fish to spawn
    {
        ShowUI(fishScreen); // background
        ShowUI(waitingScreen); // current screen
        HideUI(splashScreen); // hide future screens
        HideUI(minigameScreen); // hide future screens
        HideUI(catchScreen); // hide future screens

        //data for the screen
        gameActive = true;
        HideDialogue(); // clear dialogue

        //water
        //ShowUI(water); //decorative

        //fishing rod
        ShowUI(fishingRod); // decorative
        ShowUI(reel); // possible gameplay lets you move
        ShowUI(spool); // decorative

        //fish spawn shadow
        HideUI(fishShadowLarge);
        HideUI(fishShadowMedium);
        HideUI(fishShadowSmall);

        //catch timer
        HideUI(timerGroup);

        //hide buttons
        HideUI(button1);
        HideUI(button2);
    }

    public void SplashScreen() //trigger screen for when fish is hooked - decorative
    {

    }

    public void MinigameScreen() //trigger screen for minigame
    {

    }

    public void CatchScreen() //trigger screen for when fish is caught - win screen
    {

    }

    public void ResetGame() //reset all game data
    {

    }

    public void CloseGame() //hide all screens related to fishing and save data
    {
        HideUI(fishScreen); //hide UI related

        ResetGame(); //reset game data

        gameActive = false;
        playAgain = false;
        isTalking = false;
        canMove = true;
    }
}
