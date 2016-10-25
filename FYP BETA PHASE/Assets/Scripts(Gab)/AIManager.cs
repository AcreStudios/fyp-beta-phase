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
    public Transform player;
    public ObstaclesData[] obstacles;

    void Start() {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject[] temp = GameObject.FindGameObjectsWithTag("Obstacles");
        obstacles = new ObstaclesData[temp.Length];

        for (var i = 0; i < obstacles.Length; i++)
            obstacles[i].obstacle = temp[i].GetComponent<Collider>();
    }

    public Collider AssignCover(GameObject ai, float range) {
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
    }
}
