using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullChunk : MonoBehaviour
{
    public GameObject treePrefab;
    public int numTrees = 5;
    public LayerMask groundMask;
    public float treeSpread;
    private void Start()
    {
        for (int i = 0; i < numTrees; i++)
        {
            Vector3 up = GetComponent<Chunk>().sphereGenerator.transform.rotation*GetComponent<Chunk>().normal;
            Vector3 right = Vector3.Cross(up, transform.up);
            Vector3 forwards = -Vector3.Cross(up, right);
            Vector3 randomSpawnPos = transform.position + Random.Range(0, treeSpread)*right + Random.Range(0, treeSpread) * forwards + up * 20;

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
