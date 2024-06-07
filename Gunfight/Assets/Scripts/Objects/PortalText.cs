using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalText : MonoBehaviour
{
    [SerializeField] private Transform endPos;
    public SpriteRenderer text;
    [SerializeField] private Collider2D OtherCollider;
    [SerializeField] private bool nearPortal = false;

    private float dist;

    void OnTriggerEnter2D(Collider2D other)
    {
        OtherCollider = other;
        if (OtherCollider.CompareTag("Player"))
        {
            nearPortal = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (OtherCollider.CompareTag("Player"))
        {
            nearPortal = false;
            text.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (nearPortal)
        {
            dist = Vector3.Distance(endPos.position, OtherCollider.gameObject.GetComponent<Transform>().position);
            text.color = new Color(1.0f, 1.0f, 1.0f, 3.0f/dist);
        }
    }
}
