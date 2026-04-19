using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    public WeaponSwitch weaponSwitch;
    public Image weaponImage;

    public Sprite[] weaponIcons; 

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