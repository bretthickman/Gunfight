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
            OtherCollider.gameObject.GetComponent<PlayerController>().SteleCameraShake();
        }
    }

    public void TurnOnText(bool tOrF)
    {
        text.SetActive(tOrF);
        if (tOrF)
            CmdPlaySteleSound();
    }

    [Command(requiresAuthority = false)]
    public void CmdPlaySteleSound()
    {
        Debug.Log("Player near stele");
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
