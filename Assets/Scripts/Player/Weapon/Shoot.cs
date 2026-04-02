using UnityEngine;
using System.Collections;

public class Shoot : MonoBehaviour
{
    public Transform bulletSpawn;
    public GameObject bulletPrefab;
    public GunHolderRotate gun;
    public AudioSource gunSounds;
    public AudioClip shootSound;
    public float shootDelay = 0.3f;
    public float velocityMultiplier = 10f;
    public bool isShooting = false;

    void Update()
    {
        if (Input.GetButton("Shoot") && !isShooting)
            StartCoroutine(ShootCoroutine());
    }

    IEnumerator ShootCoroutine()
    {
        isShooting = true;

        if (bulletPrefab != null && bulletSpawn != null)
        {
            Rigidbody2D bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity).GetComponent<Rigidbody2D>();
            Vector2 shootDir = gun != null ? gun.AimDirection.normalized : Vector2.right;
            bullet.AddForce(shootDir * velocityMultiplier, ForceMode2D.Impulse);

            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (gunSounds != null && shootSound != null)
            gunSounds.PlayOneShot(shootSound);

        yield return new WaitForSeconds(shootDelay);
        isShooting = false;
    }
}
