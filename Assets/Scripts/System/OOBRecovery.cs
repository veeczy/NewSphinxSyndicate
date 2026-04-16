using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OOBRecovery : MonoBehaviour
{
    [Header("Wall Check")]
    public LayerMask wallLayer;
    public Transform centerCheck;   // empty object in middle of player
    public float checkRadius = 0.05f;
    public float stuckTime = 0.2f;

    [Header("Saved Spawn")]
    public Vector3 savedSpawnPosition; // stores spawn position per scene

    private float stuckTimer = 0f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(SaveSpawnNextFrame()); // wait one frame before saving spawn
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // listen for scene changes
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // clean up event
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SaveSpawnNextFrame()); // update spawn after scene load
    }

    void Update()
    {
        if (centerCheck == null)
            return;

        bool insideWall = Physics2D.OverlapCircle(centerCheck.position, checkRadius, wallLayer); // detect if inside wall

        if (insideWall)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckTime) // delay so normal wall touches don’t trigger
            {
                TeleportToSavedSpawn(); // fix OOB
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f; // reset timer if not stuck
        }
    }

    IEnumerator SaveSpawnNextFrame()
    {
        yield return null; // wait 1 frame so player is fully placed in new scene
        savedSpawnPosition = transform.position; // store spawn
    }

    void TeleportToSavedSpawn()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // stop movement
            rb.angularVelocity = 0f;          // stop rotation
        }

        transform.position = savedSpawnPosition; // teleport back to spawn
    }

    void OnDrawGizmosSelected()
    {
        if (centerCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerCheck.position, checkRadius);
        }
    }
}