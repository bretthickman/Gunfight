using UnityEngine;
using Mirror;
public enum WeaponID
{
    AK47,
    Knife,
    Pistol,
    Sniper,
    Uzi
}

public class WeaponInfo : NetworkBehaviour
{
    public WeaponID id;

    public int nAmmo;

    public int damage;

    public float range;

    public float speedOfPlayer;

    public float cooldown;

    public bool isAuto;

    public bool isMelee;

    public void setDefault()
    {
        id = WeaponID.Knife;
        nAmmo = 1000;
        damage = 10;
        range = 2f;
        speedOfPlayer = 10;
        cooldown = 0.3f;
        isAuto = false;
        isMelee = true;
    }

    public void setWeaponInfo(WeaponInfo newInfo)
    {
        id = newInfo.id;
        nAmmo = newInfo.nAmmo;
        damage = newInfo.damage;
        range = newInfo.range;
        speedOfPlayer = newInfo.speedOfPlayer;
        cooldown = newInfo.cooldown;
        isAuto = newInfo.isAuto;
        isMelee= newInfo.isMelee;
    }
}
