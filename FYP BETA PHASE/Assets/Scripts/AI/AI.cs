using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class AI : AIFunctions {

    PatrolModule patrolMod;
    float stateChangeTimer;

    void Start() {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        startingPoint = transform.position;
        eColl = GetComponent<Collider>();

        animator = GetComponent<Animator>();

        if ((patrolMod = GetComponent<PatrolModule>()) != null) {
            if (patrolMod.patrolLocations.Length > 0) {
                defaultState = AIStates.Patrol;
                agent.destination = patrolMod.patrolLocations[0];
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

            agent.destination = patrolMod.patrolLocations[0];
        }

        if (knowsTarget)
            target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update() {

        if (damageTest) {
            damageTest = false;
            DamageRecieved();
        }

        switch (currentState) {
            case AIStates.Idle:
                if (target) {
                    AlertOtherTroops();
                    stateChangeTimer = Time.time + reactionTime;
                    destination = ObstacleHunting(ableToHide);
                    currentState = AIStates.Attacking;
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
                    destination = ObstacleHunting(ableToHide);
                    currentState = AIStates.Attacking;
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
                if (patrolMod.currentLocation < patrolMod.patrolLocations.Length) {
                    if (agent.velocity.sqrMagnitude == 0) {
                        transform.LookAt(target);
                        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                        animator.SetInteger("TreeState", 0);
                        if ((target.position - transform.position).sqrMagnitude < 5) {
                            patrolMod.currentLocation++;                           
                            agent.destination = patrolMod.patrolLocations[patrolMod.currentLocation];
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
                        //destination = transform.position;

                        if (target) {
                            transform.LookAt(target);
                            Attack();

                            RaycastHit hit;

                            if (Physics.Linecast(destination, target.position, out hit)) {
                                //Debug.DrawLine(destination, hit.point, Color.red, 20);
                                //Debug.DrawLine(transform.position, hit.point, Color.green);
                                if (hit.transform.root == target) {
                                    destination = ObstacleHunting(ableToHide);
                                    //Debug.DrawLine(transform.position, hit.point, Color.red);
                                }
                            }

                            if ((target.position - transform.position).sqrMagnitude > weaponRange * weaponRange)
                                destination = ObstacleHunting(ableToHide);
                        }

                    } else {
                        animator.SetInteger("TreeState", 1);
                        transform.LookAt(destination);
                    }
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                    agent.destination = destination;
                }
                //Debug.Log(destination);

                //Debug.DrawLine(destination, target.position, Color.blue);

                break;
        }

        destinationMarker.transform.position = destination;
    }

    public override void DamageRecieved() {
        toEscort = false;
        target = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = AIStates.Attacking;
    }
}
