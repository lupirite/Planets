using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullChunk : MonoBehaviour
{
    public GameObject treePrefab;
    public int numTrees = 5;
    public LayerMask groundMask;
    public float treeSpread;

    private int ID;
    private SphereGenerator sphereGenerator;

    [HideInInspector]public GameObject marchingChunk;
    private void Start()
    {
        sphereGenerator = GetComponent<Chunk>().sphereGenerator;
        for (int i = 0; i < numTrees; i++)
        {
            Vector3 up = GetComponent<Chunk>().sphereGenerator.transform.rotation*GetComponent<Chunk>().normal;
            Vector3 right = Vector3.Cross(up, transform.up);
            Vector3 forwards = -Vector3.Cross(up, right);
            Vector3 randomSpawnPos = transform.position + -Random.Range(0, treeSpread)*right + Random.Range(0, treeSpread) * forwards + up * 20;

            RaycastHit hit;
            if (Physics.Raycast(randomSpawnPos, -up, out hit, 50, groundMask))
            {
                GameObject tree = Instantiate(treePrefab, hit.point, Quaternion.identity);
                tree.transform.parent = transform;
                tree.transform.forward = up;
                //tree.transform.eulerAngles = new Vector3(tree.transform.eulerAngles.x, tree.transform.eulerAngles.y, Random.Range(0, 360));
                tree.transform.localScale *= Random.Range(.8f, 1.4f);
            }
        }

        ID = GetComponent<Chunk>().ID;
    }

    int frame = 0;
    private void Update()
    {
        frame++;
        frame = frame % sphereGenerator.resizeCheckFrameInterval;
        if ((ID + frame) % sphereGenerator.resizeCheckFrameInterval == 0)
        {
            float dist = Vector3.Distance(transform.position+(Vector3)GetComponent<Chunk>().center, sphereGenerator.viewer.transform.position);
            if (dist < sphereGenerator.renderDistance)
            {
                if (!marchingChunk)
                {
                    GetComponent<MeshRenderer>().enabled = false;
                    marchingChunk = new GameObject("MarchingChunk");
                    marchingChunk.transform.parent = transform;
                    marchingChunk.AddComponent<MarchingChunkGenerator>();

                    MarchingChunkGenerator cg = marchingChunk.GetComponent<MarchingChunkGenerator>();
                    cg.chunkOffset = GetComponent<Chunk>().chunkOffset;
                }
            }
            else
            {
                if (marchingChunk)
                {
                    GetComponent<MeshRenderer>().enabled = true;
                    Destroy(marchingChunk);
                }
            }
        }
    }

    /*
    private void OnDrawGizmos()
    {
        Vector3 up = GetComponent<Chunk>().sphereGenerator.transform.rotation * GetComponent<Chunk>().normal;
        Vector3 right = Vector3.Cross(up, transform.up);
        Vector3 forwards = -Vector3.Cross(up, right);
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, up);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, right);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, forwards);
    }*/
}
