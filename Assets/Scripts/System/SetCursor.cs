using UnityEngine;

public class SetCursor : MonoBehaviour
{
    public static SetCursor Instance;
    public Transform cursorPrefab;
    public Canvas cursorCanvasPrefab;
    private Canvas cursorCanvas;
    private Transform cursorObj;

    void Start()
    {
        // changed
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // destroy ONLY duplicate
            return; // changed
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); //bring setcursor to next room

        // changed
        if (cursorCanvas == null)
        {
            cursorCanvas = Instantiate(cursorCanvasPrefab, transform.position, transform.rotation);
            DontDestroyOnLoad(cursorCanvas.gameObject); // changed
        }

        // changed
        if (cursorObj == null && cursorCanvas != null)
        {
            cursorObj = Instantiate(cursorPrefab, cursorCanvas.transform);
        }
    }

    public void SetCrosshair(Vector2 pos)
    {
        if (cursorObj != null)
        {
            RectTransform rt = cursorObj.GetComponent<RectTransform>();
            rt.position = Camera.main.WorldToScreenPoint(pos);
        }
    }
}