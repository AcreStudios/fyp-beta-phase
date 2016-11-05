using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderExperiments : MonoBehaviour {

    [System.Serializable]
    public struct BoundaryPoints {
        public Vector3 centrePoint;
        public List<Vector3> outerPoints;
    }

    public BoundaryPoints testPoints;
    public Collider testColl;
    public Vector3[] multiplier;
    public int orientIndex;

    /*Vector3 prevVector = Vector3.zero;
    Vector3 nextVector = Vector3.zero;

    Vector3 modifiedExtents = Vector3.zero;
    RaycastHit hit;
    Collider originalColl;
    float timer;
    bool first;*/

            

    void Start() {
        ReadColliders();
    }

    void Update() {
        /*if (testColl != originalColl || orientIndex != 0 && Time.time > timer) {
            //first = false;
            orientIndex = AddIndex(orientIndex, 1, multiplier.Length);
            modifiedExtents = new Vector3(testColl.bounds.extents.x * multiplier[orientIndex].x, testColl.bounds.extents.y * multiplier[orientIndex].y, testColl.bounds.extents.z * multiplier[orientIndex].z);
            nextVector = testColl.bounds.center + modifiedExtents;
            nextVector.y = testColl.bounds.center.y;

            testColl.gameObject.layer = 2;

            if (Physics.Linecast(prevVector, nextVector, out hit)) {
                    testColl.gameObject.layer = 0;
                    testColl = hit.collider;
                    nextVector = hit.point;
                    orientIndex = AddIndex(orientIndex, -1, multiplier.Length);
                    orientIndex = AddIndex(orientIndex, -1, multiplier.Length);
            }

            Debug.DrawLine(prevVector, nextVector, Color.red,60);
            Debug.Log("Working");
            testPoints.outerPoints.Add(nextVector);

            prevVector = nextVector;
            
            timer = Time.time + 1;
        }*/
    }

    void ReadColliders() {
        Vector3 prevVector = Vector3.zero;
        Vector3 nextVector = Vector3.zero;        
        Vector3 modifiedExtents = Vector3.zero;
        RaycastHit hit;
        Collider originalColl;

        orientIndex = 0;

        testPoints.outerPoints = new List<Vector3>();
        modifiedExtents = new Vector3(testColl.bounds.extents.x * multiplier[orientIndex].x, testColl.bounds.extents.y * multiplier[orientIndex].y, testColl.bounds.extents.z * multiplier[orientIndex].z);
        
        prevVector = testColl.bounds.center + modifiedExtents;
        prevVector.y = testColl.bounds.center.y;

        originalColl = testColl;
        testColl.gameObject.layer = 2;

        do {
            orientIndex = AddIndex(orientIndex, 1, multiplier.Length);
            modifiedExtents = new Vector3(testColl.bounds.extents.x * multiplier[orientIndex].x, testColl.bounds.extents.y * multiplier[orientIndex].y, testColl.bounds.extents.z * multiplier[orientIndex].z);
            nextVector = testColl.bounds.center + modifiedExtents;
            nextVector.y = testColl.bounds.center.y;

            if (Physics.Linecast(prevVector, nextVector, out hit)) {
                testColl.gameObject.layer = 0;
                testColl = hit.collider;
                testColl.gameObject.layer = 2;
                nextVector = hit.point;
                orientIndex = AddIndex(orientIndex, -1, multiplier.Length);
                orientIndex = AddIndex(orientIndex, -1, multiplier.Length);
            }

            Debug.DrawLine(prevVector, nextVector, Color.red, Mathf.Infinity);
            testPoints.outerPoints.Add(nextVector);

            prevVector = nextVector;
        } while (testColl != originalColl || orientIndex != 0);
    }

    int AddIndex(int currentValue, int value, int arrayCount) {
        if (currentValue + value < 0)
            return arrayCount - 1;
        if (currentValue + value >= arrayCount)
            return 0;

        return currentValue + value;
    }
}
