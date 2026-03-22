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


    [Header("Fish UI - Base")]
    public GameObject fishScreen; //background screen
    //dialogue
    public GameObject dialogueUI; //background panel for dialogue
    public TMP_Text dialogueText; //plays jackpot text and any other flavor text

    [Header("Fish UI - Game")]
    public GameObject reel;
    public GameObject spool;
    public GameObject hitZone;
    public GameObject indicator;
    public GameObject fishingRod;
    public GameObject fishShadow;

    [Header("Fish UI - Buttons")]
    public GameObject button1;

    [Header("Fishing Minigame Data")]
    public string[] dialogueLines = new string[] { "", "Line 2" }; //dialogue the minigame can say
    public int dialogueIndex = 0; //number to call what dialogue is said

    [Header("Fishing Minigame Data - Reel")]
    public float angle;
    public Vector2 aimDir;
    public bool controller;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeGameObjects();  
    }

    // Update is called once per frame
    void Update()
    {
        
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
            ShowUI(fishScreen); //shows all ui related to slots
            dialogueIndex = 0;

            if (!playAgain) { CloseGame(); } //hides all ui related to slots
        }
        if (!isTalking) { canMove = true; } // return movement if not talking to minigame npc

        if (gameActive)
        {
            //this is where stuff that happens inside the game goes

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

    public void CloseGame() //hide all screens related to fishing and save data
    {
        HideUI(fishScreen); //hide UI related
        gameActive = false;
        playAgain = false;
        isTalking = false;
        canMove = true;
    }
}
