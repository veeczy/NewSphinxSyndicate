using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
//using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

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
    public Image FillBar;

    [Header("Fish UI - Game (WaitingScreen)")]
    public GameObject water;
    public GameObject reel;
    public GameObject reelBase;
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

    [Header("Fishing Minigame Data - Fish")]
    public string fishSpecies; // what species fish is active, will be found by fishSpecies = fishgroup[index]
    public int fishSpeciesIndex = 0; //index of whatever fish group you need, calculated using random of whichever group you want
    public int ranSize; // random number that determines fish size
    public int ranSkill; // random number that determines fish difficulty

    public bool fishSpawned = false;
    public bool hooked = false;

    public Sprite[] fishSmallSprite;
    public string[] fishSmallSpecies;

    public Sprite[] fishMediumSprite;
    public string[] fishMediumSpecies;

    public Sprite[] fishLargeSprite;
    public string[] fishLargeSpecies;

    public enum FishSizeType { Small, Medium, Large }
    public FishSizeType fishSize = FishSizeType.Medium; //defaults to Medium Size Fish
    public enum FishSkillType { Easy, Normal, Hard }
    public FishSkillType fishDifficulty = FishSkillType.Normal; //defaults to Normal Difficulty Fish

    [Header("Fishing Minigame Data - Timer")]
    public float catchDuration = 10f; //this is the total duration for the timer for catching the fish - how long should you wait before a fish gets away
    public float catchremainingDuration; //this is the measure for how far you are in the timer itself for catching fish

    public float spawnTimerDuration = 10f; //this is the total duration for the timer for spawning the fish - it should vary randomly using the ran function
    public float spawnremainingDuration; //this is the measure for how far you are in the timer itself for spawning fish

    public bool ongoingTimer = false;

    [Header("Fishing Minigame Data - Reel")]
    public bool minigameScreenActive = false;
    public float angle;
    public Vector2 aimDir;
    public Vector2 aimPos;
    public Vector2 reelPos;
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
            aimPos = player.GetComponent<PlayerMovement>().aimPos;
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

        if (gameActive && !minigameScreenActive)
        {
            if(Input.GetButtonDown("Shoot") && fishSpawned) { hooked = true; } //if press when fish is spawned, you hook fish

            //spawning for fish
            if(!fishSpawned && !ongoingTimer) { StartTimer(spawnTimerDuration, spawnremainingDuration); }
            if(fishSpawned) { SpawnFish(); }

            if(hooked) { SplashScreen(); }
        }

        if (minigameScreenActive)
        {
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
        FillBar = UITimerFill.GetComponent<Image>();

        //FISH UI - Waiting Screen
        if (waitingScreen == null) { waitingScreen = FindInactiveObjectByName("WaitingScreen"); } //the parent for entire waiting screen
        if (water == null) { water = FindInactiveObjectByName("water"); } //the parent for the water

        //FISH UI - Waiting Screen - Fishing Rod
        if (fishingRod == null) { fishingRod = FindInactiveObjectByName("FishingRod"); } //the fishing rod parent object - includes all decorative for the pole
        if (reel == null) { reel = FindInactiveObjectByName("reel"); } //the reel of the fishing rod
        if (reelBase == null) { reelBase = FindInactiveObjectByName("reel-base"); } //the base circle for the reel of the fishing rod
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

    private void StartTimer(float Duration, float RemainingDuration)
    {
        if(!fishSpawned) { Duration = UnityEngine.Random.Range(1f, 30f); } //if fish not spawned, timer waiting for fish
        if(fishSpawned) { Duration = UnityEngine.Random.Range(0.5f, 7f); } //if fish spawned, timer for hooking it
        RemainingDuration = Duration;
        if(!ongoingTimer)
        {
            ongoingTimer = true;
            StartCoroutine(UpdateTimer(Duration, RemainingDuration));
        }
        ShowUI(timerGroup);
    }

    private IEnumerator UpdateTimer(float Duration, float RemainingDuration)
    {
        while(RemainingDuration >= 0)
        {
            ongoingTimer = true;
            if(!hooked)
            {
                FillBar.fillAmount = Mathf.InverseLerp(0, Duration, RemainingDuration);
                RemainingDuration--;
                yield return new WaitForSeconds(1f);
            }
            yield return null;
        }
        EndTimer();
    }

    private void EndTimer()
    {
        if (fishSpawned) { fishSpawned = false; HideUI(fishShadowSmall); HideUI(fishShadowMedium); HideUI(fishShadowLarge); } //if fish spawn is true, it means this timer is for hooking and if you dont do it in time the fish gets away
        else if (!fishSpawned) { fishSpawned = true; hooked = false; } //if fish spawn is false, it means this timer is for spawning said fish and needs to be turned true when done;
        ongoingTimer = false;
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
        //randomly generate fish
        if(!ongoingTimer)
        {
            ranSize = UnityEngine.Random.Range(1, 3);
            ranSkill = UnityEngine.Random.Range(1, 3);
        }
        

        if(ranSize == 1)
        {
            fishSpeciesIndex = UnityEngine.Random.Range(0, fishSmallSpecies.Length); 
            fishSpecies = fishSmallSpecies[fishSpeciesIndex];
            ShowUI(fishShadowSmall);
        }

        if (ranSize == 2)
        {
            fishSpeciesIndex = UnityEngine.Random.Range(0, fishMediumSpecies.Length);
            fishSpecies = fishMediumSpecies[fishSpeciesIndex];
            ShowUI(fishShadowMedium);
        }

        if (ranSize == 3)
        {
            fishSpeciesIndex = UnityEngine.Random.Range(0, fishLargeSpecies.Length);
            fishSpecies = fishLargeSpecies[fishSpeciesIndex];
            ShowUI(fishShadowLarge);
        }

        //start timer for fish to unspawn
        StartTimer(catchDuration, catchremainingDuration);
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
        //show off animation that fish is hooked
        MinigameScreen();
    }

    public void MinigameScreen() //trigger screen for minigame
    {
        minigameScreenActive = true;

        //screens
        ShowUI(fishScreen); // background
        HideUI(waitingScreen); // hide waiting screen
        HideUI(splashScreen); // hide splash screens
        ShowUI(minigameScreen); // current screen
        HideUI(catchScreen); // hide future screens

        //catch timer
        HideUI(timerGroup);

        //hide buttons
        HideUI(button1);
        HideUI(button2);


    }

    public void CatchScreen() //trigger screen for when fish is caught - win screen
    {
        //change fish sprite to the fish species
        //change dialogue to say fish name
        //onWinLose reuse so when you click it closes and asks if you want to play again
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
