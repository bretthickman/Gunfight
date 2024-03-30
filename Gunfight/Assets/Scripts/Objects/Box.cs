using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Box : NetworkBehaviour, IDamageable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite brokenSprite;

    [SerializeField] private AudioClip hitSound;

    [SerializeField] private GameObject objectInsideBox; //could be turned into List for random object drops


    public bool TakeDamage(int damageAmount, Vector2 hitPoint)
    {
        RpcTakeDamage(hitPoint);

        GameObject ammoInstance = Instantiate(objectInsideBox, hitPoint, Quaternion.identity);

        NetworkServer.Spawn(ammoInstance);

        return false;
    }

    [ClientRpc]
    private void RpcTakeDamage(Vector2 hitPoint)
    {
        spriteRenderer.sprite = brokenSprite;

        AudioSource.PlayClipAtPoint(hitSound, hitPoint, AudioListener.volume);

        gameObject.GetComponent<Collider2D>().enabled = false;

        gameObject.layer = 9; //walk over
    }

    [ClientRpc]
    public void RpcResetBox()
    {
        spriteRenderer.sprite = normalSprite;

        gameObject.GetComponent<Collider2D>().enabled = true;

        gameObject.layer = 7;
    }
}
