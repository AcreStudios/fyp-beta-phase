using UnityEngine;
using System.Collections;

public class AIManager : MonoBehaviour {
    //Stores obstacle data, group data.
    [System.Serializable]
    public struct ObstaclesData {
        public Collider obstacle;
        public bool coverTaken;
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

    void Update() {

    }

    public Collider AssignCover(Vector3 pos, float range) {
        Collider temp = null;
        float dist = Mathf.Infinity;

        int reference = 0;

        for (var i = 0; i < obstacles.Length; i++) {
            if (!obstacles[i].coverTaken)
                if ((player.transform.position - obstacles[i].obstacle.transform.position).sqrMagnitude < range * range) {
                    float tempDist = (obstacles[i].obstacle.transform.position - pos).sqrMagnitude;

                    if (dist > tempDist) {
                        dist = tempDist;
                        reference = i;
                        temp = obstacles[reference].obstacle;
                    }
                }
        }

        if (temp != null)
            obstacles[reference].coverTaken = true;

        return temp;
    }
}
