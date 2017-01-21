using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

    public static AIManager instance;
    public ColliderReaderModule readerInst;
    public Transform player;
    public AudioClip[] sfxList;
    //public ObstaclesData[] obstacles;

    void Start() {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public Vector3 AssignHidingPoint(GameObject ai, GameObject toReplace, float range) {

        Vector3 temp = ai.transform.position;
        float dist = Mathf.Infinity;
        bool changed = false;
        int reference = 0;

        for (var i = 0; i < readerInst.boundPoints.Count; i++) {
            if (readerInst.boundPoints[i].aiCover == ai) {

                ColliderReaderModule.BoundaryPoints boundInst = readerInst.boundPoints[i];

                if (!toReplace)
                    boundInst.aiCover = null;
                else
                    boundInst.aiCover = toReplace;

                readerInst.boundPoints[i] = boundInst;
            }

            if (!readerInst.boundPoints[i].aiCover && !toReplace) {
                if ((player.transform.position - readerInst.boundPoints[i].centrePoint).sqrMagnitude < range * range) {
                    float tempDist = (readerInst.boundPoints[i].centrePoint - ai.transform.position).sqrMagnitude;

                    if (dist > tempDist) {
                        RaycastHit hit;
                        if (Physics.Linecast(readerInst.boundPoints[i].centrePoint, player.transform.position, out hit)) {
                            if (hit.transform.root != player && !hit.transform.root.CompareTag("Enemy")) {
                                temp = readerInst.boundPoints[i].centrePoint;
                                dist = tempDist;
                                reference = i;
                                changed = true;
                                Debug.DrawLine(player.transform.position, readerInst.boundPoints[i].centrePoint, Color.black, 10f);
                            }
                        }
                    }
                }
            }
        }

        if (changed) {
            ColliderReaderModule.BoundaryPoints boundInst = readerInst.boundPoints[reference];
            boundInst.aiCover = ai;
            readerInst.boundPoints[reference] = boundInst;
        }

        return temp;
    }

    public void PlayRandomSound(Vector3 position) {
        if (sfxList.Length > 0)
            SoundManager.instance.PlaySoundOnce(position, sfxList[Random.Range(0, sfxList.Length)]);
    }
}

