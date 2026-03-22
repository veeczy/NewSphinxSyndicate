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
        if (Instance == null) //if there is not an instance of setcursor
        {
            Instance = this;             
            DontDestroyOnLoad(gameObject); //bring setcursor to next room
                                           
            cursorCanvas = Instantiate(cursorCanvasPrefab, transform.position, transform.rotation);
            cursorObj = Instantiate(cursorPrefab, cursorCanvas.transform);

        }
        else //if there is an instance of set cursor already
        {
            Destroy(gameObject); //destroy duplicate
        }
    }
    public void SetCrosshair(Vector2 pos)
    {
        if(cursorObj != null)
        {
            RectTransform rt = cursorObj.GetComponent<RectTransform>();
            rt.position = Camera.main.WorldToScreenPoint(pos);
        }
    }

}
