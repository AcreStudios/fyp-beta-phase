using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderExperiments : MonoBehaviour {

    [System.Serializable]
    public struct BoundaryPoints {
        public Vector3[] centrePoints;
        public List<Vector3> outerPoints;
    }

    public BoundaryPoints testPoints;
    public Collider testColl;
    public Vector3[] multiplier;
    public int orientIndex;


    void Start() {
        orientIndex = 0;

        testPoints.outerPoints = new List<Vector3>();
        //ReadColliders(testColl,testColl)
        //testMesh = testColl.sharedMesh;
        Vector3 prevVector = Vector3.zero;
        Vector3 nextVector = Vector3.zero;

        Vector3 modifiedExtents = Vector3.zero;
        RaycastHit hit;

        modifiedExtents = new Vector3(testColl.bounds.extents.x * multiplier[orientIndex].x, testColl.bounds.extents.y * multiplier[orientIndex].y, testColl.bounds.extents.z * multiplier[orientIndex].z);
        Vector3 oringinalPos = testColl.bounds.center + modifiedExtents;
        prevVector = oringinalPos;

        while (nextVector != oringinalPos) {
            orientIndex = AddIndex(orientIndex, 1, multiplier.Length);
            modifiedExtents = new Vector3(testColl.bounds.extents.x * multiplier[orientIndex].x, testColl.bounds.extents.y * multiplier[orientIndex].y, testColl.bounds.extents.z * multiplier[orientIndex].z);
            nextVector = testColl.bounds.center + modifiedExtents;

            if (Physics.Linecast(prevVector, nextVector, out hit)) {
                if (hit.collider != testColl) {
                    testColl = hit.collider;
                    nextVector = hit.point;
                    orientIndex = AddIndex(orientIndex, -1, multiplier.Length);
                    Debug.Log("Working");
                }
            }
            Debug.DrawLine(prevVector, nextVector, Color.red, 20);
            testPoints.outerPoints.Add(nextVector);
            prevVector = nextVector;

        }
    }

    void Update() {
        //foreach (Vector3 points in testPoints.outerPoints)
        //Debug.DrawLine(transform.position, points, Color.red);
    }

    int AddIndex(int currentValue, int value, int arrayCount) {
        if (currentValue + value < 0)
            return arrayCount - 1;
        if (currentValue + value >= arrayCount)
            return 0;

        return currentValue + value;
    }
}
