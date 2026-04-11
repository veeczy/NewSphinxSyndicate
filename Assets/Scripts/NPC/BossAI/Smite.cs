using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class Smite : MonoBehaviour
{
    public Sprite indicatorSprite;
    public Sprite smiteAttackSprite;
    public SpriteRenderer smiteSprite;
    public float countdownTimer;
    public float dmgTimer;
    public int smiteDamage;
    public bool doDamage;
    public AudioClip smiteSound;
    private bool isRunning;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        StartCoroutine("dmgZone");
    }
    void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player") && doDamage)
        {
            Debug.Log("TRIGGERED");
            other.GetComponent<PlayerHealth>().TakeDamage(smiteDamage);
            doDamage = false;
        }
    }
    IEnumerator dmgZone()
    {
        isRunning = true;
        smiteSprite.sprite = indicatorSprite;
        yield return new WaitForSeconds(countdownTimer);
        smiteSprite.sprite = smiteAttackSprite;
        doDamage = true;
        if(gameObject.GetComponent<AudioSource>())
        gameObject.GetComponent<AudioSource>().PlayOneShot(smiteSound);
        yield return new WaitForSeconds(dmgTimer);
        doDamage = false;
        isRunning = false;
    }
}
