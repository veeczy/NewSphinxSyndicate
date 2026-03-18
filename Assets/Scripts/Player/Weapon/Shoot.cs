using UnityEngine;
using System.Collections;
public class Shoot : MonoBehaviour
{
    public Transform bulletSpawn;
    public GameObject bulletPrefab;
    public GameObject playerObj;
    public AudioSource gunSounds;
    public AudioClip shootSound;
    public float shootDelay = 1f;
    public float cooldown;
    public float velocityMultiplier = 10f;
    public bool toggleFlash = false;
    public Light muzzleFlash;
    public bool isShooting = false;
    public bool inGame = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerObj = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        inGame =! (playerObj.GetComponent<PlayerMovement>().canMove); //if you can move, youre not in a game so inGame will be false : if you cant move, youre in a game and inGame will be true

        if (Input.GetButton("Shoot") && !isShooting && !inGame) //ADDED so that you can't shoot if youre in a game
        {
            StartCoroutine("shoot");
        }
    }
    IEnumerator shoot()
    {
        isShooting = true;
        cooldown = shootDelay;
        Rigidbody2D bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation).GetComponent<Rigidbody2D>();
        if (playerObj && bullet.GetComponent<BulletId>())
            bullet.GetComponent<BulletId>().sender = playerObj;
        bullet.AddForce(bulletSpawn.right * velocityMultiplier, ForceMode2D.Impulse);
        if (shootSound != null)
        {
            gunSounds.PlayOneShot(shootSound);
        }
        if (toggleFlash)
        {
            muzzleFlash.enabled = true;
            yield return new WaitForSeconds(0.01f);
            muzzleFlash.enabled = false;
        }
        yield return new WaitForSeconds(shootDelay);
        isShooting = false;
    }
}
