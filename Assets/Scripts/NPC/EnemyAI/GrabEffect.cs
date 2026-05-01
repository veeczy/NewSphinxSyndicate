using UnityEngine;
using System.Collections;

public class GrabEffect : MonoBehaviour
{
    public int damage = 1;
    public float damageInterval = 3f;

    private Transform player;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private static int activeGrabCount = 0;
    private static float savedPlayerSpeed;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        playerHealth = player.GetComponent<PlayerHealth>();
        playerMovement = player.GetComponent<PlayerMovement>();

        // APPLY GRAB (same as snake)
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

    void Update()
    {
        if (player != null)
        {
            transform.position = player.position; // follow player
        }
    }

    IEnumerator DamageLoop()
    {
        while (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    public void TakeDamage(int amount)
    {
        Destroy(gameObject); // player breaks free by shooting it
    }

    void ReleasePlayer()
    {
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