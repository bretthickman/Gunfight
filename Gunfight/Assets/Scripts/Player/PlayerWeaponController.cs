using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.AddressableAssets;

public class PlayerWeaponController : NetworkBehaviour
{
    [SerializeField]
    internal int team;

    public PlayerController player;

    [SerializeField]
    public Sprite[] weaponSpriteArray;

    [SerializeField]
    internal List<Sprite> spriteArray;

    //Weapon
    public PlayerColliders playerColliders;

    public AudioClip pickupSound;

    // prefab array of weapons
    [SerializeField]
    private GameObject [] weapons;

    [SerializeField]
    private GameObject Grenade;

    public bool shouldChange = false;
    public WeaponInfo weaponInfoChange = null;

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            if (playerColliders.canPickup && Input.GetKeyDown(KeyCode.Mouse1))
            {
                // Pick up the weapon
                if(player.weaponInfo.id != WeaponID.Knife)
                    CmdDrop(player.weaponInfo);
                CmdPickUp(playerColliders.OtherCollider.GetComponent<WeaponInfo>());
            }
            else if(player.weaponInfo.id != WeaponID.Knife && !playerColliders.canPickup && Input.GetKeyDown(KeyCode.Mouse1))
            {
                CmdDrop(player.weaponInfo);
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                player.grenades -= 1;
                CmdThrowGrenade();
            }
        }

    }

    void throwObject(Rigidbody2D rigidBody, float velocity, float angularVelocity, float drag, float angularDrag)
    {
        rigidBody.velocity = velocity * (player.shootPoint.transform.position - transform.position);
        rigidBody.angularVelocity = angularVelocity;
        rigidBody.drag = drag;
        rigidBody.angularDrag = angularDrag;
    }

    [Command]
    void CmdThrowGrenade()
    {
        RpcThrowGrenade();
    }

    [ClientRpc]
    void RpcThrowGrenade()
    {
        GameObject newGrenade = Instantiate(Grenade, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        Rigidbody2D weaponRigidbody = newGrenade.GetComponent<Rigidbody2D>();
        throwObject(weaponRigidbody, 30f, -50f * 10f, 3.0f, 1f);
    }

    [Command]
    void CmdDrop(WeaponInfo weaponInfo)
    {
        RpcDropWeapon(weaponInfo);
    }

    [ClientRpc]
    void RpcDropWeapon(WeaponInfo weaponInfo)
    {
        // gets int value of weapon enum
        int index = (int) weaponInfo.id;
        // gets the actual weapon GameObject
        var weapon = weapons[index];

        if (isServer)
        {
            GameObject newWeapon = Instantiate(weapon, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));

            NetworkServer.Spawn(newWeapon);

            Rigidbody2D weaponRigidbody = newWeapon.GetComponent<Rigidbody2D>();
            // throws object along the ground with a velocity and spin
            throwObject(weaponRigidbody, 10f, -50f * 10f, 3.5f, 1f);
            //newWeapon.GetComponent<Collider2D>().isTrigger = false; // NEED A BETTER SOLUTION: weapons should be able to hit walls but the player can still walk over them
            //StartCoroutine(TurnOnTrigger(newWeapon.GetComponent<Collider2D>()));
            newWeapon.GetComponent<WeaponInfo>().setWeaponInfo(weaponInfo);
        }
        // if you want to throw your weapon and equip a knife
        if (playerColliders.canPickup == false)
        {
            player.weaponInfo.setDefault();
            player.isFiring = false;
            ChangeSprite(WeaponID.Knife);
        }
    }

    // Refactor: Be able to have a collision order so the weapons collide with walls but not the player
    IEnumerator TurnOnTrigger(Collider2D collider)
    {
        yield return new WaitForSeconds(0.5f); // wait for half a second
        collider.isTrigger = true;
    }

    [Command]
    void CmdPickUp(WeaponInfo weaponInfo)
    {
        RpcDestroyWeapon(weaponInfo);
    }

    [ClientRpc]
    public void RpcDestroyWeapon(WeaponInfo weaponInfo)
    {
        if (playerColliders.OtherCollider != null)
        {
            // picks up a weapon on the ground, changes sprite, updates changes player stats, and destroys the weapon
            ChangeSprite(weaponInfo.id);
            AudioSource.PlayClipAtPoint(pickupSound, player.transform.position, AudioListener.volume);
            player.weaponInfo.setWeaponInfo(weaponInfo);
            GetComponent<PlayerController>().cooldownTimer = 0f;
            GetComponent<PlayerController>().isFiring = false;
            Destroy(playerColliders.OtherCollider.gameObject);
            if (isServer)
                NetworkServer.Destroy(playerColliders.OtherCollider.gameObject);
            playerColliders.OtherCollider = null;
        }
    }

    public void ChangeSprite(WeaponID weapon)
    {
        // gets int value of WeaponId enum
        int weaponVal = (int) weapon;

        // gets int value of Team enum
        int teamVal = (int) team;

        if (weapon == (WeaponID)(-1))
        {
            int index = 5 * 4 + teamVal;
            player.weaponSpriteRenderer.sprite = weaponSpriteArray[(int)weapon];
        }
        else
        {
            // change sprite
            int index = weaponVal * 4 + teamVal;
            player.weaponSpriteRenderer.sprite = weaponSpriteArray[(int)weapon];
        }
    }
}
