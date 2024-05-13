using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stele : NetworkBehaviour
{
    [SerializeField] private GameObject text;
    [SerializeField] private Collider2D OtherCollider;
    // when player gets close to stele, sound will play for nearby players

    public void TurnOnText(bool tOrF)
    {
        Debug.Log("Changing stele text");
        text.SetActive(tOrF);
        // it is set to active but not showing, need to debug
    }


}
