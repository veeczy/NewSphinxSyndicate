using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CityBossAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public SpriteRenderer bossSprite;
    public Rigidbody2D rb;
    public Animator bossAnimator;
    protected Transform player;

    [Header("Health")]
    public Slider healthUI;
    public int health;
    public int maxHealth = 150;

    [Header("Movement / Combat")]
    public float minDistance = 0.0f;
    public float attackRange;
    public Vector2 smiteTarget;
    public bool meleeCooldown;
    public int contactDamage = 1;
    public bool allowSmite = true;
    public bool spawnDog = true;
    public bool dogReleased = false;
    public bool meleeMode = true;

    [Header("Phases")]
    public int phase = 1;
    public int phase2Health = 100;
    public int phase3Health = 50;

    [Header("Melee Attack")]
    public float meleeRange = 3f;
    public float meleeDelay = 1f;
    public float burstCount = 3f;
    public float burstDelay = 0.5f;
    public float attackTime = 10f;
    public float meleeCooldownTime = 10f;
    public int damage = 3;


    [Header("Smite Attack")]
    public GameObject smitePrefab;
    public int smiteCounter = 0;
    public int smiteCountMax = 5;
    public float smiteDelay = 3;
    public float smiteCooldownTime;
    public bool smiteCooldown;
    public bool smiteAttacking = false;


    [Header("Dog Attack")]
    public GameObject dogPrefab;
    public float dogTimer = 7.5f;
    public float dogCooldownTime = 10f;
    public bool dogAttacking = false;
    public bool dogCooldown = false;

    public Vector2 direction;
    private float distance;

    [Header("Boss Progress Tracking")]
    public int bossLevel; // 0 = desert, 1 = city, 2 = swamp
    private bool hasDied = false;

    [Header("DEBUG")]
    public KeyCode debugDamageKey = KeyCode.Alpha8;
    public int debugDamageAmount = 50;
    public bool isMelee = false;
    public bool isContacting = false;

    public KeyCode heavyDamageKey = KeyCode.J; // NEW
    public int heavyDamageAmount = 15; // NEW

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        health = maxHealth;
        healthUI.maxValue = maxHealth;
        healthUI.value = maxHealth;
    }

    void FixedUpdate()
    {
        // DEBUG DAMAGE
        if (Input.GetKeyDown(debugDamageKey))
        {
            health -= debugDamageAmount;
            healthUI.value = health;
            Debug.Log("DEBUG: Boss took " + debugDamageAmount + " damage. Health = " + health);
        }

        // J KEY DAMAGE
        if (Input.GetKeyDown(heavyDamageKey))
        {
            health -= heavyDamageAmount;
            healthUI.value = health;
            Debug.Log("J HIT: Boss took " + heavyDamageAmount + " damage. Health = " + health);
        }

        // DEATH CHECK (runs once)
        if (health <= 0 && !hasDied)
        {
            hasDied = true;
            HandleBossDefeated();
            return;
        }
        distance = Vector2.Distance(transform.position, player.position);
        direction = (player.position - transform.position);
        bossSprite.flipX = direction.x < 0;
        if(!smiteAttacking)//FREEZE BOSS WHEN SMITING
        {
            if(!meleeMode && !meleeCooldown)
            {
                StartCoroutine("closeAttack");
            }
            else if(meleeMode)
            {
                float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= meleeRange && isContacting && !isMelee)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                    {
                        ph.TakeDamage(damage);
                        StartCoroutine("meleeAttack");//HANDLE MELEE DAMAGE
                    }
        }
                rb.MovePosition(Vector2.MoveTowards(rb.position, player.position, moveSpeed * Time.deltaTime));//MOVE TOWARD PLAYER
            }      
            if(!dogAttacking && !dogReleased && spawnDog && !dogCooldown)
            {
                StartCoroutine("dogAttack");//BEGIN DOG ATTACK
            }
            if(allowSmite && !smiteCooldown)
                {
                    StartCoroutine("smiteAttack");//BEGIN SMITE ATTACK
                }
        }
//START HANDLE BOSS PHASES
        if (health <= phase2Health && phase < 2)
        {
            phase = 2;
            spawnDog = true;
            moveSpeed *= 2f;
        }
        else if(health <= phase3Health && phase < 3)
        {
            phase = 3;
            spawnDog = true;
            allowSmite = true;
        }
//END HANDLE BOSS PHASES
    }

    //updated check boss count and then loads victory scene if it equals 3
    void HandleBossDefeated()
    {

        PlayerPrefs.SetInt("cityBoss", 1);

        int count = PlayerPrefs.GetInt("bossCounter", 0) + 1;
        PlayerPrefs.SetInt("bossCounter", count);

        PlayerPrefs.Save();

        Debug.Log("Boss Count: " + count);

        if (count >= 3)
        {
            SceneManager.LoadScene("VictoryScene");
            return;
        }

        LevelManager.instance.ResetRun();
        Destroy(gameObject);
    }

    IEnumerator smiteAttack()
    {
        smiteAttacking = true;
        bossAnimator.SetBool("isAttacking", true);
        smiteTarget = player.position;
        yield return new WaitForSeconds(smiteDelay);
        GameObject smiteZone = Instantiate(smitePrefab, smiteTarget, transform.rotation);//SPAWNS TRIGGER THAT DETECTS PLAYER AND DOES LARGE DAMAGE IF PLAYER IS WITHIN TRIGGER AFTER TIMER
        smiteCounter++;
        smiteAttacking = false;
        bossAnimator.SetBool("isAttacking", false);
        if(smiteCounter >= smiteCountMax)
        {
            smiteCooldown = true;
            yield return new WaitForSeconds(smiteCooldownTime + Random.Range(-2.5f, 2.5f));
            smiteCooldown = false;
            smiteCounter = 0;
        }
    }

    IEnumerator dogAttack()
    {
        dogReleased = true;
        GameObject dog = Instantiate(dogPrefab, transform.position, transform.rotation);
        DogEnemyAI dogAI = dog.GetComponent<DogEnemyAI>();
        dogAI.owner = this.transform;//Tell dog script what object sent out the dog
        dogCooldown = true;
        yield return new WaitForSeconds(dogCooldownTime);
        dogCooldown = false;
    }
private IEnumerator meleeAttack()
    {
        isMelee = true;
        bossAnimator.SetBool("isWalking", false);
        yield return new WaitForSeconds(meleeDelay);
        isMelee = false;
    }
    IEnumerator closeAttack()
    {
        meleeMode = true;
        bossAnimator.SetBool("isWalking", true);
        yield return new WaitForSeconds(attackTime);
        meleeMode = false;
        bossAnimator.SetBool("isWalking", false);
        meleeCooldown = true;
        yield return new WaitForSeconds(meleeCooldownTime + Random.Range(-2.5f, 2.5f));
        meleeCooldown = false;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Bullet") && col.GetComponent<BulletId>())
        {
            health -= col.GetComponent<BulletId>().dmg;
            healthUI.value = health;
        }
    }
        void OnTriggerStay2D(Collider2D col)
    {
            if (col.CompareTag("Player"))
            {
                isContacting = true;
            }
            else
            {
                isContacting = false;
            }
    }
}
