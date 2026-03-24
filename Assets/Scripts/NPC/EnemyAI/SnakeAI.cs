using UnityEngine;
using System.Collections;

public class SnakeAI : EnemyAI
{
    [Header("Snake Settings")]
    public float grabRange = 1f;
    public float damageInterval = 3f;

    private bool isGrabbing = false;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    protected override void Update()
    {
        if (!CheckAggro()) return;
        if (player == null) return;

        // NEW: face player
        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x > 0) sr.flipX = false;
        else if (direction.x < 0) sr.flipX = true;

        if (isGrabbing)
        {
            transform.position = player.position;
            CheckHealth();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= grabRange)
        {
            StartGrab();
        }
        else
        {
            HandleMovement();
        }

        CheckHealth();
    }

    void StartGrab()
    {
        if (isGrabbing) return;

        isGrabbing = true;

        playerHealth = player.GetComponent<PlayerHealth>();
        playerMovement = player.GetComponent<PlayerMovement>();

        if (playerMovement != null)
            playerMovement.canMove = false;

        StartCoroutine(DamageLoop());
    }

    IEnumerator DamageLoop()
    {
        while (isGrabbing && playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    protected override void CheckHealth()
    {
        if (health <= 0)
        {
            ReleasePlayer();
            Destroy(gameObject);
        }
    }

    void ReleasePlayer()
    {
        isGrabbing = false;

        if (playerMovement != null)
            playerMovement.canMove = true;
    }

    void OnDestroy()
    {
        ReleasePlayer();
    }
}