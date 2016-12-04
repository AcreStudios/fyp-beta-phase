using UnityEngine;
using System.Collections;

public class AIManager : MonoBehaviour {
    //Stores obstacle data, group data.
    [System.Serializable]
    public struct ObstaclesData {
        public Collider obstacle;
        public GameObject aiCover;
    }

    public static AIManager instance;
    public ColliderReaderModule readerInst;
    public Transform player;
    //public ObstaclesData[] obstacles;

    void Start() {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //readerInst = GameObject.Find("ColliderTester").GetComponent<ColliderReaderModule>();
        //readerInst = ColliderReaderModule.instance;
        /*GameObject[] temp = GameObject.FindGameObjectsWithTag("Obstacles");
        obstacles = new ObstaclesData[temp.Length];

        for (var i = 0; i < obstacles.Length; i++)
            obstacles[i].obstacle = temp[i].GetComponent<Collider>();*/
    }

    /*public Collider AssignCover(GameObject ai, float range) {
        Collider temp = null;
        float dist = Mathf.Infinity;

        int reference = 0;

        for (var i = 0; i < obstacles.Length; i++) {
            if (!obstacles[i].aiCover)
                if ((player.transform.position - obstacles[i].obstacle.transform.position).sqrMagnitude < range * range) {
                    float tempDist = (obstacles[i].obstacle.transform.position - ai.transform.position).sqrMagnitude;

                    if (dist > tempDist) {
                        dist = tempDist;
                        reference = i;
                        temp = obstacles[reference].obstacle;
                    }
                }
        }

        if (temp != null)
            obstacles[reference].aiCover = ai;

        return temp;
    }*/

    public Vector3 AssignHidingPoint(GameObject ai, float range) {

        Vector3 temp = ai.transform.position;
        float dist = Mathf.Infinity;
        bool changed = false;
        int reference = 0;

        for (var i = 0; i < readerInst.boundPoints.Count; i++) {


            if (!readerInst.boundPoints[i].aiCover) {
                if ((player.transform.position - readerInst.boundPoints[i].centrePoint).sqrMagnitude < range * range) {
                    float tempDist = (readerInst.boundPoints[i].centrePoint - ai.transform.position).sqrMagnitude;

                    if (dist > tempDist) {
                        RaycastHit hit;
                        if (Physics.Linecast(readerInst.boundPoints[i].centrePoint, player.transform.position, out hit)) {
                            if (hit.transform.root != player.transform) {
                                temp = readerInst.boundPoints[i].centrePoint;
                                dist = tempDist;
                                reference = i;
                                changed = true;
                            }
                        }
                    }
                }
            } else if (readerInst.boundPoints[i].aiCover == ai) {
                ColliderReaderModule.BoundaryPoints boundInst = readerInst.boundPoints[i];
                boundInst.aiCover = null;
                readerInst.boundPoints[i] = boundInst;
            }
        }

        if (changed) {
            ColliderReaderModule.BoundaryPoints boundInst = readerInst.boundPoints[reference];
            boundInst.aiCover = ai;
            readerInst.boundPoints[reference] = boundInst;
        }
        return temp;
    }
}
