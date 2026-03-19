using UnityEngine;
using System.Collections;

public class FishAI : EnemyAI
{
    [Header("Fish Settings")]
    public float attackRange = 10f;
    public float waitAfterShot = 5f;
    public GameObject projectilePrefab;

    [Header("Teleport Spots")]
    public Transform[] teleportPoints;

    private SpriteRenderer sr;
    private Animator anim;

    private bool isAttacking = false;
    private bool isHidden = false;
    private int lastTeleportIndex = -1;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (!CheckAggro()) return;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        
        if (sr != null)
        {
            if (direction.x > 0) sr.flipX = false;
            else if (direction.x < 0) sr.flipX = true;
        }

        
        if (anim != null)
            anim.SetBool("isWalking", false);

    
        if (distance <= attackRange && !isAttacking && !isHidden)
        {
            StartCoroutine(ShootTeleportLoop());
        }

        if (!isAttacking && anim != null)
            anim.SetBool("isAttacking", false);

        CheckHealth();
    }

    private IEnumerator ShootTeleportLoop()
    {
        isAttacking = true;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", true);
        }

        yield return new WaitForSeconds(0.2f);

        Shoot();

        yield return new WaitForSeconds(0.3f);

        if (anim != null)
            anim.SetBool("isAttacking", false);

 
        yield return new WaitForSeconds(waitAfterShot);

       
        HideFish();

        
        yield return new WaitForSeconds(0.2f);

        TeleportToRandomPoint();

        ShowFish();

        isAttacking = false;
    }

    void Shoot()
    {
        if (projectilePrefab != null && player != null)
        {
            GameObject bullet = Instantiate(projectilePrefab, transform.position, transform.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 shootDir = (player.position - transform.position).normalized;
                rb.AddForce(shootDir * 8f, ForceMode2D.Impulse);
            }
        }
    }

    void HideFish()
    {
        isHidden = true;

        if (sr != null)
            sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    void ShowFish()
    {
        isHidden = false;

        if (sr != null)
            sr.enabled = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;
    }

    void TeleportToRandomPoint()
    {
        if (teleportPoints == null || teleportPoints.Length == 0) return;

        int randomIndex = Random.Range(0, teleportPoints.Length);

        if (teleportPoints.Length > 1)
        {
            while (randomIndex == lastTeleportIndex)
            {
                randomIndex = Random.Range(0, teleportPoints.Length);
            }
        }

        lastTeleportIndex = randomIndex;
        transform.position = teleportPoints[randomIndex].position;
    }
}