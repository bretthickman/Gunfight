using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleCameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Speed of camera movement
    [SerializeField] private float zoomSpeed = 5f; // Speed of camera zooming
    [SerializeField] private float maxZoom = 10f; // Maximum zoom level
    [SerializeField] private float minZoom = 2f; // Minimum zoom level
    [SerializeField] private float xBound = 4.5f; // Maximum zoom level
    [SerializeField] private float yBound = 4.5f; // Minimum zoom level

    private float topdownOffset = 0.75f;

    void Update()
    {
        // Camera movement controls
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Camera zooming controls
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomAmount = new Vector3(0f, 0f, scrollInput * zoomSpeed);
        transform.position += zoomAmount;

        // Clamp camera position to prevent going out of bounds
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -xBound, xBound); // Adjust -10f and 10f as needed for your scene
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -yBound - topdownOffset, yBound); // Adjust -10f and 10f as needed for your scene
        transform.position = clampedPosition;

        // Clamp camera zoom level
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            Mathf.Clamp(transform.position.z, -maxZoom, -minZoom)
        );
    }
}
