using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]

public class AI : AIFunctions {

    public enum AIStates {
        Idle,
        Patrol,
        Escort,
        Attacking
    }

    public enum WeaponType {
        RaycastShooting,
        Area
    }

    [Header("Behaviours")]
    public AIStates currentState;
    public float reactionTime;
    public bool toEscort;
    protected AIStates defaultState;

    [Header("Weapons")]
    public float damage;
    public WeaponType attackType;
    public float attackInterval;
    public float weaponRange;
    public bool ableToDragPlayerOutOfCover;
    float attackTimer;

    [Header("Raycast Shooting Attack Settings")]
    public float gunSprayValue;
    public TrailEffectFade gunEffect;

    [Header("Area Attack Settings")]
    public float areaTestRadius;

    [Header("Debug")]
    public bool damageTest;
    public bool displayDebugMessage;

    PatrolModule patrolMod;
    float stateChangeTimer;

    void Start() {
        gameObject.tag = "Enemy";

        guns[0] = transform.Find("Hanna_GunL");
        linecastCheck = transform.Find("LinecastChecker");

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        startingPoint = transform.position;

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
            agent.destination = patrolMod.patrolLocations[0];
        }

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
                    destination = GetDestinationPoint(weaponRange);
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
                    destination = GetDestinationPoint(weaponRange);
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

                        if (target) {
                            transform.LookAt(target);
                            Attack();

                            RaycastHit hit;

                            if (Physics.Linecast(destination, target.position, out hit)) {
                                //Debug.DrawLine(destination, hit.point, Color.red, 20);
                                //Debug.DrawLine(transform.position, hit.point, Color.green);
                                if (hit.transform.root == target) {
                                    destination = GetDestinationPoint(weaponRange);
                                    //Debug.DrawLine(transform.position, hit.point, Color.red);
                                }
                            }

                            if ((target.position - transform.position).sqrMagnitude > weaponRange * weaponRange)
                                destination = GetDestinationPoint(weaponRange);
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

    public override void DamageRecieved() {
        toEscort = false;
        target = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = AIStates.Attacking;
    }

    public void Attack() {
        animator.SetInteger("TreeState", 2);
        if (Time.time > attackTimer) {
            Transform targetHit = null;
            switch (attackType) {
                case WeaponType.RaycastShooting:
                    Vector3 offset;
                    offset = new Vector3(Random.Range(-gunSprayValue, gunSprayValue), Random.Range(-gunSprayValue, gunSprayValue), 0);
                    foreach (Transform gun in guns) {
                        gun.LookAt(target);

                        gunEffect.transform.position = gun.position;
                        gunEffect.gameObject.SetActive(true);
                        gunEffect.ObjectActive();
                        StartCoroutine(ChangeObjectLocation(gunEffect.gameObject, gun.position + gun.TransformDirection(0, 0, weaponRange) + offset));

                        RaycastHit hit;
                        Debug.DrawRay(gun.position, gun.TransformDirection(0, 0, weaponRange) + offset, Color.red);
                        if (Physics.Raycast(gun.position, gun.TransformDirection(0, 0, weaponRange) + offset, out hit))
                            if (hit.transform.CompareTag("NearPlayer"))
                                CivillianManager.instance.PlayRandomSound(hit.point);

                        if (Physics.Raycast(gun.position, gun.TransformDirection(0, 0, weaponRange) + offset, out hit, weaponRange, 1))
                            targetHit = hit.transform.root;


                    }
                    break;
                case WeaponType.Area:
                    Collider[] units = Physics.OverlapSphere(transform.position + transform.TransformDirection(0, 0, weaponRange), areaTestRadius);

                    foreach (Collider unit in units)
                        if (unit.transform.CompareTag("Player"))
                            targetHit = unit.transform;
                    break;
            }

            //Handle hits here
            if (targetHit != null)
                if (targetHit != transform) {
                    Health hp = targetHit.GetComponent<Health>();
                    AIFunctions ai;
                    if (hp && hp.isActiveAndEnabled)
                        hp.ReceiveDamage(damage);

                    if (targetHit.tag == "Player")
                        if (ableToDragPlayerOutOfCover) {
                            CoverSystem inst = targetHit.GetComponent<CoverSystem>();
                            inst.EnableController();
                        }

                    if ((ai = targetHit.GetComponent<AIFunctions>()) != null)
                        ai.destination = ai.GetDestinationPoint(weaponRange);

                    attackTimer = Time.time + attackInterval;
                }
        }
    }
}
