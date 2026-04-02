using UnityEngine;

public class GunHolderRotate : MonoBehaviour
{
    public Transform player;          // Reference to player
    public Vector3 rightOffset;       // Offset when facing right
    public Vector3 leftOffset;        // Offset when facing left

    public Vector2 AimDirection { get; set; } = Vector2.right;

    [Header("Rotation limits")]
    public float maxRotationAngle = 40f; // Max angle relative to player facing

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (player == null) return;

        bool facingRight = AimDirection.x >= 0;

        
        transform.localPosition = facingRight ? rightOffset : leftOffset;

        
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;

        
        if (facingRight)
        {
            angle = Mathf.Clamp(angle, -maxRotationAngle, maxRotationAngle);
        }
        else
        {
            // Flip 180 degrees for left
            angle = Mathf.Clamp(angle, -maxRotationAngle, maxRotationAngle) + 180f;
        }

        // Apply rotation
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

       
        transform.localScale = originalScale;
    }
}