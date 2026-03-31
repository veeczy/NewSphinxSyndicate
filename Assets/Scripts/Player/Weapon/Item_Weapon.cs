using UnityEngine;

public class Item_Weapon : MonoBehaviour
{
    private WeaponSwitch weaponScript;
    public int weaponIndex;

    void Start()
    {
        weaponScript = GameObject.FindWithTag("Player").GetComponent<WeaponSwitch>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            weaponScript.weaponInventory[weaponIndex] = true;
            PlayerPrefs.SetInt("Weapon_" + weaponIndex, 1); // new, save the weapon pickup
            weaponScript.EquipWeapon(weaponIndex);
            Destroy(gameObject);
        }
    }
}
