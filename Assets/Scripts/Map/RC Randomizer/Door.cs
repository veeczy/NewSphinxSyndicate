using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Trigger Settings")]
    public bool onTrigger = true;

    [Header("Is this a secret door?")]
    public bool isSecretDoor = false;

    [Header("Direction")] //which direction is this door facing
    public bool up = false;
    public bool down = false;
    public bool left = false;
    public bool right = false; 

    [Header("Force Scene Name (filling this overrides randomizer)")]
    public string sceneName;  // NEW

    [Header("Location")]
    public int x;
    public int y;
    public Vector2 playerCoords;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!onTrigger) return;
        if (!other.CompareTag("Player")) return;

        if (sceneName != "") // if overiding randomizer to specific scene
        {
            LevelManager.instance.LoadSceneByTrigger(sceneName);
            return;
        }
        else if (isSecretDoor) // if secret door
        {
            //LevelManager.instance.LoadSecretRoom();
        }
        else
        {
            MoveRooms(); // update coordinates

            // use coordinates to move in map
            //LevelManager.instance.LoadRoom();
        }
    }

    public void MoveRooms()
    {
        //get old coordinates
        x = PlayerPrefs.GetInt("X");
        y = PlayerPrefs.GetInt("Y");
        playerCoords = new Vector2(x, y);
        Vector2 newCoords = Vector2.zero;

        //move coordinates in direction door is facing
        if (up) { newCoords = playerCoords + Vector2.up; }
        if (down) {  newCoords = playerCoords + Vector2.down; }
        if (left) { newCoords = playerCoords + Vector2.left; }
        if (right) { newCoords = playerCoords + Vector2.right; }

        //save new coordinates
        PlayerPrefs.SetInt("X", (int)newCoords.x);
        PlayerPrefs.SetInt("Y", (int)newCoords.y);
    }
}





