using UnityEngine;

public class PigProjectile : MonoBehaviour
{
    public float lifeTime = 5f;
    public int damage = 1;

    public GameObject grabPrefab;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Instantiate(grabPrefab, col.transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
        else if (!col.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}