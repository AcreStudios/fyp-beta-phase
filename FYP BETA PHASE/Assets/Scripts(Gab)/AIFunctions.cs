using UnityEngine;
using System.Collections;

public class AIFunctions : MonoBehaviour {

    Transform[] guns = new Transform[1];
    public Vector2 rotationRange;
    public float shootInterval;
    public float gunSprayValue;
    public float range;
    //public GameObject gunEffect;

    public float Health { get; set; }

    protected Transform target;
    const float lerpAdditionValue = 0.03f;

    float lerpValue;
    float targetHeadRotation;

    float prevHeadRotation;
    float shootingTime;
    float rotateTime;
    bool ableToRotate;
    protected Vector3 startingPoint;
    protected bool showGunEffect;
    protected Vector3 scaleValue;
    protected Animator animator;

    protected Collider tempObs;
    protected Collider eColl;
    protected NavMeshAgent agent;
    protected Vector3 destination;

    void Awake() {
        Health = 100;
        ableToRotate = true;
        gameObject.tag = "Enemy";

        guns[0] = transform.Find("Hanna_GunL");
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
        if (Time.time > shootingTime) { //Draw a raycast here to see if anything is in its line of sight?
            Vector3 offset;
            AlertOtherTroops();

            offset = new Vector3(Random.Range(-gunSprayValue, gunSprayValue), Random.Range(-gunSprayValue, gunSprayValue), 0);
            foreach (Transform gun in guns) {
                gun.LookAt(target);
                Debug.DrawLine(gun.position, gun.position + transform.TransformDirection(0, 0, range) + offset, Color.red, 5);
                RaycastHit hit;
                if (Physics.Raycast(gun.position, gun.position + transform.TransformDirection(0, 0, range) + offset, out hit)) {
                    Health hp = hit.transform.root.GetComponent<Health>();
                    AIFunctions ai;
                    if (hp && hp.isActiveAndEnabled)
                        hp.ReceiveDamage(5);

                    if ((ai = hit.transform.root.GetComponent<AIFunctions>()) != null){
                        Vector3 toNorm = Vector3.Normalize(target.position - transform.position);
                        toNorm.x += 0.5f;
                        toNorm.y += 0.5f;
                        Debug.Log(hit.transform);
                        Debug.Log(hit.transform.root +" was hit by " + transform.root);
                        ai.InFiringLine(toNorm);
                    }
                }
            }
            shootingTime = Time.time + shootInterval;
            return true;
        }
        return true;
    }

    public void InFiringLine(Vector3 normalizedEnemyVector) {
        destination = target.position - (normalizedEnemyVector * range);
        Debug.Log("Working!");
    }

    public Vector3 ObstacleHunting() {

        if (tempObs)
            return ShortObstacleException(tempObs);
        else
            tempObs = AIManager.instance.AssignCover(gameObject, range);

        Vector3 temp = Vector3.zero;

        temp = target.position - transform.position;
        temp = Vector3.Normalize(temp);
        return target.position - (temp * range); //Scales this to hp?
    }

    Vector3 ShortObstacleException(Collider obsColl) {
        Vector3 temp = Vector3.zero;
        temp = obsColl.ClosestPointOnBounds(target.position) + ((obsColl.bounds.center - obsColl.ClosestPointOnBounds(target.position)) * 2);

        return temp;
    }
}
