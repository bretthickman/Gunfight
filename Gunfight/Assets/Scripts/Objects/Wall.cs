using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Wall : NetworkBehaviour, IDamageable
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    // [SerializeField] private AudioClip hitSound;

    public void TakeDamage(int damageAmount, Vector2 hitPoint)
    {
        RpcTakeDamage(hitPoint);
    }

    [ClientRpc]
    private void RpcTakeDamage(Vector2 hitPoint)
    {
        foreach(SpriteRenderer sprite in spriteRenderers)
        {
            sprite.enabled = false;
        }
        // AudioSource.PlayClipAtPoint(hitSound, hitPoint, AudioListener.volume);
        gameObject.GetComponent<Collider2D>().enabled = false;
    }

    [ClientRpc]
    public void RpcResetWall()
    {
        foreach(SpriteRenderer sprite in spriteRenderers)
        {
            sprite.enabled = true;
        }
        
        gameObject.GetComponent<Collider2D>().enabled = true;
    }
}
