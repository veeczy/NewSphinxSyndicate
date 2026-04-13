using UnityEngine;
using TMPro;

public class EnemyCounterUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text enemyText;

    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        int aliveCount = 0;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].activeInHierarchy)
            {
                aliveCount++;
            }
        }

        if (enemyText != null)
        {
            enemyText.text = "Enemies Left: " + aliveCount;
        }
    }
}