using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderReaderModule : MonoBehaviour {

    [System.Serializable]
    public struct BoundaryPoints {
        public Vector3 centrePoint;
        public List<Vector3> outerPoints;
        public GameObject aiCover;
    }

    public List<BoundaryPoints> boundPoints;
    public Vector3[] multiplier;
    public float aiRadius;
    public Collider testColl;

    void Start() {
        boundPoints = new List<BoundaryPoints>();
        SelectCollliders();
    }

    void Update() {
        foreach (BoundaryPoints boundPoint in boundPoints) {
            Debug.DrawLine(transform.position, boundPoint.centrePoint, Color.black);
        }

    }

    void SelectCollliders() {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacles");

        foreach (GameObject obstacle in obstacles) {
            if (obstacle.activeInHierarchy) {
                Collider tempColl = obstacle.GetComponent<Collider>();
                for (var i = 0; i < multiplier.Length; i++) {
                    Collider[] check = Physics.OverlapBox(tempColl.bounds.center + new Vector3(tempColl.bounds.extents.x * multiplier[i].x, tempColl.bounds.extents.y * multiplier[i].y, tempColl.bounds.extents.z * multiplier[i].z), new Vector3(aiRadius, aiRadius, aiRadius));
                    if (check.Length <= 1) {
                        boundPoints.Add(new BoundaryPoints());
                        ReadColliders(tempColl, i);
                        break;
                    }
                }
            }
        }

        foreach (GameObject obstacle in obstacles)
            obstacle.SetActive(true);
    }

    void ReadColliders(Collider targetColl, int orientIndex) {
        Vector3 prevVector = Vector3.zero;
        Vector3 nextVector = Vector3.zero;
        Vector3 modifiedExtents = Vector3.zero;
        RaycastHit hit;
        Collider originalColl;
        int infiniteLoopCheck = 0;

        List<GameObject> collided = new List<GameObject>();
        BoundaryPoints tempBound = new BoundaryPoints();
        tempBound.outerPoints = new List<Vector3>();

        modifiedExtents = new Vector3(targetColl.bounds.extents.x * multiplier[orientIndex].x, targetColl.bounds.extents.y * multiplier[orientIndex].y, targetColl.bounds.extents.z * multiplier[orientIndex].z);

        prevVector = targetColl.bounds.center + modifiedExtents;
        prevVector.y = targetColl.bounds.center.y;

        originalColl = targetColl;
        collided.Add(targetColl.gameObject);
        targetColl.gameObject.layer = 2;

        do {
            orientIndex = AddIndex(orientIndex, 1, multiplier.Length);
            infiniteLoopCheck++;

            modifiedExtents = new Vector3(targetColl.bounds.extents.x * multiplier[orientIndex].x, targetColl.bounds.extents.y * multiplier[orientIndex].y, targetColl.bounds.extents.z * multiplier[orientIndex].z);
            nextVector = targetColl.bounds.center + modifiedExtents;
            nextVector.y = targetColl.bounds.center.y;

            if (Physics.Linecast(prevVector, nextVector, out hit)) {
                infiniteLoopCheck = 0;
                nextVector = hit.point;
                targetColl.gameObject.layer = 0;
                targetColl = hit.collider;
                targetColl.gameObject.layer = 2;

                orientIndex = AddIndex(orientIndex, -1, multiplier.Length);
                orientIndex = AddIndex(orientIndex, -1, multiplier.Length);
                collided.Add(targetColl.gameObject);
            }

            Debug.DrawLine(prevVector, nextVector, Color.red, Mathf.Infinity);
            tempBound.outerPoints.Add(nextVector);
            prevVector = nextVector;

            if (infiniteLoopCheck > 4) {
                Debug.LogError("Collider causing infinite loop! Please contact Gab and inform him about this gameobject/collider: " + targetColl.name);
                break;
            }
        } while (targetColl != originalColl || orientIndex != 0);

        Vector3 compiledLocation = Vector3.zero;
        for (var i = 0; i < collided.Count; i++) {
            compiledLocation += collided[i].transform.position;
            collided[i].SetActive(false);
        }

        tempBound.centrePoint = compiledLocation / collided.Count;
        Debug.DrawRay(tempBound.centrePoint, new Vector3(0, 10, 0), Color.green, Mathf.Infinity);
        boundPoints[boundPoints.Count - 1] = tempBound;
    }

    int AddIndex(int currentValue, int value, int arrayCount) {
        if (currentValue + value < 0)
            return arrayCount - 1;
        if (currentValue + value >= arrayCount)
            return 0;

        return currentValue + value;
    }
}
