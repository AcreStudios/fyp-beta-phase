using UnityEngine;
using System.Collections;

public class NormaliseTest : MonoBehaviour {

    public Transform target;
    public float spaceBetweenChecks;
    public float length;
    public int possibleSpotsPerSide;

    Color color;

    void Start() {
        color = Color.red;
    }

    void Update() {
        //color = Random.ColorHSV();
        for (var i = -(possibleSpotsPerSide/2); i < (possibleSpotsPerSide/2) +1; i++) {
            Debug.DrawLine(transform.position, transform.position + (new Vector3(spaceBetweenChecks * i, transform.position.y, 1) * length), color);
            Debug.DrawLine(transform.position, transform.position + (new Vector3(spaceBetweenChecks * i, transform.position.y, 1) * -length), color);

            Debug.DrawLine(transform.position, transform.position + (new Vector3(1, transform.position.y, spaceBetweenChecks * i) * length), color);
            Debug.DrawLine(transform.position, transform.position + (new Vector3(1, transform.position.y, spaceBetweenChecks * i) * -length), color);
        }

    }
}
