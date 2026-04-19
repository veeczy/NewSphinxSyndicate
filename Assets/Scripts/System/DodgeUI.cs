using UnityEngine;
using TMPro;

public class DodgeUIText : MonoBehaviour
{
    public TextMeshProUGUI dodgeText;

    public float dodgeCooldown = 0.6f;

    private float cooldownTimer = 0f;

    void Update()
    {
        
        if (Input.GetButtonUp("Dodge") && cooldownTimer <= 0f)
        {
            cooldownTimer = dodgeCooldown;
        }

        // Countdown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            dodgeText.text = "COOLDOWN";
        }
        else
        {
            dodgeText.text = "READY";
        }
    }
}