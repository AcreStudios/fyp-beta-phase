using UnityEngine;
using System.Collections;

public class NormaliseTest : MonoBehaviour {


    public Transform target;
    public float spaceBetweenChecks;
    public float length;
    public int possibleSpotsPerSide;
    Vector3[] multiplier;

    Color color;

    void Start() {
        color = Color.red;

        multiplier = new Vector3[2];
        multiplier[0] = new Vector3(1, 0, 0);
        multiplier[1] = new Vector3(0, 0, 1);

        int temp = 0;
        for (var j = 0; j < 10; j++)
            for (var i = 0; i < 10; i++) {
                if (i == 1) {
                    break;
                }
                temp = i;
                Debug.Log(temp);
            }
        //Debug.Log(temp);
        //multiplier.vectorMuliply = new float[3];

    }

    void Update() {
        //color = Random.ColorHSV();

        //Debug.DrawLine(transform.position, transform.position + (new Vector3(spaceBetweenChecks * i, transform.position.y, 1) * length), color);
        // Debug.DrawLine(transform.position, transform.position + (new Vector3(spaceBetweenChecks * i, transform.position.y, 1) * -length), color);

        //Debug.DrawLine(transform.position, transform.position + (new Vector3(1, transform.position.y, spaceBetweenChecks * i) * length), color);
        //Debug.DrawLine(transform.position, transform.position + (new Vector3(1, transform.position.y, spaceBetweenChecks * i) * -length), color);


        for (var k = -1; k < 2; k += 2) {
            foreach (Vector3 multiply in multiplier) {
                //color = Random.ColorHSV();
                for (var i = -(possibleSpotsPerSide / 2); i < (possibleSpotsPerSide / 2) + 1; i++) {
                    Vector3 temp = Vector3.one;

                    for (var j = 0; j < 3; j++)
                        if (multiply[j] == 1)
                            temp[j] = spaceBetweenChecks * i;

                    temp.y = transform.position.y;

                    Debug.DrawLine(transform.position, transform.position + (temp * length * k), color);
                }
            }
        }
    }
}
