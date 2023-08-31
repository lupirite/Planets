using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Vector2 chunkOffset = Vector2.zero;
    public SphereGenerator sphereGenerator;
    public Vector3Int dir;
    public int LODLevel;
    public float chunkScale = 1;
    public Vector3 center;

    public Vector3 normal;

    [HideInInspector] public Vector3 offset;

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
            int aLODLevel = sphereGenerator.getLODLevel(transform.position + center, normal, LODLevel);
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
                            chunk.chunkOffset = chunkOffset + new Vector2(x, y) * nChunkScale;
                            gO.transform.position = Vector3Int.FloorToInt(transform.root.position * ScaleManager.instance.celestialScaleFactor);
                            gO.transform.localScale *= ScaleManager.instance.celestialScaleFactor;
                            fullChunks[x * 2 + y] = gO;
                            chunk.make(true);
                            gO.transform.position += chunk.offset * ScaleManager.instance.celestialScaleFactor;
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
                            chunk.chunkOffset = chunkOffset + new Vector2(x, y) * nChunkScale;
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

    float perlin3D(Vector3 pos)
    {
        return (Mathf.PerlinNoise(pos.x+830, pos.y-800) + Mathf.PerlinNoise(pos.x+395, pos.z+2354) + Mathf.PerlinNoise(pos.y-2345, pos.z-5778)) / 3;
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

        Vector3[] verts = new Vector3[numVerts];
        Vector2[] uvs = new Vector2[numVerts];
        Vector2[] uv2 = new Vector2[numVerts];
        int[] tris = new int[numTris];

        for (int x = 0; x < chunkRes; x++)
        {
            for (int y = 0; y < chunkRes; y++)
            {
                float alt = 0.5f;
                float temp = 0.5f;
                float humidity = 0.5f;
                Vector3 pos = (mapCube(chunkOffset.x*(chunkRes-1) + (float)x * chunkScale, chunkOffset.y*(chunkRes-1) + (float)y * chunkScale).normalized / 2 * sphereGenerator.diameter);

                Vector3 samplePos = pos + sphereGenerator.noiseOffset;
                samplePos += new Vector3(perlin3D(samplePos + new Vector3(1000, 200, 50) * .45f)/500, perlin3D(samplePos + new Vector3(-1000, 200, 50) * .45f)/500, perlin3D(samplePos + new Vector3(1000, -200, 50) * .45f)/500) + new Vector3(perlin3D(samplePos + new Vector3(1000, 200, 50) * 400f)/1000, perlin3D(samplePos + new Vector3(-100, 200, 50) * 400f)/1000, perlin3D(samplePos + new Vector3(1000, -2000, 50) * 400f)/1000);
                float height = 0;
                height += perlin3D(samplePos * .05f) / 100f;
                float val = perlin3D(samplePos * .2f);
                samplePos += new Vector3(300, 200);
                height += val / 1000;
                humidity = Mathf.Pow(val-.5f, 2)*2f;
                humidity += Mathf.Pow((.6f - Mathf.Abs(perlin3D(samplePos * .1f))) * 2f, 2);
                samplePos += new Vector3(1000, -200);
                height += perlin3D(samplePos * .4f) / 2000f;
                samplePos += new Vector3(2000, -2300);
                height += Mathf.Pow((.6f - Mathf.Abs(perlin3D(samplePos * .15f))) * 2f, 3)*2f / 700f;

                alt = (height - sphereGenerator.minAlt) / (sphereGenerator.maxAlt - sphereGenerator.minAlt);

                temp = Mathf.Pow(1-(Mathf.Abs(Vector3.Dot(samplePos.normalized, sphereGenerator.transform.up))+.01f)*.9f, 1.5f);

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
                    height += Mathf.Pow((.5f - Mathf.Abs(perlin3D(samplePos * 20f))) * 2f, 3) / 4000f;
                    humidity += Mathf.Pow((.5f - Mathf.Abs(perlin3D(samplePos * 500f))) * 2f, 3)/2f;
                    humidity += Mathf.Pow((.5f - Mathf.Abs(perlin3D(samplePos * 2000f))) * 2f, 3) / 4f;
                    //height += perlin3D(pos * 1000) / 200000;
                    //height += perlin3D(pos * 2000) / 400000;
                }
                pos = sphereGenerator.transform.rotation * (pos * (1 + height));
                if (recenter && x == 0 && y == 0)
                {
                    offset = pos;
                }

                verts[i] = pos - offset;

                uvs[i] = new Vector2(alt, temp);
                uv2[i] = new Vector2(humidity, 0);
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
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.uv2 = uv2;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = sphereGenerator.material;

        center = mesh.bounds.center;

        normal = Quaternion.Inverse(sphereGenerator.transform.rotation) * (center + offset).normalized;
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

    Vector3 mapCube(float x, float y) {
        float hr = (float)chunkRes / 2;
        float xp = Mathf.Abs((float)x - hr);
        float yp = Mathf.Abs((float)y - hr);
        float xs = mapSide(x);
        float ys = mapSide(y);
        return (Vector3)xAxis * adjustVerts(xs*2)/2 + (Vector3)yAxis * adjustVerts(ys*2)/2 + (Vector3)dir / 2;
    }

    float adjustVerts(float x)
    {
        return sideMapFunc(Mathf.Abs(x))*Mathf.Sign(x);
    }

    float sideMapFunc(float x)
    {
        return Mathf.Pow(2/(2-x)-1, .8f);
    }

    float mapSide(float x)
    {
        return (x / (float)(chunkRes - 1) - .5f);
    }
}
