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
    public bool onWinLose = false;
    private GameObject player; 


    [Header("Fish UI - Base")]
    public GameObject fishScreenBG; //background screen / underneath everything
    public GameObject fishScreen; //fish start screen (holds buttons)
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
    public GameObject trackFill;
    public Image trackFillBar;

    public GameObject successZone;
    public GameObject successZoneHB;
    public GameObject successZoneEasy;
    public GameObject successZoneNormal;
    public GameObject successZoneHard;

    public GameObject marker;

    public GameObject bobber;
    public GameObject bobberString;

    public GameObject starGroup;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    public GameObject reelGroup;
    public GameObject reel1;
    public GameObject reel2;
    public GameObject reel3;
    public GameObject reel4;
    public GameObject reel5;
    public GameObject reel6;
    public GameObject reel7;
    public GameObject reel8;

    public Sprite[] starSprites;
    public Sprite[] reelSprites;

    public bool star1Hit;
    public bool star2Hit;
    public bool star3Hit;
    public bool star4Hit;
    public bool star5Hit;
    public bool star6Hit;
    public bool star7Hit;
    public bool star8Hit;

    [Header("Fish UI - Game (SplashScreen)")]
    public GameObject fishHookedText;
    public GameObject particlesGroup;

    [Header("Fish UI - Game (CatchScreen)")]
    public GameObject fishCaughtText;
    public GameObject displayFish;
    public GameObject displayFishTextPanel;
    public TMP_Text displayFishText;
    public TMP_Text displayFishTextShadow;

    [Header("Fish UI - Buttons")]
    public GameObject button1; // yes
    public GameObject button2; // no

    [Header("Fishing Minigame Data")]
    public string[] dialogueLines = new string[] { "", "Cast your line?", "HOOKED!" }; //dialogue the minigame can say
    public int dialogueIndex = 0; //number to call what dialogue is said
    private Animator anim;
    public int rotation = 0; //this measures how many times you have reeled a full rotation

    [Header("Fishing Minigame Data - Fish")]
    public string fishSpecies; // what species fish is active, will be found by fishSpecies = fishgroup[index]
    public int fishSpeciesIndex = 0; //index of whatever fish group you need, calculated using random of whichever group you want
    public int ranSize; // random number that determines fish size
    public int ranSkill; // random number that determines fish difficulty

    public bool fishSpawned = false; //has fish spawned but not reeled
    public bool fishHasSpawned = false; //has fish been rolled at least once
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
    public bool fishScreenActive = false;
    public bool splashScreenActive = false;
    public bool minigameScreenActive = false;
    public bool catchScreenActive = false;
    public float angle;

    public bool hasDecreased = false;
    public bool reelTimeComplete = false;
    public float fishPull;
    public int hitZoneValue;

    public bool fishCatch1 = false;
    public bool fishCatch2 = false;
    public bool fishCatch3 = false;

    public Vector2 aimDir;
    public Vector2 aimPos;
    public Vector2 reelPos;
    public float controllerTurnSpeed;

    // new CTRL f and type "new" to find all the new controller code
    [Header("Controller Fishing")]
    public string controllerClickButton = "Submit";
    public string controllerReelX = "Joystick Aim X";
    public string controllerReelY = "Joystick Aim Y";
    public float controllerReelDistance = 5f;
    public float controllerDeadzone = 0.4f;
    public float controllerReelAngle = 90f;
    public float controllerReelRotateSpeed = 8f;

    // new variable to prevent player from immediately reopening the mini game
    public float reopenDelay = 0.3f;
    private float canOpenAgainTime = 0f;


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

        // new code to open mini game, checks if player is near and presses interact button and also checks if enough time has passed since last closing to prevent immediate reopening with controller
        if (playerNear && Input.GetButtonDown("Interact") && Time.time >= canOpenAgainTime)
        {
            isTalking = true;
        }

        //if talking to the Slot NPC
        if (isTalking && !gameActive)
        {
            playAgain = true;
            canMove = false; // you don't want player to be able to move around while Gambling so need to freeze movement
            if (!fishScreenActive) { FishScreen(); } //set up screen

            if (!playAgain) { CloseGame(); } //hides all ui related to slots
        }
        if (!isTalking) { canMove = true; } // return movement if not talking to minigame npc

        if (gameActive && !minigameScreenActive && !catchScreenActive)
        {
            if ((Input.GetButton("Shoot") || Input.GetButtonDown(controllerClickButton)) && fishSpawned)
            {
                hooked = true;
            } //if press when fish is spawned, you hook fish

            //spawning for fish
            if (!fishSpawned && !ongoingTimer) { StartTimer(spawnTimerDuration, spawnremainingDuration); }
            if(fishSpawned && !splashScreenActive && !fishHasSpawned) { SpawnFish(); }

            if(hooked && !splashScreenActive) { splashScreenActive = true; SplashScreen(); }
        }

        if (minigameScreenActive)
        {

            //REELING
            reelPos = Camera.main.ScreenToWorldPoint(reel.transform.position); // get the reel position from the camera (its UI so it is converted to world space)

            // new use right thumb stick
            float reelInput = Input.GetAxisRaw(controllerReelX);

            if (Mathf.Abs(reelInput) > controllerDeadzone)
            {
                controllerReelAngle -= reelInput * controllerReelRotateSpeed;

                if (controllerReelAngle > 360f) controllerReelAngle -= 360f;
                if (controllerReelAngle < 0f) controllerReelAngle += 360f;

                float rad = controllerReelAngle * Mathf.Deg2Rad;
                aimPos = (Vector2)reelPos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * controllerReelDistance;
            }

            aimDir = (Vector2)aimPos - (Vector2)reelPos; // recalculate the aim dir using the reel position
            angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg; // recalculate angle using the reel position
            reel.transform.rotation = Quaternion.Lerp(reel.transform.rotation, Quaternion.Euler(0, 0, angle), controllerTurnSpeed * Time.deltaTime); //this should rotate the reel

            //reeling
            Reeling();
            StartCoroutine(WaitTime(fishPull));
            if(rotation < 0) { rotation = 0; } //if rotation becomes negative, set it to zero
            if(rotation == hitZoneValue) { CatchProgress(); }
        }

        if(catchScreenActive && fishCatch3 && !ongoingTimer && gameActive)
        {
            //StartCoroutine(WaitTime(5f));
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
        Debug.Log("Initialized Game Objects.");

        //FISH UI BASE
        if (fishScreenBG == null) { fishScreenBG = FindInactiveObjectByName("FishingScreen"); }
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

        //FISH UI - Fishing Screen
        if (fishScreen == null) { fishScreen = FindInactiveObjectByName("fishScreen"); }

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
        if (fishHookedText == null) { fishHookedText = FindInactiveObjectByName("FishHookedGroup"); }
        if (particlesGroup == null) { particlesGroup = FindInactiveObjectByName("Particles"); }

        //FISH UI - Minigame Screen
        if (minigameScreen == null) { minigameScreen = FindInactiveObjectByName("MinigameScreen"); }
        if (trackArea == null) { trackArea = FindInactiveObjectByName("trackArea"); }
        if (trackFill == null) { trackFill = FindInactiveObjectByName("trackFill"); }
        trackFillBar = trackFill.GetComponent<Image>();

        if (successZone == null) { successZone = FindInactiveObjectByName("successZone"); }
        if (successZoneEasy == null) { successZoneEasy = FindInactiveObjectByName("successZone-Easy"); }
        if (successZoneNormal == null) { successZoneNormal = FindInactiveObjectByName("successZone-Medium"); }
        if (successZoneHard == null) { successZoneHard = FindInactiveObjectByName("successZone-Hard"); }

        if (marker == null) { marker = FindInactiveObjectByName("marker"); }

        if (bobber == null) { bobber = FindInactiveObjectByName("Bobber"); }
        if (bobberString == null) { bobberString = FindInactiveObjectByName("FishString"); }

        //FISH UI - Minigame Screen - Reel Points
        if (reelGroup == null) { reelGroup = FindInactiveObjectByName("ReelPoints"); }
        if (reel1 == null) { reel1 = FindInactiveObjectByName("Reel1"); }
        if (reel2 == null) { reel2 = FindInactiveObjectByName("Reel2"); }
        if (reel3 == null) { reel3 = FindInactiveObjectByName("Reel3"); }
        if (reel4 == null) { reel4 = FindInactiveObjectByName("Reel4"); }
        if (reel5 == null) { reel5 = FindInactiveObjectByName("Reel5"); }
        if (reel6 == null) { reel6 = FindInactiveObjectByName("Reel6"); }
        if (reel7 == null) { reel7 = FindInactiveObjectByName("Reel7"); }
        if (reel8 == null) { reel8 = FindInactiveObjectByName("Reel8"); }

        //FISH UI - Minigame Screen - Stars
        if (starGroup == null) { starGroup = FindInactiveObjectByName("StarSpace"); }
        if (star1 == null) { star1 = FindInactiveObjectByName("Star1"); }
        if (star2 == null) { star2 = FindInactiveObjectByName("Star2"); }
        if (star3 == null) { star3 = FindInactiveObjectByName("Star3"); }

        //FISH UI - Catch Screen
        if (catchScreen == null) { catchScreen = FindInactiveObjectByName("CatchScreen"); }
        if (displayFish == null) { displayFish = FindInactiveObjectByName("DisplayFish"); }
        if (displayFishTextPanel == null) { displayFishTextPanel = FindInactiveObjectByName("DisplayFishTextPanel"); }
        if (displayFishText == null)
        {
            GameObject placeholder = FindInactiveObjectByName("DisplayFishText");
            displayFishText = placeholder.GetComponent<TextMeshProUGUI>();
        }
        if (displayFishTextShadow == null)
        {
            GameObject placeholder = FindInactiveObjectByName("DisplayFishTextShadow");
            displayFishTextShadow = placeholder.GetComponent<TextMeshProUGUI>();
        }

        //FISH UI - Buttons
        if (button1 == null) { button1 = FindInactiveObjectByName("Button1"); }
        if (button2 == null) { button2 = FindInactiveObjectByName("Button2"); }

        //Player
        if (player == null) { player = GameObject.Find("Player"); }
    }

    public void AnimateText(GameObject TextObject, bool Active)
    {
        //this is where we make the text bounce using code
        anim = TextObject.GetComponent<Animator>(); //retrive animator for object
        anim.SetBool("ScreenActive", Active); //set bool for object
    }

    public void UpdateSprite(GameObject Object, Sprite NewSprite)
    {
        Object.GetComponent<Image>().sprite = NewSprite;
    }

    private void StartTimer(float Duration, float RemainingDuration) //visible timer
    {
        if(!fishSpawned) { Duration = UnityEngine.Random.Range(1f, 7f); fishHasSpawned = false; } //if fish not spawned, timer waiting for fish
        if(fishSpawned) { Duration = UnityEngine.Random.Range(0.5f, 4f); } //if fish spawned, timer for hooking it
        RemainingDuration = Duration;
        if(!ongoingTimer)
        {
            Debug.Log("Timer has Started.");
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
        Debug.Log("Timer has ended.");
        if (fishSpawned) { fishSpawned = false; HideUI(fishShadowSmall); HideUI(fishShadowMedium); HideUI(fishShadowLarge); } //if fish spawn is true, it means this timer is for hooking and if you dont do it in time the fish gets away
        else if (!fishSpawned) { fishSpawned = true; hooked = false; } //if fish spawn is false, it means this timer is for spawning said fish and needs to be turned true when done;
        ongoingTimer = false;
    }

    private IEnumerator WaitTime(float Duration) //invisible timer
    {
        //Debug.Log("Wait Time is Called.");

        if (splashScreenActive) { yield return new WaitForSeconds(Duration); MinigameScreen(); } //if this is timer for splash screen, then it needs to call minigame screen when done
        if (minigameScreenActive && !reelTimeComplete) { reelTimeComplete = true; hasDecreased = false; yield return new WaitForSeconds(Duration); DecreaseRotation(); } //if this is timer for reeling, then it decreases rotation as time passes
        if (fishCatch3 && !catchScreenActive && !fishScreenActive && minigameScreenActive) {yield return new WaitForSeconds(Duration); CatchScreen(); } //if got all three reel, small delay to show star got before changing scenes
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

    public void DecreaseRotation()
    {
        if (!hasDecreased) { rotation--; hasDecreased = true; reelTimeComplete = false; }
        return;
    }

    public void CatchProgress()
    {
        Debug.Log("Fish Catch Progress Increase.");
        if (!fishCatch1 && rotation == hitZoneValue) { fishCatch1 = true; UpdateSprite(star1, starSprites[1]); rotation = 0; hitZoneValue = UnityEngine.Random.Range(7, 16); } //first star
        if(!fishCatch2 && fishCatch1 && rotation == hitZoneValue) { fishCatch2 = true; UpdateSprite(star2, starSprites[1]); rotation = 0; hitZoneValue = UnityEngine.Random.Range(7, 16); } //second star
        if(!fishCatch3 && fishCatch2 && rotation == hitZoneValue) { fishCatch3 = true; UpdateSprite(star3, starSprites[1]); SuccessCaught(); } //third star
    }

    public void SuccessCaught()
    {
        Debug.Log("Fish Caught Successfully!");
        StartCoroutine(WaitTime(.25f));
    }

    public void SpawnFish()
    {
        fishHasSpawned = true;
        if(!ongoingTimer)
        {
            Debug.Log("Fish Spawned.");
            ranSize = UnityEngine.Random.Range(1, 3);
            ranSkill = UnityEngine.Random.Range(1, 3);
            hitZoneValue = UnityEngine.Random.Range(7, 14);
        }

        if(ranSkill == 1) //if difficulty easy
        {
            Debug.Log("Fish is Easy Difficulty.");
            fishPull = 1.5f;
            successZoneHB = successZoneEasy;
        }

        if(ranSkill == 2) //if difficulty normal
        {
            Debug.Log("Fish is Normal Difficulty.");
            fishPull = 1.35f;
            successZoneHB = successZoneNormal;
        }

        if(ranSkill == 3) //if difficulty hard
        {
            Debug.Log("Fish is Hard Difficulty.");
            fishPull = 1.2f;
            successZoneHB = successZoneHard;
        }

        if(ranSize == 1) //if fish small
        {
            Debug.Log("Fish is Small.");
            fishSpeciesIndex = UnityEngine.Random.Range(0, fishSmallSpecies.Length); 
            fishSpecies = fishSmallSpecies[fishSpeciesIndex];
            displayFish.GetComponent<Image>().sprite = fishSmallSprite[fishSpeciesIndex];
            ShowUI(fishShadowSmall);
        }

        if (ranSize == 2) //if fish medium
        {
            Debug.Log("Fish is Medium.");
            fishSpeciesIndex = UnityEngine.Random.Range(0, fishMediumSpecies.Length);
            fishSpecies = fishMediumSpecies[fishSpeciesIndex];
            displayFish.GetComponent<Image>().sprite = fishMediumSprite[fishSpeciesIndex];
            ShowUI(fishShadowMedium);
        }

        if (ranSize == 3) //if fish large
        {
            Debug.Log("Fish is Large.");
            fishSpeciesIndex = UnityEngine.Random.Range(0, fishLargeSpecies.Length);
            fishSpecies = fishLargeSpecies[fishSpeciesIndex];
            displayFish.GetComponent<Image>().sprite = fishLargeSprite[fishSpeciesIndex];
            ShowUI(fishShadowLarge);
        }

        //start timer for fish to unspawn
        StartTimer(catchDuration, catchremainingDuration);
    }

    public void FishScreen() //trigger base screen - asks if you want to play
    {
        //ResetGame();
        Debug.Log("Fish Screen Opened.");
        fishScreenActive = true;

        //hide all other existing screens in case any is open to reset sceen
        HideUI(waitingScreen); 
        HideUI(splashScreen);
        HideUI(minigameScreen);
        HideUI(catchScreen);
        HideUI(displayFish);

        ShowUI(fishScreenBG); //shows all ui related to fish
        ShowUI(fishScreen); //fishing screen for start
        ShowUI(water);
        

        dialogueIndex = 1; //ask if they want to cast a line
        StartDialogue();

        ShowUI(button1); //show button for clicking yes - hitting this should call WaitingScreen()
        ShowUI(button2); //show button for clicking no - hitting this should call CloseGame()
        
    }

    public void WaitingScreen() //trigger screen for waiting for fish to spawn
    {
        Debug.Log("Waiting Screen opened.");
        fishScreenActive = false;

        ShowUI(fishScreenBG); // background
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
        HideUI(fishScreen);
    }

    public void SplashScreen() //trigger screen for when fish is hooked - decorative
    {
        Debug.Log("Splash Screen Opened.");
        splashScreenActive = true;

        //show off animation that fish is hooked
        ShowUI(fishScreenBG); // background
        HideUI(waitingScreen); // hide waiting screen
        ShowUI(splashScreen); // show splash screens
        HideUI(minigameScreen); // hide future screens
        HideUI(catchScreen); // hide future screens

        //hide catch timer
        HideUI(timerGroup);

        //hide buttons
        HideUI(button1);
        HideUI(button2);
        HideUI(fishScreen);

        ShowUI(fishHookedText);
        ShowUI(particlesGroup);
        AnimateText(fishHookedText, splashScreenActive);

        //delay then go to next
        StartCoroutine(WaitTime(1.5f));
    }

    public void MinigameScreen() //trigger screen for minigame
    {
        Debug.Log("Minigame Screen Opened.");
        fishScreenActive = false;
        minigameScreenActive = true;

        //screens
        ShowUI(fishScreenBG); // background
        HideUI(waitingScreen); // hide waiting screen
        ShowUI(splashScreen); // hide splash screen 
        ShowUI(minigameScreen); // current screen
        HideUI(catchScreen); // hide future screens

        //catch timer
        HideUI(timerGroup);

        //hide buttons
        HideUI(button1);
        HideUI(button2);
        HideUI(fishScreen);

        //hide text
        HideUI(fishHookedText);
        splashScreenActive = false;
        AnimateText(fishHookedText, splashScreenActive); //reset text animator
        HideUI(particlesGroup);

        //Reset Stars
        ResetStars();
        ResetReel();

        //Game Assets
        ShowUI(trackArea);
        ShowUI(successZoneHB);
        ShowUI(bobber);
        ShowUI(bobberString);
        ShowUI(starGroup);
        ShowUI(reelGroup);


    }

    public void ResetStars()
    {
        //Debug.Log("Star Data Reset");

        UpdateSprite(star1, starSprites[0]);
        UpdateSprite(star2, starSprites[0]);
        UpdateSprite(star3, starSprites[0]);

        fishCatch1 = false;
        fishCatch2 = false;
        fishCatch3 = false;
    }

    public void ResetReel()
    {
        //Debug.Log("Reel Data Reset");

        UpdateSprite(reel1, reelSprites[0]);
        UpdateSprite(reel2, reelSprites[0]);
        UpdateSprite(reel3, reelSprites[0]);
        UpdateSprite(reel4, reelSprites[0]);
        UpdateSprite(reel5, reelSprites[0]);
        UpdateSprite(reel6, reelSprites[0]);
        UpdateSprite(reel7, reelSprites[0]);
        UpdateSprite(reel8, reelSprites[0]);

        star1Hit = false;
        star2Hit = false;
        star3Hit = false;
        star4Hit = false;
        star5Hit = false;
        star6Hit = false;
        star7Hit = false;
        star8Hit = false;
    }

    public void Reeling()
    {
        //Debug.Log("Measuring Reeling.");

        if (angle >= 70 && angle <= 105) //range for star 1
        {
            UpdateSprite(reel1, reelSprites[1]);
            star1Hit = true;
            if(star8Hit) //if one full rotation is hit
            {
                rotation++;
                ResetReel();
            }
        }

        if(angle >= 25 && angle <= 55 && star1Hit) //range for star 2, star 1 must be active for it to continue
        {
            UpdateSprite(reel2, reelSprites[1]);
            star2Hit = true;
        }

        if(angle <= 20 && angle >= -15 && star2Hit) //range for star 3, star 2 must be active for it to continue
        {
            UpdateSprite(reel3, reelSprites[1]);
            star3Hit = true;
        }

        if(angle <= -25 && angle >= -55 && star3Hit) //range for star 4, star 3 must be active for it to continue
        {
            UpdateSprite(reel4, reelSprites[1]);
            star4Hit = true;
        }

        if (angle <= -70 && angle >= -105 && star4Hit) //range for star 5, star 4 must be active for it to continue
        {
            UpdateSprite(reel5, reelSprites[1]);
            star5Hit = true;
        }

        if (angle <= -125 && angle >= -150 && star5Hit) //range for star 6, star 5 must be active for it to continue
        {
            UpdateSprite(reel6, reelSprites[1]);
            star6Hit = true;
        }

        if ((angle <= -170 || angle >= 170) && star6Hit) //range for star 7, star 6 must be active for it to continue
        {
            UpdateSprite(reel7, reelSprites[1]);
            star7Hit = true;
        }

        if (angle <= 145 && angle >= 130 && star7Hit) //range for star 8, star 7 must be active for it to continue
        {
            UpdateSprite(reel8, reelSprites[1]);
            star8Hit = true;
        }

        //update tracker vision for reeling
        trackFillBar.fillAmount = Mathf.InverseLerp(0, hitZoneValue, rotation);
    }

    public void CatchScreen() //trigger screen for when fish is caught - win screen
    {
        Debug.Log("Catch Screen Opened.");
        fishScreenActive = false;
        catchScreenActive = true;
        minigameScreenActive = false;

        //screens
        ShowUI(fishScreenBG); // background
        HideUI(waitingScreen); // hide waiting screen
        //ShowUI(splashScreenB); // hide splash screen 
        HideUI(minigameScreen); // hide Minigame screen
        ShowUI(catchScreen); // current screen

        //change dialogue to say fish name
        fishSpecies = fishSpecies.ToUpper();
        displayFishText.text = fishSpecies + "!";
        displayFishTextShadow.text = fishSpecies + "!";
        ShowUI(displayFishTextPanel);
        AnimateText(displayFishTextPanel, catchScreenActive);

        //change fish sprite to the fish species
        if (catchScreenActive) { ShowUI(displayFish); }

        //onWinLose reuse so when you click it closes and asks if you want to play again
        onWinLose = true;
    }

    public void ResetGame() //reset all game data
    {
        Debug.Log("Data Reset");

        ResetReel();
        ResetStars();
        fishCatch3 = false;

        gameActive = false;
        //playAgain = false;
        //isTalking = false;
        canMove = true;
        onWinLose = false;

        catchScreenActive = false;
        minigameScreenActive = false;
        splashScreenActive = false;
        fishScreenActive = false;

        fishHasSpawned = false;
        fishSpawned = false;
        hooked = false;
    }

    public void RestartGame()
    {
        ResetGame();
        FishScreen();
    }


    public void CloseGame() //hide all screens related to fishing and save data
    {
        Debug.Log("Game Closed.");

        HideUI(fishScreenBG); //hide UI related

        ResetGame(); //reset game data

        canOpenAgainTime = Time.time + reopenDelay; // new stop player from opening for a short while cause of controller.

        gameActive = false;
        playAgain = false;
        isTalking = false;
        canMove = true;
    }
}
