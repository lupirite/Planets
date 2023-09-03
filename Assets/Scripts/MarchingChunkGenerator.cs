using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingChunkGenerator : MonoBehaviour
{
    public VectorD2 chunkOffset;

    private List<Vector3> verts;
    List<int> tris;
    public void Generate()
    {
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();

        verts = new List<Vector3>();
        tris = new List<int>();

        //calculateMesh();

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

    private void calculateMesh()
    {
        /*
        VectorD3 pos = (Chunk.mapCube((double)(chunkOffset.x * (chunkRes - 1)) + (double)x * chunkScale, (double)(chunkOffset.y * (chunkRes - 1)) + (double)y * chunkScale).normalized() / 2 * sphereGenerator.diameter);

        Vector3 samplePos = (Vector3)(pos + (VectorD3)sphereGenerator.noiseOffset);
        samplePos += new Vector3(simplex3D(samplePos + new Vector3(1000, 200, 50) * .45f) / 500, simplex3D(samplePos + new Vector3(-1000, 200, 50) * .45f) / 500, simplex3D(samplePos + new Vector3(1000, -200, 50) * .45f) / 500) + new Vector3(simplex3D(samplePos + new Vector3(1000, 200, 50) * 400f) / 1000, simplex3D(samplePos + new Vector3(-100, 200, 50) * 400f) / 1000, simplex3D(samplePos + new Vector3(1000, -2000, 50) * 400f) / 1000);
        float height = 0;
        height += simplex3D(samplePos * .05f) / 100f;
        float val = simplex3D(samplePos * .2f);
        samplePos += new Vector3(300, 200);
        height += val / 1000;
        humidity = Mathf.Pow(val - .5f, 2) * 2f;
        humidity += Mathf.Pow((.6f - Mathf.Abs(simplex3D(samplePos * .1f))) * 2f, 2);
        samplePos += new Vector3(1000, -200);
        height += simplex3D(samplePos * .4f) / 2000f;
        samplePos += new Vector3(2000, -2300);
        height += Mathf.Pow((.6f - Mathf.Abs(simplex3D(samplePos * .15f))) * 2f, 3) * 2f / 700f;

        alt = (height - sphereGenerator.minAlt) / (sphereGenerator.maxAlt - sphereGenerator.minAlt);

        temp = Mathf.Pow(1 - (Mathf.Abs(Vector3.Dot(samplePos.normalized, sphereGenerator.transform.up)) + .01f) * .9f, 1.5f);

        bool water = false;
        if (height < .0065)
        {
            if (height < .006)
            {
                water = true;
                height = .0035f;
            }
            else
            {
                height = .0035f + (height - .006f) * 6;
            }
        }

        if (!water)
        {
            samplePos += new Vector3(100, -2000);
            height += Mathf.Pow((.5f - Mathf.Abs(simplex3D(samplePos * 20f))) * 2f, 3) / 4000f;
            humidity += Mathf.Pow((.5f - Mathf.Abs(simplex3D(samplePos * 500f))) * 2f, 3) / 2f;
            humidity += Mathf.Pow((.5f - Mathf.Abs(simplex3D(samplePos * 2000f))) * 2f, 3) / 4f;
            //height += simplex3D(pos * 1000) / 200000;
            //height += simplex3D(pos * 2000) / 400000;
        }
        pos = (VectorD3)(sphereGenerator.transform.rotation * (pos * (1 + height)));
        if (recenter && x == 0 && y == 0)
        {
            offset = pos;
        }

        verts[i] = pos - offset;
        */
    }

    private void Start()
    {
        Generate();
    }
}
