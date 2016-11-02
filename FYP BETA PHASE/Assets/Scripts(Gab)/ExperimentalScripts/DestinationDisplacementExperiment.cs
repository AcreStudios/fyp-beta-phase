using UnityEngine;
using System.Collections;

public class DestinationDisplacementExperiment : MonoBehaviour {

    public Transform target;
    public Vector3 normalizedDist;
    public float range;

    int ai;

    void Start() {
        normalizedDist = Vector3.Normalize(target.position - transform.position);
        
        //transform.position = target.position - (normalizedDist * range);
        DisplaceLocation();
    }

    void DisplaceLocation() {
        ai = 0;
        Collider[] inCollision = Physics.OverlapCapsule(target.position - (normalizedDist * range), target.position - (normalizedDist * range), 1);

        foreach (Collider collision in inCollision)
            if (collision.transform.tag == "Marker")
                ai++;

        while (ai >0) {
            ai = 0;
            normalizedDist.x += 0.1f;
            normalizedDist.z += 0.1f;

            inCollision = Physics.OverlapCapsule(target.position - (normalizedDist * range), target.position - (normalizedDist * range), 1);

            foreach (Collider collision in inCollision)
                if (collision.transform.tag == "Marker")
                    ai++;
        }

        transform.position = target.position - (normalizedDist * range);
    }

}
