using UnityEngine;
using System.Collections;

public class NormalizeTest : MonoBehaviour {

    public Transform[] specimens;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        foreach (Transform specimen in specimens) {
            Vector3 inst = Vector3.Normalize(specimen.position - transform.position);
            inst.x += 0.1f;
            Debug.DrawLine(transform.position, inst * 10, Color.red);
        }
    }
}
