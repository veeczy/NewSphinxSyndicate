using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuCursor : MonoBehaviour
{
    public RectTransform cursorRect;
    public Canvas canvas;
    public float cursorSpeed = 1000f;
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string submitButton = "Submit";

    public float sliderSpeed = 0.5f;

    public bool usingController = false;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Vector3 lastMousePosition;

    private Slider activeSlider;

    void Start()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (cursorRect == null)
            cursorRect = GetComponent<RectTransform>();

        raycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        cursorRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        lastMousePosition = Input.mousePosition;

        SetCursorVisible(false);
    }

    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        bool isMenuScene =
            sceneName == "MainMenu" ||
            sceneName == "Settings" ||
            sceneName == "HowToPlay" ||
            sceneName == "Wanted Board" ||
            sceneName == "LoseScene" ||
            sceneName == "WinScene" ||
            sceneName == "VictoryScene";

        bool isPauseMenuOpen =
            PauseManager.Instance != null &&
            PauseManager.Instance.isPaused;

        if (!isMenuScene && !isPauseMenuOpen)
        {
            SetCursorVisible(false);
            return;
        }

        float moveX = Input.GetAxisRaw(horizontalAxis);
        float moveY = Input.GetAxisRaw(verticalAxis);

        bool controllerInput =
            Mathf.Abs(moveX) > 0.4f ||
            Mathf.Abs(moveY) > 0.4f ||
            Input.GetButtonDown(submitButton);

        bool mouseInput =
            Input.mousePosition != lastMousePosition ||
            Input.GetMouseButtonDown(0);

        if (mouseInput)
            usingController = false;

        if (controllerInput)
        {
            usingController = true;

            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(null);
        }

        lastMousePosition = Input.mousePosition;

        SetCursorVisible(usingController);

        if (!usingController)
            return;

        MoveCursor();
        HandleSlider();

        if (Input.GetButtonDown(submitButton))
            ClickUI();
    }

    void MoveCursor()
    {
        float moveX = Input.GetAxisRaw(horizontalAxis);
        float moveY = Input.GetAxisRaw(verticalAxis);

        Vector3 move = new Vector3(moveX, moveY, 0f) * cursorSpeed * Time.unscaledDeltaTime;
        cursorRect.position += move;

        Vector3 pos = cursorRect.position;
        pos.x = Mathf.Clamp(pos.x, 0f, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0f, Screen.height);
        cursorRect.position = pos;
    }

    void HandleSlider()
    {
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
                Debug.Log("MenuCursor clicked: " + btn.name);
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