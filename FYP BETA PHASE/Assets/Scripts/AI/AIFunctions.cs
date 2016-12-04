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
    float attackTimer;

    [Header("Raycast Shooting Attack Settings")]
    public float gunSprayValue;
    public GameObject gunEffect;

    [Header("Area Attack Settings")]
    public float areaTestRadius;

    [Header("Debug")]
    public bool damageTest;
    public bool displayDebugMessage;

    public Collider destinationMarker;

    protected Transform target;
    Transform[] guns = new Transform[1];

    protected Transform linecastCheck;
    protected Vector3 startingPoint;
    protected bool showGunEffect;

    protected Animator animator;

    protected Collider tempObs;
    protected Collider eColl;
    protected NavMeshAgent agent;
    protected Vector3 destination;

    Vector3 currentNormalizedDist;

    void Awake() {
        gameObject.tag = "Enemy";

        guns[0] = transform.Find("Hanna_GunL");
        linecastCheck = transform.Find("LinecastChecker");

        multiplier = new Vector3[2];
        multiplier[0] = new Vector3(1, 0, 0);
        multiplier[1] = new Vector3(0, 0, 1);
    }

    public virtual void DamageRecieved(float damage) {
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
                        gunEffect.SetActive(true);

                        StartCoroutine(ChangeObjectLocation(gunEffect, gun.position + gun.TransformDirection(0, 0, weaponRange) + offset));
                        StartCoroutine(TurnOffObject(0.2f, gunEffect));

                        RaycastHit hit;
                        if (Physics.Raycast(gun.position, gun.TransformDirection(0, 0, weaponRange) + offset, out hit)) {
                            targetHit = hit.transform.root;
                        }
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
                        if (HitFeedbackManager.instance)
                            HitFeedbackManager.instance.RetriggerHitEvent();

                    if ((ai = targetHit.GetComponent<AIFunctions>()) != null) {
                        //Vector3 toNorm = Vector3.Normalize(target.position - transform.position);
                        //Debug.Log(hit.transform.root + " was hit by " + transform.root);
                        ai.destination = ai.DisplaceAILocation();
                    }
                    attackTimer = Time.time + attackInterval;
                }
        }
    }

    public IEnumerator TurnOffObject(float time, GameObject obj) {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }

    public IEnumerator ChangeObjectLocation(GameObject obj, Vector3 location) {
        yield return new WaitForSeconds(0.1f);
        obj.transform.position = location;
    }

    public Vector3 DisplaceAILocation() {
        Vector3 cachedV3 = transform.position;
        Vector3 temp = Vector3.zero;
        float dist = Mathf.Infinity;

        for (var k = -1; k < 2; k += 2) {
            foreach (Vector3 multiply in multiplier) {
                //color = Random.ColorHSV();
                for (var i = -(possibleSpotsPerSide / 2); i < (possibleSpotsPerSide / 2) + 1; i++) {
                    temp = Vector3.one;

                    for (var j = 0; j < 3; j++)
                        if (multiply[j] == 1)
                            temp[j] = spaceBetweenChecks * i;

                    temp *= k;
                    temp.y = 0;
                    temp = Vector3.Normalize(temp);

                    Vector3 tempNormStore = temp;
                    temp *= weaponRange;
                    temp += target.position;

                    if (ColliderCheck(temp)) {
                        if ((temp - transform.position).sqrMagnitude < dist) {
                            destinationMarker.transform.position = temp;
                            dist = (temp - transform.position).sqrMagnitude;
                            cachedV3 = temp;
                            currentNormalizedDist = tempNormStore;
                        }
                    }
                }
            }
        }
        return cachedV3;
    }

    public bool ColliderCheck(Vector3 temp) {
        bool hitFloor = false;
        if (displayDebugMessage)
            Debug.DrawLine(target.position, temp, Color.red, 10);
        Collider[] inCollision = Physics.OverlapCapsule(temp, temp, 1);

        foreach (Collider collision in inCollision) {
            if (collision != destinationMarker)
                if (collision.transform.tag == "Marker") {
                    return false;
                }
            if (collision.transform.tag == "Floor")
                hitFloor = true;
        }
        return hitFloor;
    }

    public Vector3 ObstacleHunting(bool hide) {
        Vector3 temp = transform.position;
        Vector3 backUp = temp;

        if (hide)
            temp = AIManager.instance.AssignHidingPoint(gameObject, weaponRange);

        if (temp == backUp)
            if (currentNormalizedDist == Vector3.zero)
                temp = DisplaceAILocation();
            else
                if (ColliderCheck(target.position + (currentNormalizedDist * weaponRange)))
                temp = target.position + (currentNormalizedDist * weaponRange);
            else
                temp = DisplaceAILocation();

        destinationMarker.transform.position = temp;
        return temp;
    }
}
