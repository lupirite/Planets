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
    public AnimationCurve sideMapFunc;
    public float radius = 10;
    [Header("Editor")]
    [Range(2, 255)]
    public int res;
    [HideInInspector] public int chunkVerts;
    [HideInInspector] public int triArrSize;

    public int getLODLevel(float scaledDist)
    {
        int level = (int)Mathf.Min(1/Mathf.Pow(scaledDist+distOffset, LODPower)*LODFactor, LODLevels);
        return level;
    }

    private void Start()
    {
        destroyChunks();
        make();
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
                gO.GetComponent<Chunk>().dir = dir;
                gO.GetComponent<Chunk>().sphereGenerator = this;
                gO.GetComponent<Chunk>().make();
                gO.GetComponent<Chunk>().LODLevel = 0;
                gO.layer = 6;
                gO.transform.position = transform.position;
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