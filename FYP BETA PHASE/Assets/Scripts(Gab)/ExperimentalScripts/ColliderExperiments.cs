using UnityEngine;
using System.Collections;

public class ColliderExperiments : MonoBehaviour {

    public Collider testColl;
    public MeshFilter mesh;
    public Mesh testMesh;

    void Start() {
        //testMesh = testColl.sharedMesh;
    }

    void Update() {
        Debug.DrawLine(transform.position, testColl.ClosestPointOnBounds(transform.position), Color.red);
    }
}
