using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public SpriteRenderer tree;
    public Collider2D OtherCollider;
    public bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OtherCollider = other;
            // makes the tree transparent
            tree.color = new Color(1f, 1f, 1f, 0.5f);
            triggered = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // sets the tree back to normal
            tree.color = new Color(1f, 1f, 1f, 1f);
            triggered = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Check if the colliding object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            OtherCollider = other;
            // makes the tree transparent
            tree.color = new Color(1f, 1f, 1f, 0.5f);
            triggered = true;
        }
    }
}
