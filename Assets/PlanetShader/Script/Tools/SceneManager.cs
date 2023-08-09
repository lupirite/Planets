using UnityEngine;
using System.Collections;

public class SceneManager : MonoBehaviour
{

    public float planetRotSpeed = 1.0f;
    public float sunRotSpeed = 1.0f;
    public Transform sun;
    public Transform[] planets = new Transform[9];

    void Update()
    {
        foreach (Transform planet in planets)
            planet.Rotate(new Vector3(0, planetRotSpeed * Time.deltaTime, 0));
        if (Input.GetKey(KeyCode.Z))
            sun.Rotate(new Vector3(0, sunRotSpeed * Time.deltaTime, 0));
        else if (Input.GetKey(KeyCode.C))
            sun.Rotate(new Vector3(0, -sunRotSpeed * Time.deltaTime, 0));
    }
}
