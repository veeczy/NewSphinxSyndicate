using UnityEngine;
using System.Collections;

public class AlligatorAI : EnemyAI
{
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (!CheckAggro()) return;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 direction = (player.position - transform.position).normalized;

        // flipped facing
        if (sr != null)
        {
            if (direction.x > 0) sr.flipX = true;
            else if (direction.x < 0) sr.flipX = false;
        }

        // --- MOVEMENT ---
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

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);
        }

        nextAttackTime = Time.time + attackCooldown;

        isAttacking = false;
        anim.SetBool("isAttacking", false);
        anim.SetBool("isWalking", false);
    }
}