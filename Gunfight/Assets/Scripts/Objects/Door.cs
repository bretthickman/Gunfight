using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Door : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Collider2D doorCollider;

    // [SerializeField] private AudioClip hitSound;
    public bool isOpen = false;

    public void ActivateDoor()
    {
        if (!isClient) return;
        Debug.Log("Activating door");
        CmdActivateDoor();
    }

    [Command(requiresAuthority = false)]
    public void CmdActivateDoor()
    {
        Debug.Log("server activate door");
        RpcActivateDoor();
    }

    [ClientRpc]
    public void RpcActivateDoor()
    {
        if (isOpen == false) // if the player is trying to open the door
        {
            Debug.Log("Door is opened");
            isOpen = true;
            spriteRenderer.sprite = openSprite;
            doorCollider.enabled = false;
        }
        else if (isOpen == true) // if the player is trying to close the door
        {
            Debug.Log("Door is closed");
            isOpen = false;
            spriteRenderer.sprite = closedSprite;
            doorCollider.enabled = true;
        }
    }

    [ClientRpc]
    public void RpcResetDoor()
    {
        Debug.Log("Reset doors");
        isOpen = false;
        spriteRenderer.sprite = closedSprite;
        doorCollider.enabled = true;
    }
}
