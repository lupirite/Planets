using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shipSpawner : MonoBehaviour
{
    public GameObject shipPrefab;
    public Vector2 spawnOffset;
    public Transform cam;
    GameObject ship;

    private void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            if (!ship)
            {
                ship = Instantiate(shipPrefab);
            }
            ship.transform.position = transform.position + cam.up * spawnOffset.y + cam.forward * spawnOffset.x;
            ship.transform.rotation = cam.rotation;
        }
    }
}
