using UnityEngine;
using UnityEngine.InputSystem;

public class MouseCameraController : MonoBehaviour
{
    public float sensitivity = 2.0f; // Mouse sensitivity
    public Transform playerBody; // Player's body transform
    public Transform player;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // Lock cursor to the game window and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Read mouse input
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        // Calculate vertical rotation for camera
        rotationX -= mouseY;
        rotationY += mouseX;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Clamp vertical rotation to avoid flipping

        // Rotate the player's body horizontally
        playerBody.localRotation = Quaternion.Euler(0, rotationY, 0);

        // Apply rotation to camera
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}