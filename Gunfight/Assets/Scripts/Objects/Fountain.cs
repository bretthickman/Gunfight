using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Fountain : NetworkBehaviour
{
    [SyncVar]
    public float hp = 15f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D otherCollider;
    [SerializeField] public bool canHeal = false;
    [SerializeField] public bool active = true; // if fountain is on or off

    public void ResetHealth()
    {
        // reset hp for each round
        hp = 15f;
        active = true;
        // changes water back to normal
        RpcChangeFountainColor(1f, 1f, 1f);
        Debug.Log("Reset fountain hp");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canHeal = true;
            otherCollider = other;
            HealingPlayer();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        otherCollider = other;
        canHeal = true;
    }

    public void HealingPlayer()
    {
        PlayerController player = otherCollider.gameObject.GetComponent<PlayerController>();
        if (canHeal && hp > 0)
        {
            if (player.health < 10)
            {
                StartCoroutine(player.Heal(this));
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        canHeal = false;
    }

    [Command(requiresAuthority = false)]
    public void CmdHealPlayer()
    {
        hp -= 1;
        Debug.Log("Healing a player");
        if (hp == 0)
        {
            Debug.Log("Fountain can no longer heal");
            // changes water to a dark blue
            RpcChangeFountainColor(0.3474991f, 0.3477602f, 0.5377358f);
            active = false;
        }
    }

    [ClientRpc]
    public void RpcChangeFountainColor(float r, float g, float b)
    {
        spriteRenderer.color = new Color(r, g, b, 1f);
    }
}
