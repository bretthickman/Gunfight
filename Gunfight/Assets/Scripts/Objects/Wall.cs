using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Wall : NetworkBehaviour, IDamageable
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private Sprite[] walls;

    // [SerializeField] private AudioClip hitSound;

    public bool TakeDamage(int damageAmount, Vector2 hitPoint)
    {
        RpcTakeDamage(hitPoint);
        return false;
    }

    [ClientRpc]
    private void RpcTakeDamage(Vector2 hitPoint)
    {
        spriteRenderers[0].sprite = walls[0];
        spriteRenderers[0].gameObject.GetComponent<Collider2D>().enabled = true;
        spriteRenderers[1].sprite = walls[1];
        spriteRenderers[1].gameObject.GetComponent<Collider2D>().enabled = true;

        // AudioSource.PlayClipAtPoint(hitSound, hitPoint, AudioListener.volume);
        
        gameObject.GetComponent<Collider2D>().enabled = false;
    }

    [ClientRpc]
    public void RpcResetWall()
    {
        foreach(SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sprite = walls[2];
            spriteRenderer.gameObject.GetComponent<Collider2D>().enabled = false;
        }
        
        gameObject.GetComponent<Collider2D>().enabled = true;
    }
}
