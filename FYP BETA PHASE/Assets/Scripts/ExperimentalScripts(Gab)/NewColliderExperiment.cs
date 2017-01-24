using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NewColliderExperiment : MonoBehaviour {

    public Transform target;
    public Vector3 test;
    public int radius;

    public void GetNewPoint() {
        Collider[] colliders = Physics.OverlapSphere(target.position, 100);
        //Will return a position based on obstacles here if its enabled.
        for (var i = 0; i < colliders.Length; i++) {
            if (colliders[i].transform.CompareTag("Obstacles")) {
                Vector3 temp = Vector3.zero;
                temp = colliders[i].bounds.center - target.position;

                temp.x = (temp.x / Mathf.Abs(temp.x)) * colliders[i].bounds.extents.x; //Formula for directly behind
                temp.y = 0;
                temp.z = (temp.z / Mathf.Abs(temp.z)) * colliders[i].bounds.extents.z;

                if (colliders[i].bounds.center.y > 0.5f) {
                    if (Mathf.Abs(colliders[i].bounds.center.x - target.position.x) > Mathf.Abs(colliders[i].bounds.center.z - target.position.z))
                        temp.x *= -1;
                    else
                        temp.z *= -1;
                }

                Debug.DrawLine(target.position, colliders[i].bounds.center + temp, Color.red, 5f);
            }
        }
    }

    public Vector3 ArcBasedPosition(Vector3 givenVector, Vector3 targetPos, float givenLength) {
        Vector3 gradient = givenVector.x >= givenVector.z ? givenVector / givenVector.x : givenVector / givenVector.z;

        gradient *= -1;

        for (var i = -givenLength; i < givenLength + 1; i++) {
            Vector3 currentPosInCircle = gradient * i;
            Vector3 reflexedGradient = new Vector3(-(gradient.z), 0, gradient.x) * (givenLength - Mathf.Abs(i));

            Debug.DrawLine(targetPos, Vector3.Normalize(targetPos + currentPosInCircle + reflexedGradient) * givenLength, Color.red, 10);
            Debug.DrawLine(targetPos, Vector3.Normalize(targetPos + currentPosInCircle - reflexedGradient) * givenLength, Color.blue, 10);
            //Debug.DrawLine(targetPos, targetPos + (gradient * i), Color.red, 10f);
            //Debug.DrawLine(targetPos, targetPos + temp, Color.red, 5f);
            //if (CheckIfPosAvail(targetPos + temp))
            //return targetPos + temp;s

            //temp.z *= -1;
            //Debug.DrawLine(targetPos, targetPos + temp, Color.blue, 5f);
            //if (CheckIfPosAvail(targetPos + temp))
            //return targetPos + temp;
        }
        return transform.position;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NewColliderExperiment))]
public class NewColliderExperimentUI : Editor {

    NewColliderExperiment t;

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        t = target as NewColliderExperiment;
        if (t != null) {
            if (GUILayout.Button("Show new target point"))
                t.GetNewPoint();

            if (GUILayout.Button("Show new arc"))
                t.ArcBasedPosition(t.transform.position - t.target.position, t.transform.position, 50);
        }
    }
}
#endif
