using UnityEngine;
using TMPro; 

public class EnemyCounterUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text enemyText; 

    [Header("Optional: Trap reference")]
    public Trap trap; //Trap assign

    void Update()
    {
        if (trap != null)
        {
            // Enemy Count
            int enemiesLeft = trap.enemies.Length;

            // UI Txt
            if (enemyText != null)
                enemyText.text = $"Enemies Left: {enemiesLeft}";
        }
        else
        {
            // Enemy Tag
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            int enemiesLeft = enemies.Length;

            if (enemyText != null)
                enemyText.text = $"Enemies Left: {enemiesLeft}";
        }
    }
}