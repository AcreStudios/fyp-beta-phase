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

    [Header("Weapons")]
    public WeaponType attackType;
    public float attackInterval;
    public float weaponRange;
    public bool piercing;
    float attackTimer;

    [Header("Raycast Shooting Attack Settings")]
    public float gunSprayValue;

    [Header("Area Attack Settings")]
    public float areaTestRadius;

    [Header("Debug")]
    public bool damageTest;
    
    public Collider destinationMarker;

    protected Transform target;
    Transform[] guns = new Transform[1];
   
    protected Transform linecastCheck;
    protected Vector3 startingPoint;
    protected bool showGunEffect;
    public GameObject gunEffect;
    protected Animator animator;

    protected Collider tempObs;
    protected Collider eColl;
    protected NavMeshAgent agent;
    protected Vector3 destination;

    void Awake() {
        gameObject.tag = "Enemy";

        guns[0] = transform.Find("Hanna_GunL");
        linecastCheck = transform.Find("LinecastChecker");
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
                    //Collider[] units = Physics.OverlapSphere()
                    break;
            }

            //Handle hits here
            if (targetHit != null)
                if (targetHit != transform) {
                    Health hp = targetHit.GetComponent<Health>();
                    AIFunctions ai;
                    if (hp && hp.isActiveAndEnabled)
                        hp.ReceiveDamage(5);

                    if (targetHit.tag == "Player")
                        if (HitFeedbackManager.instance)
                            HitFeedbackManager.instance.RetriggerHitEvent();

                    if ((ai = targetHit.GetComponent<AIFunctions>()) != null) {
                        Vector3 toNorm = Vector3.Normalize(target.position - transform.position);
                        //Debug.Log(hit.transform.root + " was hit by " + transform.root);
                        ai.DisplaceAILocation(toNorm);
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

    public void DisplaceAILocation(Vector3 normalizedEnemyVector) {

        int ai = 0;
        Collider[] inCollision = Physics.OverlapCapsule(target.position - (normalizedEnemyVector * weaponRange), target.position - (normalizedEnemyVector * weaponRange), 1);

        foreach (Collider collision in inCollision)
            if (collision != destinationMarker)
                if (collision.transform.tag == "Marker")
                    ai++;

        while (ai > 0) {
            ai = 0;
            normalizedEnemyVector.x += 0.1f;
            normalizedEnemyVector.z += 0.1f;

            inCollision = Physics.OverlapCapsule(target.position - (normalizedEnemyVector * weaponRange), target.position - (normalizedEnemyVector * weaponRange), 1);

            foreach (Collider collision in inCollision)
                if (collision != destinationMarker)
                    if (collision.transform.tag == "Marker")
                        ai++;
        }

        destination = target.position - (normalizedEnemyVector * weaponRange);
        destinationMarker.transform.position = target.position - (normalizedEnemyVector * weaponRange);
    }

    public Vector3 ObstacleHunting(bool hide) {
        Vector3 temp = transform.position;
        Vector3 backUp = temp;

        if (hide)
            temp = AIManager.instance.AssignHidingPoint(gameObject, weaponRange);

        if (temp == backUp) {
            temp = target.position - transform.position;
            temp = Vector3.Normalize(temp);
            DisplaceAILocation(temp);
            return destination;
        }
        return temp;
    }

    Vector3 ShortObstacleException(Collider obsColl) {
        Vector3 temp = Vector3.zero;
        temp = obsColl.ClosestPointOnBounds(target.position) + ((obsColl.bounds.center - obsColl.ClosestPointOnBounds(target.position)) * 2);

        return temp;
    }
}
