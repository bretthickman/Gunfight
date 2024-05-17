using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stele : NetworkBehaviour
{
    [SerializeField] private GameObject text;
    [SerializeField] private Collider2D OtherCollider;

    void OnTriggerEnter2D(Collider2D other)
    {
        OtherCollider = other;
        if (OtherCollider.CompareTag("Player"))
        {
            TurnOnText(true);
        }
    }

    public void TurnOnText(bool tOrF)
    {
        text.SetActive(tOrF);
        if (tOrF)
            RpcSteleSound();
    }

    [ClientRpc]
    public void RpcSteleSound()
    {
        Debug.Log("Player near stele");
    }

     void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TurnOnText(false);
        }
    }
}
