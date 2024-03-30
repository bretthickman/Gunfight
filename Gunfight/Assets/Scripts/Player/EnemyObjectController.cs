using System.Collections;
using UnityEngine;
using Mirror;
using Unity.Burst.CompilerServices;

public class EnemyObjectController : NetworkBehaviour, IDamageable
{
    public Pathfinding.AIDestinationSetter target;
    public Pathfinding.AIPath path;
    public SpriteRenderer spriteRenderer;

    public float health;

    public float speed;
    public float speedOffset = 0.1f;
    public float speedMultipiler = 0.25f;
    public float damage;
    public float damageMultipiler = 0.5f;
    public float attackInterval;
    public GameObject closestPlayer;
    public Animator ratAnimator;

    private float attackCooldownRemaining = 0;

    private GameObject[] players;

    private Vector3 previousPosition;

    void Start()
    {
        health = 10.0f;
        players = GameObject.FindGameObjectsWithTag("Player");
        target.target = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating("FindClosest", 0f, 3f);
        path.maxSpeed *= speed + Random.Range(-speedOffset,speedOffset);
        speed = path.maxSpeed;
        previousPosition = transform.position;
    }

    void Update()
    {
        updateFlip();
    }

    private void FindClosest()
    {
        float closestDistance = float.MaxValue;
        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                target.target = player.transform;
            }
        }

    }

    public void updateSpeed(int currentRound)
    {
        float newSpeed = speed + (currentRound - 1) * speedMultipiler + Random.Range(-speedOffset, speedOffset);
        path.maxSpeed = newSpeed;
        speed = path.maxSpeed;
    }

    public void updateDamage(int currentRound)
    {
        damage = damage + (currentRound - 1) * damageMultipiler;
    }

    void updateFlip()
    {
        // Calculate the change in position
        Vector3 deltaPosition = transform.position - previousPosition;

        // Update the previous position for the next frame
        previousPosition = transform.position;

        if (deltaPosition.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }

    public bool TakeDamage(int damage, Vector2 hitPoint)
    {

        health -= damage;
        Debug.Log("Zombie took " + damage + " Damage");

        if (health <= 0)
        {
            RpcDie();
            return true;
        }
        else
        {
            RpcHitColor();
            return false;
        }
    }

    // damage player every attackInterval seconds
    public void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.transform.CompareTag("Player"))
        {
            if(attackCooldownRemaining <= 0)
            {
                attackCooldownRemaining = attackInterval;
                PlayerController p = collision.collider.gameObject.GetComponent<PlayerController>();
                HitPlayer(p);

            }
            else
            {
                attackCooldownRemaining -= Time.deltaTime;
            }
        }
    }

    // reset attack timer when collision stops
    public void OnCollisionExit2D(Collision2D collision)
    {
        
        if (collision.gameObject.transform.CompareTag("Player"))
        {
            if (attackCooldownRemaining > 0)
            {
                attackCooldownRemaining = 0;
            }
        }
    }

    public void HitPlayer(PlayerController player)
    {
        // player.takeDamage doesn't use collision location, give it a dummy var
        Vector2 collisionLocation = new Vector2(0, 0);
        player.TakeDamage(Mathf.FloorToInt(damage), collisionLocation);
    }

    void RpcDie()
    {
        path.maxSpeed = 0;
        ratAnimator.SetTrigger("death");
        GetComponent<Collider2D>().enabled = false;
        GameModeManager.Instance.currentGameMode.DecrementCurrentNumberOfEnemies();
        Destroy(gameObject, 0.5f);
    }

    IEnumerator FlashSprite()
    {
        // makes player flash red when hit
        Color temp = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    [ClientRpc]
    void RpcHitColor()
    {
        StartCoroutine(FlashSprite());
    }
}
