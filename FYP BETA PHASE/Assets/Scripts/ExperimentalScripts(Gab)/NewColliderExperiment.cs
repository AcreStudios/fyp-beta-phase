using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NewColliderExperiment : MonoBehaviour {

    public Transform target;
    float targetAmount;

    public int radius;

    public void GetNewPoint() {
        Collider[] colliders = Physics.OverlapSphere(target.position, 100);
        //Will return a position based on obstacles here if its enabled.
        for (var i = 0; i < colliders.Length; i++) {
            if (colliders[i].transform.CompareTag("Obstacles")) {
                Vector3 temp = Vector3.zero;
                temp = colliders[i].bounds.center - target.position;

                temp.x = (temp.x / Mathf.Abs(temp.x)) * colliders[i].bounds.extents.x;
                temp.y = 0;
                temp.z = (temp.z / Mathf.Abs(temp.z)) * colliders[i].bounds.extents.z;

                Debug.DrawLine(target.position, colliders[i].bounds.center + temp, Color.red, 5f);
            }
        }
    }

    public void ExperimentForArc() {
        for (var i = -radius; i < radius; i++) {
            Vector3 temp = new Vector3();
            temp.x = i;
            temp.z = 50 - Mathf.Abs(i);
            Debug.DrawLine(transform.position, transform.position + temp, Color.red, 5f);
        }
        for (var i = radius; i > -radius; i--) {
            Vector3 temp = new Vector3();
            temp.x = i;
            temp.z = -(50 - Mathf.Abs(i));
            Debug.DrawLine(transform.position, transform.position + temp, Color.blue, 5f);
        }
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
                t.ExperimentForArc();
        }
    }
}
#endif
