using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuCursor : MonoBehaviour
{
    // cursor setup
    public RectTransform cursorRect;
    public Canvas canvas;
    public float cursorSpeed = 1000f;
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string submitButton = "Submit";

    // slider control speed
    public float sliderSpeed = 0.5f;

    // tracks if controller is being used
    public bool usingController = false;

    // ui screens for minigames
    public GameObject blackjackScreen;
    public GameObject fishingScreen;
    public GameObject slotScreen;

    // ui systems
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Vector3 lastMousePosition;

    // active slider reference
    private Slider activeSlider;

    void Start()
    {
        // get canvas if not set
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        // get cursor rect if not set
        if (cursorRect == null)
            cursorRect = GetComponent<RectTransform>();

        
        raycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        
        cursorRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        SetCursorVisible(false);
    }

    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // check if current scene is a menu
        bool isMenuScene =
            sceneName == "MainMenu" ||
            sceneName == "Settings" ||
            sceneName == "HowToPlay" ||
            sceneName == "Wanted Board" ||
            sceneName == "LoseScene" ||
            sceneName == "WinScene" ||
            sceneName == "VictoryScene";

        // check pause menu
        bool isPauseMenuOpen =
            PauseManager.Instance != null &&
            PauseManager.Instance.isPaused;

        // check minigame screens
        bool isBlackjackOpen =
            blackjackScreen != null &&
            blackjackScreen.activeInHierarchy;

        bool isFishingOpen =
            fishingScreen != null &&
            fishingScreen.activeInHierarchy;

        bool isSlotOpen =
            slotScreen != null &&
            slotScreen.activeInHierarchy;

        // hide cursor if not in any ui state
        if (!isMenuScene && !isPauseMenuOpen && !isBlackjackOpen && !isFishingOpen && !isSlotOpen)
        {
            SetCursorVisible(false);
            return;
        }

        // get controller input
        float moveX = Input.GetAxisRaw(horizontalAxis);
        float moveY = Input.GetAxisRaw(verticalAxis);

        bool controllerInput =
            Mathf.Abs(moveX) > 0.4f ||
            Mathf.Abs(moveY) > 0.4f ||
            Input.GetButtonDown(submitButton);

        // detect mouse input
        bool mouseInput =
            Input.mousePosition != lastMousePosition ||
            Input.GetMouseButtonDown(0);

        // mouse turns off controller cursor
        if (mouseInput)
            usingController = false;

        // controller turns it on
        if (controllerInput)
        {
            usingController = true;

            
            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(null);
        }

        lastMousePosition = Input.mousePosition;

        
        SetCursorVisible(usingController);

        // stop if not using controller
        if (!usingController)
            return;

        
        MoveCursor();
        HandleSlider();

        if (Input.GetButtonDown(submitButton))
            ClickUI();
    }

    void MoveCursor()
    {
        // move cursor based on input
        float moveX = Input.GetAxisRaw(horizontalAxis);
        float moveY = Input.GetAxisRaw(verticalAxis);

        Vector3 move = new Vector3(moveX, moveY, 0f) * cursorSpeed * Time.unscaledDeltaTime;
        cursorRect.position += move;

        // clamp cursor to screen
        Vector3 pos = cursorRect.position;
        pos.x = Mathf.Clamp(pos.x, 0f, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0f, Screen.height);
        cursorRect.position = pos;
    }

    void HandleSlider()
    {
        // allow controller to adjust sliders
        if (Input.GetButton(submitButton))
        {
            if (activeSlider == null)
            {
                PointerEventData pointerData = new PointerEventData(eventSystem);
                pointerData.position = cursorRect.position;

                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(pointerData, results);

                foreach (RaycastResult result in results)
                {
                    Slider slider = result.gameObject.GetComponent<Slider>();

                    if (slider == null)
                        slider = result.gameObject.GetComponentInParent<Slider>();

                    if (slider != null)
                    {
                        activeSlider = slider;
                        break;
                    }
                }
            }

            if (activeSlider != null)
            {
                float moveX = Input.GetAxisRaw(horizontalAxis);
                activeSlider.value += moveX * sliderSpeed * Time.unscaledDeltaTime;
            }
        }
        else
        {
            activeSlider = null;
        }
    }

    void ClickUI()
    {
        // simulate button click with cursor
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = cursorRect.position;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Button btn = result.gameObject.GetComponent<Button>();

            if (btn == null)
                btn = result.gameObject.GetComponentInParent<Button>();

            if (btn != null)
            {
                Debug.Log("menu cursor clicked: " + btn.name);
                btn.onClick.Invoke();
                return;
            }
        }
    }

    void SetCursorVisible(bool visible)
    {
        
        Graphic[] graphics = cursorRect.GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            graphic.enabled = visible;
        }
    }
}