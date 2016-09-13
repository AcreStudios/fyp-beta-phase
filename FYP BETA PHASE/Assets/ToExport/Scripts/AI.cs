using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]

public class AI : AIFunctions {

    public enum AIStates {
        Idle,
        Patrol,
        Attacking,
        Retreating,
        AttackingInOpen,
        Escort
    }

    public float reactionTime;
    public bool toEscort;
    AIStates currentState;
    AIStates defaultState;

    PatrolModule patrolMod;
    NavMeshAgent agent;
    Vector3 destination;
    bool hasStarted;
    float timer;

    //public bool damageTest;

    void Start() {

        agent = GetComponent<NavMeshAgent>();
        startingPoint = transform.position;
        eColl = GetComponent<Collider>();

        animator = transform.GetChild(0).GetComponent<Animator>();

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
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

    }

    void Update() {
        if (showGunEffect) {
            //gunEffect.transform.position += scaleValue*10;
        }
        switch (currentState) {
            case AIStates.Idle:

                if (target != null) {
                    if (!hasStarted) {
                        timer = Time.time + reactionTime;
                        hasStarted = true;
                    }
                    if (Time.time > timer) {
                        AlertOtherTroops();
                        //ObstacleHunting();
                        currentState = AIStates.Attacking;
                        hasStarted = false;
                    }
                } else {
                    hasStarted = false;
                    if ((startingPoint - transform.position).magnitude < 1) {
                        LookAround();
                        animator.SetInteger("TreeState", 0);
                    } else {
                        agent.destination = startingPoint;
                    }
                }
                break;

            case AIStates.Patrol:
                if (target != null) {
                    if (!hasStarted) {
                        timer = Time.time + reactionTime;
                        hasStarted = true;
                    }
                    if (Time.time > timer) {
                        AlertOtherTroops();
                        currentState = AIStates.Attacking;
                        hasStarted = false;
                    }
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

            case AIStates.Attacking:
                RaycastHit hit;

                if (tempObs == null) {
                    currentState = AIStates.AttackingInOpen;
                } else {
                    if (Physics.Linecast(lastHidingPoint, target.position, out hit)) { //if player can see it in its hiding spot
                        if (hit.transform.root.tag == "Player" || hit.transform.root == transform) {
                            destination = ObstacleHunting();
                            currentState = AIStates.Retreating;
                        }
                    }

                    if ((target.position - tempObs.transform.position).magnitude > range) {
                        destination = ObstacleHunting();
                        currentState = AIStates.Retreating;
                    }

                    if (tempObs != null)

                        if ((destination - transform.position).magnitude < 3) {
                            if ((lastAttackPoint - transform.position).magnitude < 3) {
                                transform.LookAt(target);
                                if (Shooting()) {                                    
                                }
                            }
                        } else {
                            agent.destination = destination;
                            animator.SetInteger("TreeState", 1);
                            transform.LookAt(destination);
                            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                        }
                }
                break;

            case AIStates.Retreating:

                if ((destination - transform.position).magnitude < 1) {
                    destination = ObstacleHunting();
                    AlertOtherTroops();
                    if ((destination - transform.position).magnitude < 1) {
                        currentState = AIStates.Attacking;
                    }
                } else {
                    agent.destination = destination;
                    animator.SetInteger("TreeState", 1);
                }
                break;

            case AIStates.AttackingInOpen:
                destination = ObstacleHunting();
                agent.destination = destination;
                animator.SetInteger("TreeState", 1);

                if ((destination - transform.position).magnitude < 1) {
                    if (Shooting()) {
                        transform.LookAt(target);
                        AlertOtherTroops();
                    }
                }
                if (tempObs != null) {
                    currentState = AIStates.Attacking;
                }
                break;

            case AIStates.Escort:
                if (patrolMod.currentLocation < patrolMod.patrolLocations.Length) {
                    if ((patrolMod.patrolLocations[patrolMod.currentLocation] - transform.position).magnitude < 2) {
                        transform.LookAt(target);
                        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                        if ((target.position - transform.position).magnitude < 2) {
                            patrolMod.currentLocation++;
                        }
                    } else {
                        agent.destination = patrolMod.patrolLocations[patrolMod.currentLocation];
                    }
                } else {
                    transform.LookAt(target);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                }
                break;
        }
    }

    public override void DamageRecieved(float damage) {
        if (currentState == AIStates.Attacking || currentState == AIStates.AttackingInOpen)
            StartCoroutine(ChangeDestination());

        currentState = AIStates.Attacking;
        base.DamageRecieved(damage);
    }

    IEnumerator ChangeDestination() {
        destination = lastHidingPoint;
        yield return new WaitForSeconds(3);
        destination = lastAttackPoint;
    }
}
