using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    public WeaponSwitch weaponSwitch;
    public Image weaponImage;

    public Sprite[] weaponIcons;

    public GameObject player;

    private void Start()
    {
        if(weaponImage == null) { GameObject placeholder = GameObject.Find("WeaponSourceImage"); weaponImage = placeholder.GetComponent<Image>(); }
        if(player == null) { player = GameObject.FindGameObjectsWithTag("Player")[0]; }
        if (weaponSwitch == null) { weaponSwitch = player.GetComponent<WeaponSwitch>(); }
    }

    void Update()
    {
        if (weaponSwitch == null) return;

        GameObject currentWeapon = weaponSwitch.GetCurrentWeapon();

        if (currentWeapon == null) return;

        
        for (int i = 0; i < weaponSwitch.weaponInstances.Length; i++)
        {
            if (weaponSwitch.weaponInstances[i] == currentWeapon)
            {
                weaponImage.sprite = weaponIcons[i];
                break;
            }
        }
    }
}