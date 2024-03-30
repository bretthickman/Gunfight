using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Mirror;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour, IDamageable
{
    public WeaponInfo weaponInfo;
    public GameObject weapon;
    public int grenades;

    public bool hasSpawned = false;

    public Rigidbody2D rb;

    public Camera cam;

    public PlayerObjectController poc;

    [SerializeField] 
    public int team;

    //Sprite

    public SpriteRenderer spriteRendererBody;
    public SpriteRenderer spriteRendererHair;
    public SpriteRenderer spriteRendererEyes;
    public SpriteRenderer weaponSpriteRenderer;

    public Animator playerAnimator;

    public SpriteLibraryAsset[] bodySpriteLibraryArray;
    public SpriteLibraryAsset[] hairSpriteLibraryArray;
    public SpriteLibraryAsset[] eyesSpriteLibraryArray;

    public SpriteLibrary bodySpriteLibrary;
    public SpriteLibrary hairSpriteLibrary;
    public SpriteLibrary eyesSpriteLibrary;

    //Shooting
    public Transform shootPoint;

    public float cooldownTimer = 0;

    public bool isFiring;

    public CameraShaker CameraShaker;

    [SyncVar]
    public float health = 10f;

    public bool alive = true;

    public GameObject hitParticle;

    public GameObject bulletParticle;

    public AudioClip PistolShotSound;

    public AudioClip UziShotSound;

    public AudioClip SniperShotSound;

    public AudioClip AK47ShotSound;

    public AudioClip KnifeSound;

    public AudioClip Walk_1;

    public AudioClip Walk_2;

    public AudioClip Walk_3;

    public AudioClip Walk_4;

    public AudioClip emptySound;

    public AudioClip breakSound;

    public AudioClip[] HurtsSound;

    private AudioSource audioSource;

    [SerializeField] private GameObject ammo;
    public string skinCategory;

    public void SwitchBodySprite(int index)
    {
       bodySpriteLibrary.spriteLibraryAsset = bodySpriteLibraryArray[index];
    }

    public void SwitchHairSprite(int index)
    {
        hairSpriteLibrary.spriteLibraryAsset = hairSpriteLibraryArray[index];
    }

    public void SwitchEyesSprite(int index)
    {
        eyesSpriteLibrary.spriteLibraryAsset = eyesSpriteLibraryArray[index];
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    private void Start()
    {
        poc = GetComponent<PlayerObjectController>();
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            if (isLocalPlayer)
            {
                Movement();
            }
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            if (!hasSpawned)
            {
                // Spawns player with knife, sets position, team, and sprite
                Debug.Log("Spawning");
                weaponInfo.setDefault();
                //SetPosition();
                Respawn();
                SetTeam();
                health = 10f;
                hasSpawned = true;
            }

            if (isLocalPlayer)
            {
                // Check if you are firing your weapon and if the cooldown is 0
                if (Input.GetButtonDown("Fire1") && cooldownTimer <= 0f)
                {
                    // Camera Shake
                    if (weaponInfo.nAmmo > 0)
                        CameraShaker.ShootCameraShake(5.0f);

                    // Start firing if the fire button is pressed down and this weapon is automatic
                    if (weaponInfo.isAuto)
                    {
                        // Set the isFiring flag to true and start firing
                        isFiring = true;
                        StartCoroutine(ContinuousFire());
                    }
                    else
                    {
                        // Fire a single shot
                        cooldownTimer = weaponInfo.cooldown;
                        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
                        CmdShoot(shootPoint.position, shootPoint.rotation);
                    }
                }
                else if (Input.GetButtonUp("Fire1") && weaponInfo.isAuto)
                {
                    // Stop firing if the fire button is released and this weapon is automatic
                    isFiring = false;
                    StopCoroutine(ContinuousFire());
                }

                // used for testing - kill yourself
                if (Input.GetKeyDown(KeyCode.F))
                {
                    health = 0;
                    RpcDie();
                    playerAnimator.SetBool("isDead", true);
                    SendPlayerDeath();
                }

                // updates weapon cooldown timer
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer < 0) cooldownTimer = 0;
            }
        }
    }

    private IEnumerator ContinuousFire()
    {
        while (isFiring && cooldownTimer <= 0f)
        {
            // Fire a shot and wait for the cooldown timer to expire
            cooldownTimer = weaponInfo.cooldown;
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            CmdShoot(shootPoint.position, shootPoint.rotation);
            if (weaponInfo.nAmmo > 0)
                CameraShaker.ShootCameraShake(5.0f);
            yield return new WaitForSeconds(cooldownTimer);
        }
    }

    public void SetPosition()
    {
        // Determine the spawn points based on the game mode
        Transform[] spawnPoints = GameModeManager.Instance.currentGameMode is not SurvivalMode
            ? MapManager.Instance.FFASpawnPoints
            : MapManager.Instance.SPSpawnPoints;

        // Ensure that PlayerIdNumber is within a valid range
        int playerId = Mathf.Clamp(poc.PlayerIdNumber, 1, spawnPoints.Length);

        // Set the position based on the PlayerIdNumber
        transform.position = spawnPoints[playerId - 1].position;
    }

    public void SetTeam()
    {
        team = poc.PlayerIdNumber-1;
        GetComponent<PlayerWeaponController>().team = team;
    }

    public void Movement()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float yDirection = Input.GetAxis("Vertical");

        Vector3 mousePosition = Input.mousePosition;
        if(cam != null)
            mousePosition = cam.ScreenToWorldPoint(mousePosition);

        if ((mousePosition.x > transform.position.x && spriteRendererBody.flipX) ||
            (mousePosition.x < transform.position.x && !spriteRendererBody.flipX))
        {
            CmdFlipPlayer(spriteRendererBody.flipX);
        }

        Vector2 direction = (mousePosition - weapon.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weapon.transform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 moveDirection = new Vector3(xDirection, yDirection, 0.0f);
        //animate player running if they are moving
        if(moveDirection != new Vector3(0, 0, 0))
        {
            playerAnimator.SetBool("isRunning", true);
        }
        else
        {
            playerAnimator.SetBool("isRunning", false);
        }
        //apply the movement
        rb.MovePosition(transform.position + moveDirection *
                        weaponInfo.speedOfPlayer *
                        Time.deltaTime);
        Physics2D.SyncTransforms();
    }

    [Command]
    void CmdFlipPlayer(bool flipped)
    {
        RpcFlipPlayer(flipped);
    }

    [ClientRpc]
    void RpcFlipPlayer(bool flipped)
    {
        if (flipped == spriteRendererBody.flipX) // Fixes BUG: Flips switch on Client for some reason? better solution...?
        {
            spriteRendererBody.flipX = !spriteRendererBody.flipX;
            spriteRendererHair.flipX = !spriteRendererHair.flipX;
            spriteRendererEyes.flipX = !spriteRendererEyes.flipX;
            weapon.transform.localScale = new Vector3(1, -weapon.transform.localScale.y, 1);
        }
    }

    [ClientRpc]
    void RpcSpawnBulletTrail(Vector2 startPos, Vector2 endPos)
    {
        if (weaponInfo.isMelee)
        {
            AudioSource.PlayClipAtPoint(KnifeSound, startPos, AudioListener.volume);
        }
        else
        {
            if (weaponInfo.id == WeaponID.AK47)
            {
                AudioSource.PlayClipAtPoint(AK47ShotSound, startPos, AudioListener.volume);
            }
            if (weaponInfo.id == WeaponID.Uzi)
            {
                AudioSource.PlayClipAtPoint(UziShotSound, startPos, AudioListener.volume);
            }
            if (weaponInfo.id == WeaponID.Sniper)
            {
                AudioSource.PlayClipAtPoint(SniperShotSound, startPos, AudioListener.volume);
            }
            if (weaponInfo.id == WeaponID.Pistol)
            {
                AudioSource.PlayClipAtPoint(PistolShotSound, startPos, AudioListener.volume);
            }
        }
        if (!weaponInfo.isMelee && weaponInfo.nAmmo > 0)
        {
            Instantiate(bulletParticle.GetComponent<ParticleSystem>(),
            startPos,
            Quaternion.FromToRotation(Vector2.up, endPos-startPos));
            weaponInfo.nAmmo--;
        }

        Vector2 newPoint = endPos + ((endPos - startPos).normalized * -0.2f);
        var hitParticleInstance =
            Instantiate(hitParticle.GetComponent<ParticleSystem>(),
            newPoint,
            Quaternion.identity);
    }

    [Command]
    public void CmdShoot(Vector2 shootPoint, Quaternion gunRotation)
    {
        if (weaponInfo.nAmmo > 0)
        {
            Vector2 direction = gunRotation * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(shootPoint, direction, weaponInfo.range);

            var endPos = hit.point;

            if (hit.collider != null && !hit.collider.CompareTag("Uncolliable"))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();

                //no friendly fire in survival mode
                IGameMode gameMode =  FindObjectOfType<SurvivalMode>();
                if (gameMode == null)
                {
                    Debug.Log("gameMode is null");
                }
                else
                {
                    Debug.Log("game mode is " + gameMode);
                }

                if (damageable != null && (gameMode != null && !hit.collider.gameObject.CompareTag("Player")))
                {
                    damageable.TakeDamage(weaponInfo.damage, hit.point);
                }
            }
            else
            {
                endPos = shootPoint + direction * weaponInfo.range;
            }
            RpcSpawnBulletTrail(shootPoint, endPos);
        }
        else if (!weaponInfo.isMelee)
            RpcPlayEmptySound(shootPoint);
    }

    [ClientRpc]
    void RpcPlayEmptySound(Vector2 startPos)
    {
        AudioSource.PlayClipAtPoint(emptySound, startPos, AudioListener.volume);
    }

    [Command]
    public void CmdPlayerDied()
    {
        // Call the PlayerDied function on the server
        GameModeManager.Instance.currentGameMode.PlayerDied(this);
    }

    public void TakeDamage(int damage, Vector2 hitPoint)
    {
        if (!isServer) return;

        health -= damage;
        Debug.Log("Player took " + damage + " Damage");

        RpcHurtCameraShake();

        if (health <= 0)
        {
            RpcDie();
            playerAnimator.SetBool("isDead", true);
            SendPlayerDeath();
        }
        else
        {
            RpcHitColor();
        }
    }

    private void SendPlayerDeath()
    {
        if (isOwned)
        {
            // If the object has authority (belongs to the local player), send a command to notify the server about the death
            CmdPlayerDied();
        }
        else
        {
            // If the object does not have authority, it's likely a remote player object, and we don't need to do anything on the client-side.
            // The server will handle the death logic, and the state will be synchronized to this client automatically.
            GameModeManager.Instance.currentGameMode.PlayerDied(this);
        }
    }

    [ClientRpc]
    void RpcHurtCameraShake()
    {
        if (isLocalPlayer)
        {
            CameraShaker.HurtCameraShake(5.0f);
        }
    }

    IEnumerator FlashSprite()
    {
        spriteRendererBody.color = Color.red;
        spriteRendererHair.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRendererBody.color = Color.white;
        spriteRendererHair.color = Color.white;
    }

    [ClientRpc]
    void RpcHitColor()
    {
        StartCoroutine(FlashSprite());
    }

    [ClientRpc]
    void RpcDie()
    {
        weaponInfo.nAmmo = 0;
        weaponInfo.range = 0;
        weaponInfo.damage = 0;
        weaponInfo.speedOfPlayer = 0;
        weaponSpriteRenderer.enabled = false;
        //spriteRenderer.enabled = false;
        GetComponent<PlayerWeaponController>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    public void Respawn()
    {
        SetPosition();
        health = 10f;
        weaponInfo.setDefault();
        GetComponent<PlayerWeaponController>().ChangeSprite(WeaponID.Knife);
        spriteRendererBody.color = Color.white; // prevents sprite from having the red damage on it forever
        spriteRendererHair.color = Color.white;
        playerAnimator.SetBool("isDead", false);
        GetComponent<PlayerWeaponController>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        weaponSpriteRenderer.enabled = true;
        spriteRendererBody.enabled = true;
    }
}
