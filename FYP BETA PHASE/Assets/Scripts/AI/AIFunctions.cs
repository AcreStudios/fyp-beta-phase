using UnityEngine;
using System.Collections;

public class AIFunctions : MonoBehaviour {
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
    public bool ableToHide;
    public bool ableToCommunicate;
    public bool toEscort;
    public bool knowsTarget;
    protected AIStates defaultState;

    [Header("Seek Settings (Do not touch)")]
    public float spaceBetweenChecks;
    public int possibleSpotsPerSide;
    Vector3[] multiplier;

    [Header("Weapons")]
    public float damage;
    public WeaponType attackType;
    public float attackInterval;
    public float weaponRange;
    public bool piercing;
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

    public Collider destinationMarker;

    public Transform target;
    protected Transform[] guns = new Transform[1];

    protected Transform linecastCheck;
    protected Vector3 startingPoint;
    protected bool showGunEffect;

    protected Animator animator;

    protected Collider tempObs;
    protected Collider eColl;
    protected UnityEngine.AI.NavMeshAgent agent;
    protected Vector3 destination;

    Vector3 currentNormalizedDist;

    void Awake() {
        multiplier = new Vector3[2];
        multiplier[0] = new Vector3(1, 0, 0);
        multiplier[1] = new Vector3(0, 0, 1);
    }

    public virtual void DamageRecieved() {
    }

    public void AlertOtherTroops() {
        Collider[] troops;

        troops = Physics.OverlapSphere(transform.position, 20);
        foreach (Collider troop in troops) {
            if (troop.tag == "Enemy")
                troop.transform.gameObject.GetComponent<AIFunctions>().target = target;
        }
    } //Keep for future reference.

    public void Triggered(Transform player) {
        target = player;
    }

    public void GroupWithAllies() {
        GameObject[] troops = GameObject.FindGameObjectsWithTag("Enemy");
        float dist;
        dist = Mathf.Infinity;

        foreach (GameObject troop in troops) {
            if (troop != gameObject) {
                float tempDist;

                tempDist = (troop.transform.position - startingPoint).magnitude;

                if (dist > tempDist) {
                    dist = tempDist;
                    startingPoint = troop.transform.position;
                }
            }
        }
    } //Keep for future reference.

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
                                AIManager.instance.PlayRandomSound(hit.point);

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

                    if (targetHit.tag == "Player") {

                        if (ableToDragPlayerOutOfCover) {
                            CoverSystem inst = targetHit.GetComponent<CoverSystem>();
                            inst.EnableController();
                        }
                    }

                    if ((ai = targetHit.GetComponent<AIFunctions>()) != null) {
                        //AIManager.instance.AssignHidingPoint(ai.gameObject, gameObject, weaponRange);
                        //ai.destination = ai.ObstacleHunting(ai.ableToHide);
                        ai.destination = ai.GetDestinationPoint();
                    }
                    attackTimer = Time.time + attackInterval;
                }
        }
    }

    public IEnumerator ChangeObjectLocation(GameObject obj, Vector3 location) {
        yield return new WaitForSeconds(0.1f);
        obj.transform.position = location;
    }

    public Vector3 GetDestinationPoint() {
        Collider[] colliders = Physics.OverlapSphere(target.position, 100);
        //Will return a position based on obstacles here if its enabled.
        for (var i = 0; i < colliders.Length; i++) {
            if (colliders[i].transform.CompareTag("Obstacles")) {
                Vector3 temp = Vector3.zero;
                temp = colliders[i].bounds.center - target.position;

                temp.x = (temp.x / Mathf.Abs(temp.x)) * colliders[i].bounds.extents.x;
                temp.y = 0;
                temp.z = (temp.z / Mathf.Abs(temp.z)) * colliders[i].bounds.extents.z;

                //Debug.DrawLine(target.position, colliders[i].bounds.center + temp, Color.red, 5f);
                if (CheckIfPosAvail(colliders[i].bounds.center + temp))
                    return colliders[i].bounds.center + temp;
            }
        }
        return ArcBasedPosition(target.position - transform.position);
    }

    //This method still needs some working on...
    public Vector3 ArcBasedPosition(Vector3 givenVector) {
        float total = Mathf.Abs(givenVector.x) + Mathf.Abs(givenVector.z);

        for (var i = -total; i < total + 1; i++) {
            Vector3 temp = new Vector3();

            temp.x = i;
            temp.z = total - Mathf.Abs(i);
            temp = Vector3.Normalize(temp) * total;

            //Debug.DrawLine(transform.position, transform.position + temp, Color.red, 5f);
            if (CheckIfPosAvail(transform.position + temp))
                return transform.position + temp;

            //Debug.DrawLine(transform.position, transform.position + temp, Color.blue, 5f);
            temp.z *= -1;

            if (CheckIfPosAvail(transform.position + temp))
                return transform.position + temp;
        }
        return transform.position;
    }

    public bool CheckIfPosAvail(Vector3 temp) {
        temp.y = 0.1f;
        bool hitFloor = false;

        Collider[] inCollision = Physics.OverlapCapsule(temp, temp, 1);

        foreach (Collider collision in inCollision) {
            if (collision != destinationMarker)
                if (collision.transform.tag == "Marker") {
                    return false;
                }
            if (collision.transform.tag == "Floor")
                hitFloor = true;
        }

        if (hitFloor)
            destinationMarker.transform.position = temp;

        return hitFloor;
    }
}
