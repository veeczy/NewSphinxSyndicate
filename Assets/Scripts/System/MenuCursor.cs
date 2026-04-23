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

    public bool usingController = true;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    void Start()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (cursorRect == null)
            cursorRect = GetComponent<RectTransform>();

        raycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        Vector2 startPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        cursorRect.position = startPos;
    }

    void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        bool isMenuScene =
            sceneName == "MainMenu" ||
            sceneName == "Settings" ||
            sceneName == "HowToPlay";

        bool isPauseMenuOpen =
            PauseManager.Instance != null &&
            PauseManager.Instance.isPaused;

        bool canUseMenuCursor = isMenuScene || isPauseMenuOpen;

        if (!canUseMenuCursor)
        {
            cursorRect.gameObject.SetActive(false);
            return;
        }

        float moveX = Input.GetAxisRaw(horizontalAxis);
        float moveY = Input.GetAxisRaw(verticalAxis);

        if (isPauseMenuOpen)
        {
            usingController = true;
            cursorRect.gameObject.SetActive(true);
            MoveCursor();

            if (Input.GetButtonDown(submitButton))
                ClickUI();

            return;
        }

        if (Mathf.Abs(moveX) > 0.2f || Mathf.Abs(moveY) > 0.2f)
            usingController = true;

        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
            Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f ||
            Input.GetMouseButtonDown(0))
            usingController = false;

        cursorRect.gameObject.SetActive(usingController);

        if (!usingController)
            return;

        MoveCursor();

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

    void ClickUI()
    {
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = cursorRect.position;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                Button btn = results[i].gameObject.GetComponent<Button>();

                if (btn == null)
                    btn = results[i].gameObject.GetComponentInParent<Button>();

                if (btn != null)
                {
                    btn.onClick.Invoke();
                    return;
                }
            }
        }
    }
}