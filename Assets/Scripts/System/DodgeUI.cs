using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DodgeUIText : MonoBehaviour
{
    public TMP_Text dodgeText;
    public TMP_Text dodgeTextShadow;
    public float dodgeCooldown = 0.6f;
    private float cooldownTimer = 0f;

    public bool buttonHeld = false;
    private GameObject buttonUI;
    private Animator buttonAnimator;

    public GameObject keyboardUI;
    public Animator keyboardAnimator;
    public GameObject controllerUI;
    public Animator controllerAnimator;

    public GameObject player;
    public bool controller;

    private void Start()
    {
        InitializeGameObjects();
    }

    void Update()
    {
        if(Input.GetButtonDown("Dodge")) { buttonHeld = true; }
        if(Input.GetButtonUp("Dodge")) { buttonHeld = false; }
        if(player != null) { controller = player.GetComponent<PlayerMovement>().controller; }
        if(controller) { buttonUI = controllerUI; buttonAnimator = controllerAnimator; }
        if(!controller) { buttonUI = keyboardUI; buttonAnimator = keyboardAnimator; }

        if(buttonHeld)
        {
            buttonUI.SetActive(true); // show button
        }
        if(!buttonHeld) { buttonAnimator.SetTrigger("Press"); buttonUI.SetActive(false); buttonAnimator.ResetTrigger("Press"); }


        //nichs stuff
        if (Input.GetButtonUp("Dodge") && cooldownTimer <= 0f)
        {
            cooldownTimer = dodgeCooldown;
        }

        // Countdown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            dodgeText.text = "COOLDOWN";
            dodgeTextShadow.text = dodgeText.text;
        }
        else
        {
            dodgeText.text = "READY";
            dodgeTextShadow.text = dodgeText.text;
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
        //nichs original dodge cooldown ui retrieve
        if (dodgeText == null) // enemy counter text
        {
            GameObject placeholder = GameObject.Find("DodgeUI");
            dodgeText = placeholder.GetComponent<TMP_Text>();
        }
        if (dodgeTextShadow == null) // enemy counter text
        {
            GameObject placeholder = GameObject.Find("DodgeTimer");
            dodgeTextShadow = placeholder.GetComponent<TMP_Text>();
        }

        //the button pressing indicator ui
        if (keyboardUI == null) { keyboardUI = GameObject.Find("KeyboardButtonUI"); keyboardUI = FindInactiveObjectByName("KeyboardButtonUI"); keyboardAnimator = keyboardUI.GetComponent<Animator>(); }
        if (controllerUI == null) { controllerUI = GameObject.Find("ControllerButtonUI"); controllerUI = FindInactiveObjectByName("ControllerButtonUI"); controllerAnimator = controllerUI.GetComponent<Animator>(); }
        if (player == null) { GameObject.Find("Player"); }
        controllerUI.SetActive(false);
        keyboardUI.SetActive(false);
    }
}