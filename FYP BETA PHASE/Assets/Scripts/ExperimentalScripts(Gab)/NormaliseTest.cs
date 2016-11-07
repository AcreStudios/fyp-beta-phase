using UnityEngine;
using System.Collections;

public class NormaliseTest : MonoBehaviour {

    public Transform target;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        Debug.Log(Vector3.Normalize(target.position - transform.position));
    }
}
