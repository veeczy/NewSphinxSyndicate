using UnityEngine;
using System.Collections;

public class RaccoonAI : EnemyAI
{
    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    private Animator anim;

    [Header("Facing")]
    public bool flipFacing = false;

    [Header("Dodge")]
    public float dodgeDistance = 1.5f;
    public float dodgeTime = 0.2f;
    public float bulletDetectRange = 2f;
    public LayerMask wallLayer;
    public string playerBulletTag = "PlayerBullet";

    private bool isDodging = false;
    private bool hasDodgedBullet = false;
    public bool isImmune = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (!CheckAggro()) return;
        if (player == null) return;

        if (isDodging)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isAttacking", false);
            CheckHealth();
            return;
        }

        TryDodgeBullet();

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        if (sr != null)
        {
            if (direction.x > 0)
                sr.flipX = flipFacing;
            else if (direction.x < 0)
                sr.flipX = !flipFacing;
        }

        if (distance > attackRange && !isAttacking)
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

            anim.SetBool("isWalking", true);
            anim.SetBool("isAttacking", false);
        }
        else if (distance <= attackRange)
        {
            anim.SetBool("isWalking", false);

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                StartCoroutine(Attack());
            }
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        CheckHealth();
    }

    private IEnumerator Attack()
    {
        isAttacking = true;

        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", true);

        yield return new WaitForSeconds(0.3f);

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(damage);
            }
        }

        nextAttackTime = Time.time + attackCooldown;

        isAttacking = false;
        anim.SetBool("isAttacking", false);
        anim.SetBool("isWalking", false);
    }

    private void TryDodgeBullet()
    {
        if (hasDodgedBullet || isDodging) return;

        GameObject[] bullets = GameObject.FindGameObjectsWithTag(playerBulletTag);

        foreach (GameObject bullet in bullets)
        {
            if (bullet == null) continue;

            float dist = Vector2.Distance(transform.position, bullet.transform.position);
            if (dist <= bulletDetectRange)
            {
                StartCoroutine(Dodge());
                return;
            }
        }
    }

    private IEnumerator Dodge()
    {
        isDodging = true;
        hasDodgedBullet = true;
        isImmune = true;
        isAttacking = false;

        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", false);

        Vector2 startPos = transform.position;

        float upSpace = CheckFreeSpace(Vector2.up);
        float downSpace = CheckFreeSpace(Vector2.down);

        Vector2 dodgeDir = upSpace >= downSpace ? Vector2.up : Vector2.down;
        Vector2 targetPos = startPos + dodgeDir * dodgeDistance;

        float elapsed = 0f;

        while (elapsed < dodgeTime)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed / dodgeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        isImmune = false;
        isDodging = false;
    }

    private float CheckFreeSpace(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, dodgeDistance, wallLayer);

        if (hit.collider != null)
            return hit.distance;

        return dodgeDistance;
    }
}