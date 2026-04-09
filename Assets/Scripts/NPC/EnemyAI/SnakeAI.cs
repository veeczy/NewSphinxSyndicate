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

    private static int activeGrabCount = 0;
    private static float savedPlayerSpeed;

    protected override void Update()
    {
        if (!CheckAggro()) return;
        if (player == null) return;

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
        {
            if (activeGrabCount == 0)
            {
                savedPlayerSpeed = playerMovement.speed;
            }

            activeGrabCount++;
            playerMovement.speed = 0f;

            if (playerMovement.myPlayer != null)
                playerMovement.myPlayer.linearVelocity = Vector2.zero;
        }

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
        if (!isGrabbing) return;

        isGrabbing = false;

        if (playerMovement != null)
        {
            activeGrabCount--;

            if (activeGrabCount <= 0)
            {
                activeGrabCount = 0;
                playerMovement.speed = savedPlayerSpeed;
            }
        }
    }

    void OnDestroy()
    {
        ReleasePlayer();
    }
}