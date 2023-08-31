using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingChunkGenerator : MonoBehaviour
{
    public void Generate()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        verts.Add(new Vector3(0, 1, 0));
        verts.Add(new Vector3(1, 0, 0));
        verts.Add(new Vector3(0, 0, 0));

        tris.Add(0);
        tris.Add(1);
        tris.Add(2);

        Vector3[] vs = new Vector3[verts.Count];
        int[] ts = new int[tris.Count];
        verts.CopyTo(vs, 0);
        tris.CopyTo(ts, 0);
        mesh.vertices = vs;
        mesh.triangles = ts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = transform.parent.GetComponent<Chunk>().sphereGenerator.material;
    }

    private void Start()
    {
        Generate();
    }
}
