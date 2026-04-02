using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public Rigidbody2D myPlayer;
    public float speed = 1.0f;
    public float stopSpeed = 1.0f;
    public Vector2 direction;
    // Reference to the BlackJack script on the BJ NPC object
    public bool canMove = true;
    private GameObject BlackJackObject;
    

    [Header("Dodge Settings")]
    public float dodgeDistance = 1f;
    public float dodgeDuration = 0.15f;
    public float dodgeCooldown = 0.6f;
    public Sprite dodgeSprite;
    private Sprite playerSprite;
    public bool dodgekeypress = false;
    public bool dodgeclick = false;

    [Header("Charge Dodge Settings")]
    public bool chargeDodge = false;
    public bool chargeDodgeStart = false;
    public bool isCharging = false;
    public float chargeTimer = 0f;
    public Collider2D hit;
    ContactFilter2D contactFilter;
    LayerMask mask;
    Vector3 offsetPos;
    public GameObject offset;
    bool isSwamp;

    [Header("Player Objects")]
    public GameObject weaponObject;
    public bool isDodging = false;
    private bool canDodge = true;
    private Vector2 dodgeStart;
    private Vector2 dodgeEnd;
    private float dodgeTimer;

    [Header("Audio")]
    public AudioClip footstepAudio;
    private AudioSource playerAudio;
    public AudioClip dodgeAudio;
    public float dodgeVolume = 1f;

    [Header("Aim")]
    public Vector2 aimPos;
    public float controllerAimDist = 5f;
    public Vector2 lastStickPos;
    public bool useCursor = true;

    private Animator anim;
    public bool controller = false;
    public Vector2 deadzone = new Vector2(0.5f, 0.5f);
    public Vector2 stickAxis;

    private Vector3 originalScale;

    public GunHolderRotate gun;
    public Transform bulletSpawn;
    public GameObject weaponObjectReference; 
    public Transform aimCursor;

    void Start()
    {
        myPlayer.linearDamping = stopSpeed;
        myPlayer.gravityScale = 0;
        playerSprite = GetComponent<SpriteRenderer>().sprite;
        playerAudio = GetComponent<AudioSource>();
        playerAudio.clip = footstepAudio;

        BlackJackObject = GameObject.Find("BJ-NPC-Test");
        if (BlackJackObject != null)
            canMove = BlackJackObject.GetComponent<BlackJack>().canMove;

        anim = GetComponent<Animator>();

        if (offset == null)
            offset = GameObject.Find("GunHolder");

        originalScale = transform.localScale;

        if (gun == null && offset != null)
            gun = offset.GetComponent<GunHolderRotate>();
    }

    void Update()
    {
        // Detect controller vs mouse
        stickAxis = new Vector2(Input.GetAxis("Joystick Aim X"), Input.GetAxis("Joystick Aim Y"));
        if (!controller && (stickAxis.sqrMagnitude > deadzone.sqrMagnitude || stickAxis.sqrMagnitude < -deadzone.sqrMagnitude))
            controller = true;
        else if (controller && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            controller = false;

        if (Input.GetButtonDown("Dodge")) dodgekeypress = true;
        if (Input.GetButtonUp("Dodge"))
        {
            dodgeclick = true;
            dodgekeypress = false;
        }

        if (BlackJackObject != null)
            canMove = BlackJackObject.GetComponent<BlackJack>().canMove;

        if (SetCursor.Instance != null)
            SetCursor.Instance.SetCrosshair(aimPos);

        // AIM CALCULATION 
        Vector2 aimDir;
        if (!controller)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
            aimPos = worldMouse;
            aimDir = ((Vector2)aimPos - (Vector2)bulletSpawn.position).normalized;
        }
        else
        {
            if (stickAxis.sqrMagnitude > deadzone.sqrMagnitude)
            {
                Vector2 stickPos = stickAxis.normalized;
                aimDir = stickPos;
                aimPos = (Vector2)transform.position + aimDir * controllerAimDist;
                lastStickPos = stickPos;
            }
            else
            {
                aimDir = lastStickPos;
                aimPos = (Vector2)transform.position + aimDir;
            }
        }

       
        if (gun != null)
            gun.AimDirection = aimDir;

        //  FLIP PLAYER BASED ON GUN AIM 
        if (gun != null)
        {
            Vector3 scale = transform.localScale;
            scale.x = gun.AimDirection.x >= 0 ? Mathf.Abs(originalScale.x) : -Mathf.Abs(originalScale.x);
            transform.localScale = scale;

            if (weaponObject != null)
            {
                Vector3 weaponScale = weaponObject.transform.localScale;
                weaponScale.x = scale.x;
                weaponObject.transform.localScale = weaponScale;
            }
        }

        // --- UPDATE AIM CURSOR ---
        if (aimCursor != null)
            aimCursor.position = aimPos;
    }

    void FixedUpdate()
    {
       
        if (isDodging)
        {
            dodgeTimer += Time.fixedDeltaTime;
            float t = dodgeTimer / dodgeDuration;
            myPlayer.MovePosition(Vector2.Lerp(dodgeStart, dodgeEnd, t));
            anim.SetBool("isWalking", false);
            anim.SetBool("isDodging", true);
            if (t >= 1f)
            {
                isDodging = false;
                StartCoroutine(DodgeCooldown());
            }
            return;
        }

        if (isSwamp) mask = LayerMask.GetMask("SwampWall");
        else mask = LayerMask.GetMask("Wall");
        contactFilter.layerMask = mask;
        offsetPos = offset.transform.position;

        if (dodgekeypress)
        {
            chargeTimer += Time.fixedDeltaTime;
            if (chargeTimer > .3f)
            {
                isCharging = true;
                anim.SetBool("ischarging", isCharging);
            }
            if (chargeTimer > 1.5) chargeDodge = true;
            else chargeDodge = false;
        }
        if (!dodgekeypress)
        {
            if (chargeTimer <= 1.5) chargeTimer = 0;
            isCharging = false;
        }

        if (!canMove) direction = Vector2.zero;
        if (canMove)
        {
            direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            myPlayer.linearVelocity = direction * speed;
        }

        bool isMoving = direction.magnitude > 0.1f;
        anim.SetBool("isWalking", isMoving);
        anim.SetBool("isDodging", isDodging);
        anim.SetBool("ischarging", isCharging);
        anim.SetBool("chargeRoll", chargeDodge);

        if (!isDodging && isMoving)
        {
            if (!playerAudio.isPlaying)
                playerAudio.Play();
        }
        else
        {
            if (playerAudio.isPlaying)
                playerAudio.Stop();
        }

        hit = Physics2D.OverlapCircle(offsetPos, .5f, mask);

        if (chargeDodgeStart)
        {
            dodgeTimer += Time.fixedDeltaTime;
            if (hit != null)
            {
                chargeDodgeStart = false;
                anim.SetBool("isDodging", false);
                StartCoroutine(DodgeCooldown());
            }
            if (hit == null)
            {
                myPlayer.position = myPlayer.position + gun.AimDirection * dodgeTimer;
                anim.SetBool("isWalking", false);
                anim.SetBool("isDodging", true);
                anim.SetBool("ischarging", false);
            }
            return;
        }

        if (canDodge)
        {
            if ((dodgeclick) && !chargeDodge)
                StartDodge(gun.AimDirection);
            if ((dodgeclick) && chargeDodge)
                StartChargeRoll(gun.AimDirection);
        }
    }

    private void StartDodge(Vector2 dir)
    {
        isDodging = true;
        canDodge = false;
        dodgeTimer = 0f;

        dodgeStart = myPlayer.position;

        if (dodgeAudio != null) playerAudio.PlayOneShot(dodgeAudio, dodgeVolume);

        dodgeEnd = dodgeStart + dir * dodgeDistance;
        dodgeclick = false;
    }

    private void StartChargeRoll(Vector2 dir)
    {
        chargeTimer = 0;
        dodgeTimer = 0f;
        chargeDodgeStart = true;
        dodgeclick = false;
        chargeDodge = false;
    }

    private IEnumerator DodgeCooldown()
    {
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }
}