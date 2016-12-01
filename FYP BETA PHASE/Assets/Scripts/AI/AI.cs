using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]

public class AI : AIFunctions {

    PatrolModule patrolMod;
    float stateChangeTimer;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        startingPoint = transform.position;
        eColl = GetComponent<Collider>();

        animator = GetComponent<Animator>();

        if ((patrolMod = GetComponent<PatrolModule>()) != null) {
            if (patrolMod.patrolLocations.Length > 0) {
                defaultState = AIStates.Patrol;
            } else {
                defaultState = AIStates.Idle;
            }
        } else {
            defaultState = AIStates.Idle;
        }

        destination = transform.position;
        currentState = defaultState;

        if (toEscort) {
            currentState = AIStates.Escort;
            knowsTarget = true;
        }

        if (knowsTarget)
            target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update() {

        if (damageTest) {
            damageTest = false;
            DamageRecieved(0);
        }

        switch (currentState) {
            case AIStates.Idle:
                if (target != null) {
                    AlertOtherTroops();
                    stateChangeTimer = Time.time +  reactionTime;
                    currentState = AIStates.Attacking;
                    destination = ObstacleHunting(ableToHide);
                } else {
                    if ((startingPoint - transform.position).magnitude < 1) {
                        animator.SetInteger("TreeState", 0);
                    } else {
                        agent.destination = startingPoint;
                    }
                }
                break;

            case AIStates.Patrol:
                if (target != null) {
                    AlertOtherTroops();
                    stateChangeTimer = Time.time + reactionTime;
                    currentState = AIStates.Attacking;
                    destination = ObstacleHunting(ableToHide);
                } else {
                    if ((patrolMod.patrolLocations[patrolMod.currentLocation] - transform.position).magnitude < 1) {
                        if (patrolMod.currentLocation >= patrolMod.patrolLocations.Length - 1) {
                            patrolMod.valueToAdd = -1;
                        } else if (patrolMod.currentLocation <= 0) {
                            patrolMod.valueToAdd = 1;
                        }

                        patrolMod.currentLocation += patrolMod.valueToAdd;
                    } else {
                        agent.destination = patrolMod.patrolLocations[patrolMod.currentLocation];
                        animator.SetInteger("TreeState", 1);
                        transform.LookAt(agent.destination);
                        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    }
                }
                break;

            case AIStates.Escort:
                if (patrolMod.currentLocation < patrolMod.limit) {
                    if (agent.velocity.sqrMagnitude == 0) {
                        transform.LookAt(target);
                        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                        animator.SetInteger("TreeState", 0);
                        if ((target.position - transform.position).magnitude < 2) {
                            patrolMod.currentLocation++;
                        }
                    } else {
                        agent.destination = patrolMod.patrolLocations[patrolMod.currentLocation];
                        animator.SetInteger("TreeState", 1);
                    }
                } else {
                    transform.LookAt(target);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    animator.SetInteger("TreeState", 0);
                }
                break;

            case AIStates.Attacking:
                if (Time.time > stateChangeTimer) {
                    if (agent.velocity.sqrMagnitude == 0) {
                        transform.LookAt(target);
                        Attack();

                        RaycastHit hit;

                        if (Physics.Linecast(destination, target.position, out hit)) {
                            //Debug.DrawLine(destination, hit.point, Color.red, 20);
                            if (hit.transform.root == target)
                                destination = ObstacleHunting(ableToHide);
                        }
                    } else {
                        animator.SetInteger("TreeState", 1);
                        transform.LookAt(destination);
                    }
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    agent.destination = destination;
                }

                break;
        }

        destinationMarker.transform.position = destination;
    }

    public override void DamageRecieved(float damage) {
        toEscort = false;
        currentState = AIStates.Attacking;
    }
}
