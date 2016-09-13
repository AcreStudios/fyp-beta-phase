using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(NavMeshAgent))]
public class CivillianAIExperiment : MonoBehaviour {

    public enum CivillianStates {
        Calm, Panic, Alert
    }

    public CivillianStates currentState;
    public float detectionRadius;
    public Transform target;

    NavMeshAgent agent;
    Vector3 destination;
    RaycastHit hit;

    Collider placeToHide;
    Collider lastObs;

    Animator animator;

    void Start() {
        //currentState = CivillianStates.Calm;
        destination = transform.position;
        agent = GetComponent<NavMeshAgent>();

        animator = transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    void Update() {
        switch (currentState) {
            case CivillianStates.Calm:
                animator.SetInteger("TreeState", Random.Range(0, 2));
                break;

            case CivillianStates.Panic:
                if ((destination - transform.position).magnitude < 1) {
                    animator.SetInteger("TreeState", 2);
                    if (Physics.Linecast(transform.position, target.position, out hit)) {
                        if (hit.transform == target) {
                            destination = HuntForHidingSpot();
                            //Debug.Log("Working");
                            
                        }
                    }
                } else {
                    agent.destination = destination;
                    animator.SetInteger("TreeState", 3);
                }

                break;

            case CivillianStates.Alert:
                if ((target.position - transform.position).magnitude < detectionRadius) {
                    destination = HuntForHidingSpot();
                    currentState = CivillianStates.Panic;
                }
                animator.SetInteger("TreeState", 0);
                Debug.Log("Player not in range, but AI on alert"); //Maybe play a heightened talking anim
                break;
        }
    }

    Vector3 HuntForHidingSpot() {

        Collider[] temp;

        float dist = Mathf.Infinity;

        temp = Physics.OverlapSphere(transform.position, detectionRadius);


            foreach (Collider obs in temp) {
                if (obs.transform.tag == "Obstacles") {
                    if (obs != lastObs) {
                        float tempDist = (obs.transform.position - transform.position).magnitude;
                        if (tempDist < dist) {
                            dist = tempDist;
                            placeToHide = obs;
                        }
                    }
                }
            }
        if (placeToHide) {
            lastObs = placeToHide;
            return placeToHide.ClosestPointOnBounds(target.position) + ((placeToHide.bounds.center - placeToHide.ClosestPointOnBounds(target.position)) * 2);
        }
        return transform.position; //Maybe return a further location for AI to run away 
    }
}
