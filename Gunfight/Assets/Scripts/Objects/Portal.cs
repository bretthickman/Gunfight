using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Portal : NetworkBehaviour
{
    [SerializeField] private Transform endPos;
    [SerializeField] private bool canPortal = false;
    public bool leftPortal = false;
    [SerializeField] private Collider2D OtherCollider;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        OtherCollider = other;
        // if the player just teleported
        if (OtherCollider.gameObject.GetComponent<PlayerController>().hasTeleported)
        {
            leftPortal = false;
            canPortal = false;
        }
        else // if the player didnt just teleport
        {
            canPortal = true;
            StartCoroutine(TeleportPlayer());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player.hasTeleported && endPos.gameObject.GetComponent<Portal>().leftPortal && !leftPortal)
        {
            player.hasTeleported = false;
        }
        player.GetComponent<PlayerController>().CmdPlayerMat(0);
        leftPortal = true;
        canPortal = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        OtherCollider = other;
    }

    public IEnumerator TeleportPlayer()
    {
        GameObject player = OtherCollider.gameObject;
        player.GetComponent<PlayerController>().CmdPlayerMat(1);
        yield return new WaitForSeconds(1f);
        if (canPortal && !player.GetComponent<PlayerController>().hasTeleported)
        {
            leftPortal = true;
            player.GetComponent<Transform>().position = endPos.position;
            Debug.Log("Player is teleporting");
            player.GetComponent<PlayerController>().hasTeleported = true;
            player.GetComponent<PlayerController>().CmdPlayerMat(0);
        }
    }
}
