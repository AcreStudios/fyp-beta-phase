using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PatrolModule : MonoBehaviour {

    public Vector3[] patrolLocations;
    public int currentLocation;

    [HideInInspector] public int valueToAdd;
}
