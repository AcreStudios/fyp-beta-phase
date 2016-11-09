using UnityEngine;
using System.Collections;

public class AIFunctions : MonoBehaviour {

    Transform[] guns = new Transform[1];
    public Vector2 rotationRange;
    public float shootInterval;
    public float gunSprayValue;
    public float range;
    public Collider destinationMarker;
    //public GameObject gunEffect;

    public float Health { get; set; }

    protected Transform target;
    const float lerpAdditionValue = 0.03f;

    float lerpValue;
    float targetHeadRotation;

    float prevHeadRotation;
    float shootingTime;
    float rotateTime;
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
        Health = 100;
        gameObject.tag = "Enemy";

        guns[0] = transform.Find("Hanna_GunL");
        linecastCheck = transform.Find("LinecastChecker");
    }

    public virtual void DamageRecieved(float damage) {
        Health -= damage;
        if (Health <= 0) {
            Destroy(gameObject);
        }
    }

    public void LookAround() {
        if (rotateTime < Time.time) {
            if (lerpValue >= 1 || prevHeadRotation == targetHeadRotation) {
                lerpValue = 0;

                prevHeadRotation = targetHeadRotation;
                targetHeadRotation = Random.Range(rotationRange.x, rotationRange.y);
            } else {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Lerp(prevHeadRotation, targetHeadRotation, lerpValue), transform.eulerAngles.z);
                lerpValue += lerpAdditionValue;
                if (lerpValue >= 1 || prevHeadRotation == targetHeadRotation) {
                    rotateTime = Time.time + Random.Range(3, 6);
                }
            }
        }
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

    public bool Shooting() {
        animator.SetInteger("TreeState", 2);
        if (Time.time > shootingTime) {
            Vector3 offset;
            //AlertOtherTroops();

            offset = new Vector3(Random.Range(-gunSprayValue, gunSprayValue), Random.Range(-gunSprayValue, gunSprayValue), 0);
            foreach (Transform gun in guns) {
                gun.LookAt(target);

                gunEffect.transform.position = gun.position;
                gunEffect.SetActive(true);

                StartCoroutine(ChangeObjectLocation(gunEffect, gun.position + gun.TransformDirection(0, 0, range) + offset));
                StartCoroutine(TurnOffObject(0.2f, gunEffect));

                RaycastHit hit;
                if (Physics.Raycast(gun.position, gun.TransformDirection(0, 0, range) + offset, out hit)) {
                    if (hit.transform.root != transform) {
                        Health hp = hit.transform.root.GetComponent<Health>();
                        AIFunctions ai;
                        if (hp && hp.isActiveAndEnabled)
                            hp.ReceiveDamage(5);

                        if (hit.transform.root.tag == "Player")
                            HitFeedbackManager.instance.RetriggerHitEvent();

                        if ((ai = hit.transform.root.GetComponent<AIFunctions>()) != null) {
                            Vector3 toNorm = Vector3.Normalize(target.position - transform.position);
                            //Debug.Log(hit.transform.root + " was hit by " + transform.root);
                            ai.DisplaceAILocation(toNorm);
                        }
                    }
                }
            }
            shootingTime = Time.time + shootInterval;
            return true;
        }
        return true;
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
        Collider[] inCollision = Physics.OverlapCapsule(target.position - (normalizedEnemyVector * range), target.position - (normalizedEnemyVector * range), 1);

        foreach (Collider collision in inCollision)
            if (collision != destinationMarker)
                if (collision.transform.tag == "Marker")
                    ai++;

        while (ai > 0) {
            ai = 0;
            normalizedEnemyVector.x += 0.1f;
            normalizedEnemyVector.z += 0.1f;

            inCollision = Physics.OverlapCapsule(target.position - (normalizedEnemyVector * range), target.position - (normalizedEnemyVector * range), 1);

            foreach (Collider collision in inCollision)
                if (collision != destinationMarker)
                    if (collision.transform.tag == "Marker")
                        ai++;
        }

        destination = target.position - (normalizedEnemyVector * range);
        destinationMarker.transform.position = target.position - (normalizedEnemyVector * range);
    }

    public Vector3 ObstacleHunting() {

        //if (tempObs)
        //return ShortObstacleException(tempObs);
        //else
        //tempObs = AIManager.instance.AssignCover(gameObject, range);
        //AIManager.instance.AssignCover
        Vector3 temp = transform.position;
        Vector3 backUp = temp;
        temp = AIManager.instance.AssignHidingPoint(gameObject, range); ;

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
