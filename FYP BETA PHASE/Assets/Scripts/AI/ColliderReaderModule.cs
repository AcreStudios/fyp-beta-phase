using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderReaderModule : MonoBehaviour {

    [System.Serializable]
    public struct BoundaryPoints {
        public Vector3 centrePoint;
        public List<Vector3> outerPoints;
    }

    public BoundaryPoints boundPoints;
    public Collider testColl;
    public Vector3[] multiplier;
    public float aiRadius;

    void Start() {
        SelectCollliders();
    }

    void SelectCollliders() {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacles");

        foreach (GameObject obstacle in obstacles) {
            if (obstacle.activeInHierarchy) {
                Collider tempColl = obstacle.GetComponent<Collider>();
                for (var i = 0; i < multiplier.Length; i++) {
                    Collider[] check = Physics.OverlapBox(tempColl.bounds.center + new Vector3(tempColl.bounds.extents.x * multiplier[i].x, tempColl.bounds.extents.y * multiplier[i].y, tempColl.bounds.extents.z * multiplier[i].z), new Vector3(aiRadius, aiRadius, aiRadius));
                    Debug.Log(check.Length);
                    if (check.Length <= 1) {
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

        boundPoints.outerPoints = new List<Vector3>();
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
            boundPoints.outerPoints.Add(nextVector);
            prevVector = nextVector;

            if (infiniteLoopCheck > 4) {
                Debug.LogError("Collider causing infinite loop! Please contact Gab and inform him about this gameobject/collider: " + targetColl.name);
                break;
            }
        } while (targetColl != originalColl || orientIndex != 0);

        for (var i = 0; i < collided.Count; i++) {
            collided[i].SetActive(false);
        }
    }

    int AddIndex(int currentValue, int value, int arrayCount) {
        if (currentValue + value < 0)
            return arrayCount - 1;
        if (currentValue + value >= arrayCount)
            return 0;

        return currentValue + value;
    }
}
