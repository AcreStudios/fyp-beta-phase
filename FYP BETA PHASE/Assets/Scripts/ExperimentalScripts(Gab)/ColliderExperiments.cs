using UnityEngine;
using System.Collections;

public class ColliderExperiments : MonoBehaviour {

    [System.Serializable]
    public struct BoundaryPoints {
        public Vector3[] centrePoints;
        public Vector3[] outerPoints;
    }

    public BoundaryPoints testPoints;
    public Collider[] testColl;

    void Start() {
        
        //testMesh = testColl.sharedMesh;
    }

    void Update() {
    }
}
