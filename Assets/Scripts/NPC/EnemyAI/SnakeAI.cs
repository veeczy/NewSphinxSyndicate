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

    private Animator anim;

    protected override void Update()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (!CheckAggro())
        {
            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isBinding", false);
            }
            return;
        }

        if (player == null) return;

        if (isGrabbing)
        {
            transform.position = player.position;

            if (anim != null)
            {
                anim.SetBool("isWalking", false);
                anim.SetBool("isBinding", true);
            }

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

            if (anim != null)
            {
                anim.SetBool("isWalking", true);
                anim.SetBool("isBinding", false);
            }
        }

        CheckHealth();
    }

    void StartGrab()
    {
        if (isGrabbing) return;

        isGrabbing = true;

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isBinding", true);
        }

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

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isBinding", false);
        }

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