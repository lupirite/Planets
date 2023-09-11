using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;

public class ScaleManager : MonoBehaviour
{
    public static ScaleManager instance;

    public float celestialScaleFactor = 100;
    public float stellarScaleFactor = 10;
    public GameObject unscaledCamera;
    public GameObject celestialCamera;

    public LayerMask fullScaleMask;
    public LayerMask celestialMask;

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        celestialCamera.transform.rotation = unscaledCamera.transform.rotation;
        celestialCamera.transform.position = unscaledCamera.transform.position/celestialScaleFactor;

        Vector3 offset = unscaledCamera.transform.position;

        GameObject[] allGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject g in allGameObjects)
        {
            if ((fullScaleMask & (1 << g.layer)) != 0)
            {
                g.transform.position -= offset;
            }
            else if ((celestialMask & (1 << g.layer)) != 0)
            {
                if (g.GetComponent<SphereGenerator>())
                {
                    g.GetComponent<SphereGenerator>().doublePos -= (VectorD3)offset / (double)celestialScaleFactor;
                }
                g.transform.position -= offset/celestialScaleFactor;
            }
        }
    }

    public Vector3 worldToCelestial(Vector3 worldPos)
    {
        return worldPos/celestialScaleFactor;
    }

    public Vector3 celestialToWorld(Vector3 scaledPos)
    {
        return scaledPos*celestialScaleFactor;
    }
}
