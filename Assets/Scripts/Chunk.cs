using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Xml.XPath;
using UnityEditor.Animations;

public class Chunk : MonoBehaviour
{
    public VectorD2 chunkOffset = VectorD2.zero;
    public SphereGenerator sphereGenerator;
    public Vector3Int dir;
    public int LODLevel;
    public float chunkScale = 1;
    public Vector3 center;

    public Vector3 normal;

    public Chunk[] adjacentChunks = new Chunk[4];// (1, 0), (-1, 0), (0, 1), (0, -1)

    [HideInInspector] public VectorD3 offset;

    Vector3Int xAxis;
    Vector3Int yAxis;

    GameObject[] fullChunks = new GameObject[4];
    public GameObject parentChunk;

    public BinaryInt xCoord;
    public BinaryInt yCoord;

    public string[] adjPoss = new string[4];

    [HideInInspector] public Transform rootChunk;
    [HideInInspector]public int ID;

    private Vector3[] originalVerts;

    public void chRefresh()
    {
        if (transform.childCount == 0)
        {
            refreshSeams();
        }
        else
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Chunk>().chRefresh();
            }
        }
    }

    int snapTo(float i, int interval)
    {
        return ((int)Math.Round(i / (float)interval)) * interval;
    }

    float nfmod(float a, float b)
    {
        return a - b * Mathf.Floor(a / b);
    }

    private void removeSeam(int side, int LODDif)
    {
        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        Vector2Int dir = dirs[side];

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] verts = mesh.vertices;
        int res = chunkRes;

        bool[] xVals = xCoord.getVals();
        bool[] yVals = yCoord.getVals();
        int xOffset = 0;
        int yOffset = 0;
        for (int j = 0; j < Mathf.Min(xVals.Length - 1, 3); j++)
        {
            if (xVals[xVals.Length - 1 - j])
                xOffset += (int)Mathf.Pow(2, j) * (res - 1);
            if (yVals[yVals.Length - 1 - j])
                yOffset += (int)Mathf.Pow(2, j) * (res - 1);
        }
        for (int i = 0; i < res; i++)
        {
            Vector2Int vertPos = new Vector2Int(Mathf.Abs(dir.y)*i+(int)nfmod(-Mathf.Max(dir.x, 0), res), (Mathf.Abs(dir.x)*i+(int)nfmod(-Mathf.Max(dir.y, 0), res)));
            Vector2Int snapVertPos = new Vector2Int(Mathf.Abs(dir.y) * (snapTo((float)i + .001f*dir.y + (float)xOffset, (int)Mathf.Pow(2, LODDif)) - xOffset) + (int)nfmod(-Mathf.Max(dir.x, 0), res), Mathf.Abs(dir.x) * (snapTo((float)i + .001f * dir.x + (float)yOffset, (int)Mathf.Pow(2, LODDif)) - yOffset) + (int)nfmod(-Mathf.Max(dir.y, 0), res));
            
            if (snapVertPos.x >= res || snapVertPos.y >= res || snapVertPos.x < 0 || snapVertPos.y < 0) 
            {
                verts[vertPos.x * res + vertPos.y] = (Vector3)getVertPos(snapVertPos.x, snapVertPos.y);
            }
            else
            {
                verts[vertPos.x * res + vertPos.y] = originalVerts[snapVertPos.x * res + snapVertPos.y];
            }
        }
        mesh.vertices = verts;
        mesh.normals = generateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    Vector3[] generateNormals()
    {
        Vector3[] normals = new Vector3[numVerts];
        for (int x = 0; x < chunkRes; x++)
        {
            for (int y = 0; y < chunkRes; y++)
            {
                Vector3 curPos = (Vector3)verts[(x * chunkRes) + y];
                Vector3 posA = getVert(x+1, y);
                Vector3 posB = getVert(x, y+1);

                Vector3 dirA = posA - curPos;
                Vector3 dirB = posB - curPos;

                Vector3 pVec = -Vector3.Cross(dirA, dirB);

                normals[x * chunkRes + y] = (pVec * 100).normalized;
            }
        }
        
        return normals;
    }

    Vector3 getVert(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < chunkRes && y < chunkRes) {
            return (Vector3)verts[(x * chunkRes) + y];
        }
        VectorD3 vert = getVertPos(x, y);
        if (mustRecenter)
        {
            vert = vert * (double)ScaleManager.instance.celestialScaleFactor;
        }
        return (Vector3)vert;
    }

    public void refreshSeams()
    {
        if (transform.childCount == 0 && !fullChunks[0] && rootChunk && LODLevel != sphereGenerator.LODLevels-1)
        {
            getAdjacentChunks();

            List<int> indices = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (adjacentChunks[i] == null)
                {
                    continue;
                }
                else
                {
                    if (indices.Count == 0)
                    {
                        indices.Add(i);
                    }
                    else
                    {
                        int ind = 0;
                        for (int j = 0; j < indices.Count; j++)
                        {
                            if (adjacentChunks[indices[j]].LODLevel < adjacentChunks[i].LODLevel)
                            {
                                ind++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        indices.Insert(ind, i);
                    }
                }
            }

            for (int i = 0; i < indices.Count; i++)
            {
                removeSeam(indices[i], Mathf.Max(LODLevel - adjacentChunks[indices[i]].LODLevel, 0));
            }
        }
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
                            BinaryInt xc = xCoord.Concat(x == 1);
                            BinaryInt yc = yCoord.Concat(y == 1);
                            GameObject gO = new GameObject("UnscaledChunk(" + (xc).ToString() + ", " + (yc).ToString() + ")");
                            Chunk chunk = gO.AddComponent<Chunk>();
                            chunk.rootChunk = rootChunk;
                            chunk.xCoord = xc;
                            chunk.yCoord = yc;
                            chunk.parentChunk = gameObject;
                            chunk.dir = dir;
                            chunk.sphereGenerator = sphereGenerator;
                            chunk.LODLevel = LODLevel + 1;
                            float nChunkScale = 1 / Mathf.Pow(2, LODLevel + 1);
                            chunk.chunkScale = nChunkScale * ((float)sphereGenerator.chunkRes / (float)sphereGenerator.chunkRes);
                            chunk.chunkOffset = chunkOffset + new VectorD2(x, y) * nChunkScale;
                            gO.transform.position = (Vector3)(sphereGenerator.doublePos * (double)ScaleManager.instance.celestialScaleFactor);
                            //gO.transform.localScale *= ScaleManager.instance.celestialScaleFactor;
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
                            BinaryInt xc = xCoord.Concat(x == 1);
                            BinaryInt yc = yCoord.Concat(y == 1);
                            GameObject gO = new GameObject("Chunk(" + (xc).ToString() + ", " + (yc).ToString() + ")");
                            gO.transform.parent = transform;
                            Chunk chunk = gO.AddComponent<Chunk>();
                            chunk.rootChunk = rootChunk;
                            chunk.xCoord = xc;
                            chunk.yCoord = yc;
                            chunk.parentChunk = gameObject;
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
            else if (aLODLevel <= LODLevel && (fullChunks[0] || (transform.childCount > 0 && !(LODLevel >= sphereGenerator.LODLevels - 2))))
            {
                destroyChunks();
                scheduleGeneration();
            }
        }
    }

    int chunkRes;
    int numVerts;
    int numTris;
    bool mustRecenter;
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

        scheduleGeneration(recenter);
    }

    public void scheduleGeneration(bool recenter=false)
    {
        mustRecenter = recenter;

        generate();

        /*
        ScheduledChunkGeneration scheduledChunk = new ScheduledChunkGeneration();
        scheduledChunk.chunk = this;
        scheduledChunk.recenter = recenter;
        sphereGenerator.scheduledChunks.Add(scheduledChunk);*/
    }

    /*
    double simplex3D(VectorD3 pos)
    {
        return (DoubleNoise.PerlinNoise(pos.x+830d, pos.y-800d) + DoubleNoise.PerlinNoise(pos.x+395d, pos.z+2354d) + DoubleNoise.PerlinNoise(pos.y-2345d, pos.z-5778d)) / 3;
    }
    */

    void getAdjacentChunks()
    {
        Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        for (int i = 0; i < 4; i++)
        {
            int x = dirs[i].x; int y = dirs[i].y;
            BinaryInt xPos = xCoord + BinaryInt.one * x;
            BinaryInt yPos = yCoord + BinaryInt.one * y;

            adjPoss[i] = xPos.ToString()+", "+yPos.ToString();
            if (xPos.ToString().Length != xCoord.ToString().Length || yPos.ToString().Length != yCoord.ToString().Length)
            {
                return;
            }
            adjacentChunks[i] = getChunkAtPos(xPos, yPos);
        }
    }

    Chunk getChunkAtPos(BinaryInt x, BinaryInt y)
    {
        Transform curChunk = rootChunk;
        int i = 0;
        while (true)
        {
            bool[] xVals = x.getVals();
            bool[] yVals = y.getVals();
            if (curChunk.childCount == 0 || i == xVals.Length)
            {
                break;
            }
            int ind = 0;
            if (xVals[i]) ind += 2;
            if (yVals[i]) ind += 1;

            curChunk = curChunk.GetChild(ind);
            i++;
        }
        return curChunk.GetComponent<Chunk>();
    }

    VectorD3[] verts;
    Vector2[] uvs;
    Vector2[] uv2;
    int[] tris;
    Vector3[] vector3Array;

    private void Start()
    {
        ID = (int)(chunkOffset.x * 100) + (int)(chunkOffset.y * 100);

        if (sphereGenerator.correctSeams)
        {
            getAdjacentChunks();
            foreach (var chunk in adjacentChunks)
            {
                if (chunk == null)
                {
                    continue;
                }
                chunk.chRefresh();
            }
            refreshSeams();
        }
    }

    void applyMesh()
    {
        if (GetComponent<MeshRenderer>())
        {
            return;
        }
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        mesh.vertices = vector3Array;
        originalVerts = mesh.vertices;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.uv2 = uv2;
        mesh.normals = generateNormals();
        mesh.RecalculateBounds();

        center = mesh.bounds.center;

        normal = Quaternion.Inverse(sphereGenerator.transform.rotation) * (center + (Vector3)offset).normalized;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = sphereGenerator.material;
    }
    
    void generate()
    {
        verts = new VectorD3[numVerts];
        uvs = new Vector2[numVerts];
        uv2 = new Vector2[numVerts];
        tris = new int[numTris];

        int i = 0;

        for (int x = 0; x < chunkRes; x++)
        {
            for (int y = 0; y < chunkRes; y++)
            {
                double alt = 0.75d;
                double temp = 0.5d;
                double humidity = 0.5d;
                VectorD3 pos = (mapCube((double)(chunkOffset.x*(chunkRes-1)) + (double)x * chunkScale, (double)(chunkOffset.y*(chunkRes-1)) + (double)y * chunkScale, chunkRes, xAxis, yAxis, dir).normalized() / 2 * sphereGenerator.diameter);

                VectorD3 samplePos = (pos + (VectorD3)sphereGenerator.noiseOffset)/4;
                samplePos += new VectorD3(SphereGenerator.simplex3D(samplePos + new VectorD3(1000, 200, 50) * .45d)/500, SphereGenerator.simplex3D(samplePos + new VectorD3(-1000, 200, 50) * .45d)/500, SphereGenerator.simplex3D(samplePos + new VectorD3(1000, -200, 50) * .45d)/500) + new VectorD3(SphereGenerator.simplex3D(samplePos + new VectorD3(1000, 200, 50) * 400d)/1000, SphereGenerator.simplex3D(samplePos + new VectorD3(-100, 200, 50) * 400d)/1000, SphereGenerator.simplex3D(samplePos + new VectorD3(1000, -2000, 50) * 400d)/1000);
                
                double height = SphereGenerator.getHeight(samplePos);
                humidity = SphereGenerator.getHumidity(samplePos);
                temp = Math.Pow(1 - (Math.Abs(pos.normalized().Dot((VectorD3)sphereGenerator.transform.up)) + .01d) * .9d, 1.5d)+SphereGenerator.simplex3D(pos*.1)/10;
                alt = (height - sphereGenerator.minAlt) / (sphereGenerator.maxAlt - sphereGenerator.minAlt);

                pos = (VectorD3)(sphereGenerator.transform.rotation * (pos * (1 + height)));
                if (mustRecenter && x == 0 && y == 0)
                {
                    offset = pos;
                }

                verts[i] = getVertPos(x, y);
                if (mustRecenter)
                {
                    verts[i] = verts[i] * (double)ScaleManager.instance.celestialScaleFactor;
                }

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

        vector3Array = new Vector3[verts.Length];

        for (int g = 0; g < verts.Length; g++)
        {
            vector3Array[g] = new Vector3((float)verts[g].x, (float)verts[g].y, (float)verts[g].z);
        }

        applyMesh();
    }

    public VectorD3 getVertPos(int x, int y)
    {
        VectorD3 pos = (mapCube((double)(chunkOffset.x * (chunkRes - 1)) + (double)x * chunkScale, (double)(chunkOffset.y * (chunkRes - 1)) + (double)y * chunkScale, chunkRes, xAxis, yAxis, dir).normalized() / 2 * sphereGenerator.diameter);

        VectorD3 samplePos = (pos + (VectorD3)sphereGenerator.noiseOffset) / 4;
        samplePos += new VectorD3(SphereGenerator.simplex3D(samplePos + new VectorD3(1000, 200, 50) * .45d) / 500, SphereGenerator.simplex3D(samplePos + new VectorD3(-1000, 200, 50) * .45d) / 500, SphereGenerator.simplex3D(samplePos + new VectorD3(1000, -200, 50) * .45d) / 500) + new VectorD3(SphereGenerator.simplex3D(samplePos + new VectorD3(1000, 200, 50) * 400d) / 1000, SphereGenerator.simplex3D(samplePos + new VectorD3(-100, 200, 50) * 400d) / 1000, SphereGenerator.simplex3D(samplePos + new VectorD3(1000, -2000, 50) * 400d) / 1000);

        double height = SphereGenerator.getHeight(samplePos);

        pos = (VectorD3)(sphereGenerator.transform.rotation * (pos * (1 + height)));

        return (pos - offset);
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
        return 2/(2-x)-1;
    }

    public static double mapSide(double x, int chunkRes)
    {
        return (x / (float)(chunkRes - 1) - .5f);
    }
}
