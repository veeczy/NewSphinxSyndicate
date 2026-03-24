using UnityEngine;
using System.Collections;

public class CarAI : EnemyAI
{
    [Header("Car Settings")]
    public float attackRange = 4f;
    public float chargeTime = 1f;
    public float dashSpeed = 12f;
    public float stunTime = 5f;

    [Header("Hit Settings")]
    public float dashHitRange = 1f;
    private bool hasHitPlayer = false;

    [Header("Wall Layers")]
    public LayerMask wallLayer;

    private bool isCharging = false;
    private bool isDashing = false;
    private bool isStunned = false;

    private Vector2 dashDirection;

    protected override void Update()
    {
        if (!CheckAggro()) return;
        if (player == null) return;

        // face player
        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x > 0) sr.flipX = true;
        else if (direction.x < 0) sr.flipX = false;

        if (isStunned)
        {
            CheckHealth();
            return;
        }

        if (isCharging)
        {
            CheckHealth();
            return;
        }

        if (isDashing)
        {
            transform.position += (Vector3)(dashDirection * dashSpeed * Time.deltaTime);

            if (dashDirection.x > 0) sr.flipX = false;
            else if (dashDirection.x < 0) sr.flipX = true;

            CheckDashHitPlayer();
            CheckHealth();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            StartCoroutine(ChargeAndDash());
        }
        else
        {
            HandleMovement();
        }

        CheckHealth();
    }

    IEnumerator ChargeAndDash()
    {
        if (isCharging || isDashing || isStunned) yield break;

        isCharging = true;

        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;

        if (directionToPlayer.x > 0) sr.flipX = false;
        else if (directionToPlayer.x < 0) sr.flipX = true;

        yield return new WaitForSeconds(chargeTime);

        if (player == null)
        {
            isCharging = false;
            yield break;
        }

        dashDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        hasHitPlayer = false;

        isCharging = false;
        isDashing = true;
    }

    void CheckDashHitPlayer()
    {
        if (hasHitPlayer || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= dashHitRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hasHitPlayer = true;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!isDashing) return;

        if (((1 << col.gameObject.layer) & wallLayer) != 0)
        {
            StartCoroutine(StunAfterCrash());
        }
    }

    IEnumerator StunAfterCrash()
    {
        isDashing = false;
        isStunned = true;

        yield return new WaitForSeconds(stunTime);

        isStunned = false;
    }
}