using UnityEngine;
using TMPro;

public class EnemyCounterUI : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text enemyText;
    public TMP_Text enemyShadow;

    private void Start()
    {
        //retrieve ui if not linked in inspector
        if (enemyText == null) // enemy counter text
        {
            GameObject placeholder = GameObject.Find("EnemyCounterTxt");
            enemyText = placeholder.GetComponent<TMP_Text>();
        }
        if (enemyShadow == null) // enemy counter text shadow
        {
            GameObject placeholder = GameObject.Find("EnemyCounterTxtShadow");
            enemyShadow = placeholder.GetComponent<TMP_Text>();
        }
    }
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
            if(aliveCount > 0) //if enemies in area, display number
            {
                enemyText.text = aliveCount.ToString();
                enemyShadow.text = enemyText.text;
            }
            if(aliveCount <= 0) //hide ui if no enemies in area
            {
                enemyText.text = "";
                enemyShadow.text = enemyText.text;
            }
            
        }
    }
}