using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinSceneButtons : MonoBehaviour
{
    public bool toggle = false; //false is keyboard, true is controller
    //private GameObject buttonUI; //button 
    public TMP_Text buttonText; //text on button that says keyboard/controller
    public TMP_Text buttonTextShadow;

    public GameObject HTPcontroller;
    public GameObject HTPkeyboard;

    private void Start()
    {
        //if(buttonUI != null) { buttonUI = FindInactiveObjectByName("Toggle"); }
        if (buttonText == null)
        {
            GameObject placeholder = FindInactiveObjectByName("ToggleText");
            buttonText = placeholder.GetComponent<TextMeshProUGUI>();
        }
        if (buttonTextShadow == null)
        {
            GameObject placeholder = FindInactiveObjectByName("ToggleTextShadow");
            buttonTextShadow = placeholder.GetComponent<TextMeshProUGUI>();
        }
        if (HTPcontroller == null) { HTPcontroller = FindInactiveObjectByName("Controller"); }
        if (HTPkeyboard == null) { HTPkeyboard = FindInactiveObjectByName("Keyboard"); }
    }
    // Call this on Main Menu button
    public void GoToMainMenu()
    {
        Debug.Log("[WinScene] Main Menu button pressed.");
        SceneManager.LoadScene("MainMenu");
    }

    // Call this on Return to Office button
    public void ReturnOffice()
    {
        Debug.Log("[WinScene] Return button pressed.");
        SceneManager.LoadScene("Office");
    }

    // Call this on Quit Game button
    public void QuitGame()
    {
        Debug.Log("[WinScene] Quit Game button pressed.");

#if UNITY_EDITOR
        Debug.Log("[WinScene] Application.Quit() would run in build.");
#else
        Application.Quit();
#endif
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

    public void ToggleControls()
    {
        toggle = !toggle;

        if(toggle) //if controller
        {
            HTPcontroller.SetActive(true);
            HTPkeyboard.SetActive(false);
            buttonText.text = "Controller";
            buttonTextShadow.text = "Controller";
        }
        if(!toggle)
        {
            HTPcontroller.SetActive(false);
            HTPkeyboard.SetActive(true);
            buttonText.text = "Keyboard";
            buttonTextShadow.text = "Keyboard";
        }
    }
}