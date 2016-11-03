using UnityEngine;
using System.Collections;

public class ColliderExperiments : MonoBehaviour {

    public MeshCollider testColl;
    public MeshFilter mesh;
    public Mesh testMesh;

    void Start() {
        testMesh = testColl.sharedMesh;
    }

    void Update() {

        //Debug.DrawLine(transform.position, testColl.ClosestPointOnBounds(transform.position), Color.red);
        //Color tempColor;
        //tempColor = Random.ColorHSV();

        //foreach (Vector3 vertice in testMesh.vertices)
            //Debug.DrawLine(transform.position, testColl.transform.position + new Vector3(vertice.x*testColl.transform.localScale.x, vertice.y * testColl.transform.localScale.y, vertice.z * testColl.transform.localScale.z), tempColor);
    }
}
