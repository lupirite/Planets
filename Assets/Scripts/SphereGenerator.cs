using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SphereGenerator : MonoBehaviour
{
    [Header("LOD")]
    public Transform viewer;
    public float distOffset;
    public int LODLevels;
    public float LODFactor;
    public float LODPower = .2f;
    [Header("Generation")]
    public int chunkRes;
    public Material material;
    public float diameter = 10;
    public Vector3 noiseOffset;
    public float minAlt;
    public float maxAlt;
    public float smoothingAngle = 5;
    public int resizeCheckFrameInterval = 5;
    [Header("Editor")]
    [Range(2, 255)]
    public int res;
    [HideInInspector] public int chunkVerts;
    [HideInInspector] public int triArrSize;
    [Header("FullScale")]
    public GameObject treePrefab;
    public int numTrees = 5;
    public LayerMask groundMask;
    public float treeSpread = 10;
    [Header("MarchingChunks")]
    public float renderDistance;

    [HideInInspector] public VectorD3 doublePos;

    float surfaceAngle = -99;
    public int getLODLevel(Vector3 chunkPos, Vector3 normal, float LOD)
    {
        float scaledDist = Vector3.Distance(chunkPos, viewer.position)*(ScaleManager.instance.celestialScaleFactor/100000);
        float edgeMultiplier = 1;
        Vector3 toCenter = transform.position - viewer.position;

        float angle = Vector3.Angle(-toCenter, transform.rotation*normal) - 90 / Mathf.Pow(2, LOD);

        if (angle > 60)
        {
            edgeMultiplier = 0;
        }
        if (angle > 170) { }
        else
        {
            if (surfaceAngle == -99)
            {
                Vector3 dir = viewer.position - transform.position;
                float dist = dir.magnitude - estimateHeight(dir.normalized * diameter / 2) + diameter * .0002f;
                float h = 1 - 1 / (dist / (diameter / 2));
                surfaceAngle = Mathf.Acos((2 - Mathf.Pow(h * 2, 2)) / 2) * Mathf.Rad2Deg / 2;
            }

            if (surfaceAngle < angle) // surface angle is the angle that represents how much of the surface (assumed to be a sphere) is visible. Angle is the angle between the viewer, the center of the planet, and the chunk center
            {
                edgeMultiplier = Mathf.Max(0, Mathf.Min(smoothingAngle, surfaceAngle - angle)/smoothingAngle);
            }
        }

        int level = (int)(Mathf.Min(1/Mathf.Pow(scaledDist+distOffset, LODPower)*LODFactor, LODLevels)*edgeMultiplier);
        return level;
    }

    public static List<Transform> sphereGenerators;
    private void Start()
    {
        doublePos = (VectorD3)transform.position;

        if (sphereGenerators == null)
            sphereGenerators = new List<Transform>();
        sphereGenerators.Add(transform);
        destroyChunks();
        make();
    }

    private void Update()
    {
        surfaceAngle = -99;
    }

    public static Transform getNearest(Vector3 pos)
    {
        Transform nearestOb = null;
        foreach (Transform ob in sphereGenerators)
        {
            if (nearestOb == null || (nearestOb.position - pos).magnitude > (ob.position - pos).magnitude) {
                nearestOb = ob;
            }
        }
        return nearestOb;
    }

    public void make()
    {
        if (Application.isPlaying)
        {
            chunkVerts = chunkRes * chunkRes;
            triArrSize = (chunkRes - 1) * (chunkRes - 1) * 6;
        }
        else
        {
            chunkVerts = res * res;
            triArrSize = (res - 1) * (res - 1) * 6;
        }

        addChunks();
    }

    void addChunks()
    {
        Vector3Int[] dirs = { new Vector3Int(1, 0, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, 0, 1) };
        int i = 0;
        for (int s = -1; s < 2; s += 2)
        {
            for (int d = 0; d < dirs.Length; d++)
            {
                Vector3Int dir = dirs[d] * s;
                GameObject gO = new GameObject("Face" + i.ToString());
                i++;
                gO.transform.parent = transform;
                gO.AddComponent<Chunk>();
                gO.GetComponent<Chunk>().rootChunk = gO.transform;
                gO.GetComponent<Chunk>().xCoord = BinaryInt.one;
                gO.GetComponent<Chunk>().yCoord = BinaryInt.one;
                gO.GetComponent<Chunk>().parentChunk = gameObject;
                gO.GetComponent<Chunk>().dir = dir;
                gO.GetComponent<Chunk>().sphereGenerator = this;
                gO.GetComponent<Chunk>().make();
                gO.GetComponent<Chunk>().LODLevel = 0;
                gO.layer = 6;
                gO.transform.position = transform.position;
                gO.GetComponent<Chunk>().normal = dir;
            }
        }
    }

    public void destroyChunks()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public static double getHeight(VectorD3 pos)
    {
        double h = 0;
        // distant
        //h += simplex3D(pos * .02d) / 500d;
        //h += simplex3D(pos * .1d) / 1000d;
        h += simplex3D(pos * 5d) / 1000d;

        /*h -= .001d;
        if (h < 0)
        {
            h = 0;
        }*/

        // surface
        h += simplex3D(pos * 500d) / 500000d;
        return h;
    }
    
    public static float estimateHeight(Vector3 pos)
    {
        float h = 0;
        h += (float)simplex3D((VectorD3)pos * .02f) / 500f;
        h += (float)simplex3D((VectorD3)pos * .1f) / 1000f;
        return h;
    }

    public static double getHumidity(VectorD3 pos)
    {
        return simplex3D(pos * 1000d) * .5d + simplex3D(pos * 10000d) * .1d;
    }

    public static double simplex3D(VectorD3 pos)
    {
        return DoubleNoise.doubleNoise.Evaluate(pos.x, pos.y, pos.z);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SphereGenerator))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SphereGenerator myTarget = (SphereGenerator)target;

        if (GUILayout.Button("Regenerate Sphere"))
        {
            myTarget.destroyChunks();
            myTarget.make();
        }
    }
}
#endif