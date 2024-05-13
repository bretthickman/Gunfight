using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliders : MonoBehaviour
{
    public bool canPickup = false;
    public bool canActivateDoor = false;
    public Collider2D OtherCollider;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            canPickup = true;
            OtherCollider = other;
        }
        else if (other.CompareTag("Door"))
        {
            canActivateDoor = true;
            OtherCollider = other;
        }
        else if (other.CompareTag("Stele"))
        {
            OtherCollider = other;
            other.gameObject.GetComponent<Stele>().TurnOnText(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Weapon"))
        {
            canPickup = false;
        }
        else if (other.CompareTag("Door"))
        {
            canActivateDoor = false;
        }
        else if (other.CompareTag("Stele"))
        {
            other.gameObject.GetComponent<Stele>().TurnOnText(false);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the colliding object has the "Player" tag
        if (other.CompareTag("Weapon"))
        {
            OtherCollider = other;
            canPickup = true;
        }
        else if (other.CompareTag("Door"))
        {
            OtherCollider = other;
            canActivateDoor = true;
        }
        else if (other.CompareTag("Stele"))
        {
            OtherCollider = other;
            //other.gameObject.GetComponent<Stele>().TurnOnText(true);
        }
    }
}
