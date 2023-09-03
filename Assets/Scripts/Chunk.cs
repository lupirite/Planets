using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;

public class Chunk : MonoBehaviour
{
    public VectorD2 chunkOffset = VectorD2.zero;
    public SphereGenerator sphereGenerator;
    public Vector3Int dir;
    public int LODLevel;
    public float chunkScale = 1;
    public Vector3 center;

    public Vector3 normal;

    [HideInInspector] public VectorD3 offset;

    Vector3Int xAxis;
    Vector3Int yAxis;

    GameObject[] fullChunks = new GameObject[4];

    [HideInInspector]public int ID;
    private void Start()
    {
        ID = (int)(chunkOffset.x*100) + (int)(chunkOffset.y*100);
    }

    int frame = 0;
    private void Update()
    {
        frame++;
        frame = frame % sphereGenerator.resizeCheckFrameInterval;
        if ((ID+frame) % sphereGenerator.resizeCheckFrameInterval == 0)
        {
            int aLODLevel = sphereGenerator.getLODLevel(transform.position + (Vector3)center, normal, LODLevel);
            if (aLODLevel > LODLevel && (transform.childCount == 0 && LODLevel != sphereGenerator.LODLevels - 2 || !fullChunks[0] && LODLevel == sphereGenerator.LODLevels - 2))
            {
                Destroy(transform.GetComponent<MeshRenderer>());
                Destroy(transform.GetComponent<MeshFilter>());

                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        if (LODLevel == sphereGenerator.LODLevels - 2)
                        {
                            GameObject gO = new GameObject("UnscaledChunk(" + x.ToString() + ", " + y.ToString() + ")");
                            Chunk chunk = gO.AddComponent<Chunk>();
                            chunk.dir = dir;
                            chunk.sphereGenerator = sphereGenerator;
                            chunk.LODLevel = LODLevel + 1;
                            float nChunkScale = 1 / Mathf.Pow(2, LODLevel + 1);
                            //print(((float)sphereGenerator.chunkRes / (float)sphereGenerator.fullScaleRes));
                            chunk.chunkScale = nChunkScale * ((float)sphereGenerator.chunkRes / (float)sphereGenerator.chunkRes);
                            chunk.chunkOffset = chunkOffset + new VectorD2(x, y) * nChunkScale;
                            gO.transform.position = Vector3Int.FloorToInt(transform.root.position * ScaleManager.instance.celestialScaleFactor);
                            gO.transform.localScale *= ScaleManager.instance.celestialScaleFactor;
                            fullChunks[x * 2 + y] = gO;
                            chunk.make(true);
                            gO.transform.position = (Vector3)((VectorD3)gO.transform.position + chunk.offset * ScaleManager.instance.celestialScaleFactor);
                            gO.AddComponent<MeshCollider>().sharedMesh = gO.GetComponent<MeshFilter>().mesh;

                            GameObject scaleChunks;
                            if (!GameObject.Find("ScaleChunks"))
                            {
                                scaleChunks = new GameObject("ScaleChunks");
                                //scaleChunks.transform.position = transform.root.position * ScaleManager.instance.celestialScaleFactor;
                            }
                            else
                            {
                                scaleChunks = GameObject.Find("ScaleChunks");
                            }

                            gO.transform.parent = scaleChunks.transform;

                            gO.AddComponent<FullChunk>();
                            gO.GetComponent<FullChunk>().groundMask = sphereGenerator.groundMask;
                            gO.GetComponent<FullChunk>().numTrees = sphereGenerator.numTrees;
                            gO.GetComponent<FullChunk>().treePrefab = sphereGenerator.treePrefab;
                            gO.GetComponent<FullChunk>().treeSpread = sphereGenerator.treeSpread;
                            //transform.localPosition = Vector3.zero;
                        }
                        else
                        {
                            GameObject gO = new GameObject("Chunk(" + x.ToString() + ", " + y.ToString() + ")");
                            gO.transform.parent = transform;
                            Chunk chunk = gO.AddComponent<Chunk>();
                            chunk.dir = dir;
                            chunk.sphereGenerator = sphereGenerator;
                            chunk.LODLevel = LODLevel + 1;
                            float nChunkScale = 1 / Mathf.Pow(2, LODLevel + 1);
                            chunk.chunkScale = nChunkScale;
                            chunk.chunkOffset = chunkOffset + new VectorD2(x, y) * nChunkScale;
                            gO.transform.position = transform.position;
                            gO.layer = 6;
                            chunk.make();
                        }

                    }
                }
            }
            else if (aLODLevel <= LODLevel && (transform.childCount > 0 || fullChunks[0]))
            {
                destroyChunks();
                generate();
            }
        }
    }

    int chunkRes;
    int numVerts;
    int numTris;
    public void make(bool recenter=false)
    {
        if (Application.isPlaying)
        {
            chunkRes = sphereGenerator.chunkRes;
        }
        else
        {
            chunkRes = sphereGenerator.res;
        }
        numVerts = sphereGenerator.chunkVerts;
        numTris = sphereGenerator.triArrSize;

        xAxis = new Vector3Int(dir[1], dir[2], dir[0]);
        yAxis = new Vector3Int(dir[2], dir[0], dir[1]);

        generate(recenter);
    }
    /*
    double simplex3D(VectorD3 pos)
    {
        return (DoubleNoise.PerlinNoise(pos.x+830d, pos.y-800d) + DoubleNoise.PerlinNoise(pos.x+395d, pos.z+2354d) + DoubleNoise.PerlinNoise(pos.y-2345d, pos.z-5778d)) / 3;
    }
    */
    
    static double simplex3D(VectorD3 pos)
    {
        return DoubleNoise.doubleNoise.Evaluate(pos.x, pos.y, pos.z);
    }
    void generate(bool recenter = false)
    {
        if (GetComponent<MeshRenderer>())
        {
            return;
        }
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        int i = 0;

        VectorD3[] verts = new VectorD3[numVerts];
        Vector2[] uvs = new Vector2[numVerts];
        Vector2[] uv2 = new Vector2[numVerts];
        int[] tris = new int[numTris];

        for (int x = 0; x < chunkRes; x++)
        {
            for (int y = 0; y < chunkRes; y++)
            {
                double alt = 0.75d;
                double temp = 0.5d;
                double humidity = 0.5d;
                VectorD3 pos = (mapCube((double)(chunkOffset.x*(chunkRes-1)) + (double)x * chunkScale, (double)(chunkOffset.y*(chunkRes-1)) + (double)y * chunkScale, chunkRes, xAxis, yAxis, dir).normalized() / 2 * sphereGenerator.diameter);

                VectorD3 samplePos = (pos + (VectorD3)sphereGenerator.noiseOffset)/4;
                samplePos += new VectorD3(simplex3D(samplePos + new VectorD3(1000, 200, 50) * .45d)/500, simplex3D(samplePos + new VectorD3(-1000, 200, 50) * .45d)/500, simplex3D(samplePos + new VectorD3(1000, -200, 50) * .45d)/500) + new VectorD3(simplex3D(samplePos + new VectorD3(1000, 200, 50) * 400d)/1000, simplex3D(samplePos + new VectorD3(-100, 200, 50) * 400d)/1000, simplex3D(samplePos + new VectorD3(1000, -2000, 50) * 400d)/1000);
                
                double height = getHeight(samplePos);
                humidity = getHumidity(samplePos);
                temp = Math.Pow(1 - (Math.Abs(pos.normalized().Dot((VectorD3)sphereGenerator.transform.up)) + .01d) * .9d, 1.5d)+simplex3D(pos*.1)/10;
                alt = (height - sphereGenerator.minAlt) / (sphereGenerator.maxAlt - sphereGenerator.minAlt);

                pos = (VectorD3)(sphereGenerator.transform.rotation * (pos * (1 + height)));
                if (recenter && x == 0 && y == 0)
                {
                    offset = pos;
                }

                verts[i] = pos - offset;

                uvs[i] = new Vector2((float)alt, (float)temp);
                uv2[i] = new Vector2((float)humidity, 0);
                i++;
            }
        }

        i = 0;
        for (int x = 0; x < chunkRes-1; x++)
        {
            for (int y = 0; y < chunkRes-1; y++)
            {
                int r = 0;
                if (dir[0] < 0 || dir[1] < 0 || dir[2] < 0)
                    r = 1;
                tris[i] = x + chunkRes*y;
                tris[i+1+r] = x + chunkRes*y + 1;
                tris[i+2-r] = x + chunkRes*(1+y);
                tris[i+3] = x + chunkRes*(1+y);
                tris[i+4+r] = x + chunkRes * y + 1;
                tris[i+5-r] = x + chunkRes*(y+1) + 1;
                i += 6;
            }
        }

        Mesh mesh = new Mesh();

        Vector3[] vector3Array = new Vector3[verts.Length];

        for (int g = 0; g < verts.Length; g++)
        {
            vector3Array[g] = new Vector3((float)verts[g].x, (float)verts[g].y, (float)verts[g].z);
        }

        mesh.vertices = vector3Array;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.uv2 = uv2;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = sphereGenerator.material;

        center = mesh.bounds.center;

        normal = Quaternion.Inverse(sphereGenerator.transform.rotation) * (center + (Vector3)offset).normalized;
    }

    public void destroyChunks()
    {
        if (LODLevel == sphereGenerator.LODLevels - 1)
        {
            return;
        }
        while (transform.childCount > 0)
        {
            if (transform.GetChild(0).GetComponent<Chunk>())
                transform.GetChild(0).GetComponent<Chunk>().destroyChunks();
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        if (fullChunks[0] != null)
        {
            for (int i = 0; i < fullChunks.Length; i++)
            {
                Destroy(fullChunks[i]);
                fullChunks[i] = null;
            }
        }
    }

    public static VectorD3 mapCube(double x, double y, int chunkRes, Vector3 xAxis, Vector3 yAxis, Vector3 dir) {
        double hr = (double)chunkRes / 2d;
        double xp = Math.Abs((double)x - hr);
        double yp = Math.Abs((double)y - hr);
        double xs = mapSide(x, chunkRes);
        double ys = mapSide(y, chunkRes);
        return (VectorD3)xAxis * adjustVerts(xs*2)/2d + (VectorD3)yAxis * adjustVerts(ys*2)/2d + (VectorD3)dir / 2d;
    }

    public static double adjustVerts(double x)
    {
        return sideMapFunc(Math.Abs(x))*Math.Sign(x);
    }

    public static double sideMapFunc(double x)
    {
        return Math.Pow(2/(2-x)-1, .8f);
    }

    public static double mapSide(double x, int chunkRes)
    {
        return (x / (float)(chunkRes - 1) - .5f);
    }

    public static double getHeight(VectorD3 pos)
    {
        double h = 0;
        // distant
        h += simplex3D(pos * .02d) / 50d;
        h += simplex3D(pos * .1d) / 100d;
        h += simplex3D(pos * 50d) / 10000d;

        h -= .001f;
        if (h < 0)
        {
            h = 0;
        }

        // surface
        h += simplex3D(pos * 500d) / 500000d;
        return h;
    }

    public static double getHumidity(VectorD3 pos)
    {
        return simplex3D(pos * 1000d) * .5d + simplex3D(pos * 10000d)*.1d;
    }
}
