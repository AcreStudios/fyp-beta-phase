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

    #region Building/Cover Shooting Module Variables
    protected Collider tempObs;
    protected Collider eColl;
    Vector3 playerTouchPoint;
    Vector3 finalPoint;
    Vector3 otherEnd;
    Vector3 additionPoint;
    protected Vector3 lastHidingPoint;
    protected Vector3 lastAttackPoint;
    int valueToChange;
    protected Vector3 attackPoint;

    Vector3 enemyTouchPoint;

    int index;
    float temp;
    #endregion

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
    }

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
    }

    public bool Shooting() {
        animator.SetInteger("TreeState", 2);
        if (Time.time > shootingTime) {
            Vector3 offset;
            AlertOtherTroops();

            offset = new Vector3(Random.Range(-gunSprayValue, gunSprayValue), Random.Range(-gunSprayValue, gunSprayValue), 0);
            foreach (Transform gun in guns) {
                gun.LookAt(target);
                //StartCoroutine(Test(transform.position));
                Debug.DrawLine(gun.position, gun.position + transform.TransformDirection(0, 0, range) + offset, Color.red, 5);
                RaycastHit hit;
                if (Physics.Raycast(gun.position, gun.position + transform.TransformDirection(0, 0, range) + offset, out hit)) {
                    Health hp = hit.transform.root.GetComponent<Health>();
                    if (hp && hp.isActiveAndEnabled)
                        hp.ReceiveDamage(5);
                }
            }
            shootingTime = Time.time + shootInterval;
            return true;
        }
        return true;
    }

    /*IEnumerator Test(Vector3 pos) {
        gunEffect.SetActive(true);
        gunEffect.transform.position = pos;
        showGunEffect = true;
        scaleValue = Vector3.Normalize(target.position - gunEffect.transform.position); 
        yield return new WaitForSeconds(0.1f);
        showGunEffect = false;
        gunEffect.SetActive(false);
    }*/

    #region Building/Cover Shooting Module    

    public Vector3 ObstacleHunting() {
        Collider[] obstacles;
        float dist = Mathf.Infinity;

        tempObs = null;
        obstacles = Physics.OverlapSphere(target.position, range);
        foreach (Collider obstacle in obstacles) {
            if (obstacle.transform.tag == "Obstacles") {
                if ((obstacle.transform.position - transform.position).magnitude < dist) {
                    dist = (obstacle.transform.position - transform.position).magnitude;
                    tempObs = obstacle;
                }
            }
        }

        if (tempObs != null) {
            if ((tempObs.bounds.extents.y * 2) < (eColl.bounds.extents.y * 1.5f)) 
                return ShortObstacleException(tempObs);
            
            return InitializingCalculations(tempObs);
        }

        Vector3 temp;
        float totalMag;

        totalMag = (target.position - transform.position).magnitude;
        temp = target.position - transform.position;
        temp = Vector3.Normalize(temp);
        return target.position - (temp * 30); //Scales this to hp?
    }

    Vector3 ShortObstacleException(Collider obsColl) {
        Vector3 temp = Vector3.zero;
        temp = obsColl.ClosestPointOnBounds(target.position) + ((obsColl.bounds.center - obsColl.ClosestPointOnBounds(target.position)) * 2);
        lastHidingPoint = temp;
        lastAttackPoint = temp;

        return temp;
    }

    Vector3 InitializingCalculations(Collider obsColl) {
        playerTouchPoint = obsColl.ClosestPointOnBounds(target.position);
        attackPoint = playerTouchPoint;

        if (playerTouchPoint.x == obsColl.bounds.min.x || playerTouchPoint.x == obsColl.bounds.max.x) {
            return ObstacleCorners(2, 0, obsColl);
        } else if (playerTouchPoint.z == obsColl.bounds.min.z || playerTouchPoint.z == obsColl.bounds.max.z) {
            return ObstacleCorners(0, 2, obsColl);
        }
        return Vector3.zero;
    }

    Vector3 ObstacleCorners(int firstIndex, int secondIndex, Collider obsColl) { //Stuff from below here are sending wrong info probably
        float extentF;

        finalPoint = playerTouchPoint;
        otherEnd = playerTouchPoint;

        enemyTouchPoint = lastHidingPoint;

        if (enemyTouchPoint.x == obsColl.bounds.min.x || enemyTouchPoint.x == obsColl.bounds.max.x) {
            index = 2;
        } else if (enemyTouchPoint.z == obsColl.bounds.min.z || enemyTouchPoint.z == obsColl.bounds.max.z) {
            index = 0;
        }

        temp = enemyTouchPoint[index] - playerTouchPoint[index];

        if (playerTouchPoint[firstIndex] - obsColl.bounds.center[firstIndex] > 0) {
            if (temp < 0) {
                otherEnd[firstIndex] = obsColl.bounds.min[firstIndex];
                lastHidingPoint = otherEnd;
                return SmoothenVector(otherEnd, secondIndex, obsColl.bounds.center);
            } else {
                finalPoint[firstIndex] = obsColl.bounds.max[firstIndex];
                extentF = (playerTouchPoint[firstIndex] - obsColl.bounds.center[firstIndex]) / obsColl.bounds.extents[firstIndex];
            }

        } else {
            if (temp < 0) {
                finalPoint[firstIndex] = obsColl.bounds.min[firstIndex];
                extentF = (obsColl.bounds.center[firstIndex] - playerTouchPoint[firstIndex]) / obsColl.bounds.extents[firstIndex];
            } else {
                otherEnd[firstIndex] = obsColl.bounds.max[firstIndex];
                lastHidingPoint = otherEnd;
                return SmoothenVector(otherEnd, secondIndex, obsColl.bounds.center);
            }
        }

        if (extentF == 1 || extentF == -1) {
            if (playerTouchPoint[secondIndex] == obsColl.bounds.min[secondIndex]) {
                finalPoint[secondIndex] = obsColl.bounds.min[secondIndex] + (extentF * (obsColl.bounds.extents[secondIndex] * 2));
            }

            if (playerTouchPoint[secondIndex] == obsColl.bounds.max[secondIndex]) {
                finalPoint[secondIndex] = obsColl.bounds.max[secondIndex] - (extentF * (obsColl.bounds.extents[secondIndex] * 2));
            }
        }

        lastHidingPoint = finalPoint;
        return SmoothenVector(finalPoint, firstIndex, obsColl.bounds.center);
    }

    Vector3 SmoothenVector(Vector3 corner, int indexToSmooth, Vector3 colliderCenter) {

        int otherIndex;

        if (corner.x == playerTouchPoint.x) {
            indexToSmooth = 0;
            otherIndex = 2;
        } else {
            indexToSmooth = 2;
            otherIndex = 0;
        }

        if (corner[otherIndex] - colliderCenter[otherIndex] > 0) {
            corner[otherIndex] += eColl.bounds.extents[otherIndex] * 2;
        } else {
            corner[otherIndex] -= eColl.bounds.extents[otherIndex] * 2;
        }

        lastAttackPoint = corner;

        if (corner[indexToSmooth] - colliderCenter[indexToSmooth] > 0) {
            corner[indexToSmooth] -= eColl.bounds.extents[indexToSmooth] * 4;
        } else {
            corner[indexToSmooth] += eColl.bounds.extents[indexToSmooth] * 4;
        }

        lastHidingPoint = corner;
        Debug.DrawLine(transform.position, lastAttackPoint, Color.yellow, 10);
        return lastAttackPoint;
    }

    #region Disabled
    /*Vector3 BuildingCalculation(int firstIndex, int secondIndex, Collider obsColl) {
        float extentF;
        //Vector3 otherSide;
        //float randomValue;

        otherEnd = playerTouchPoint;

        if (playerTouchPoint[firstIndex] - obsColl.bounds.center[firstIndex] > 0) {
            finalPoint[firstIndex] = obsColl.bounds.max[firstIndex];
            otherEnd[firstIndex] = obsColl.bounds.min[firstIndex];
            extentF = (playerTouchPoint[firstIndex] - obsColl.bounds.center[firstIndex]) / obsColl.bounds.extents[firstIndex];
        } else {
            finalPoint[firstIndex] = obsColl.bounds.min[firstIndex];
            otherEnd[firstIndex] = obsColl.bounds.max[firstIndex];
            extentF = (obsColl.bounds.center[firstIndex] - playerTouchPoint[firstIndex]) / obsColl.bounds.extents[firstIndex];
        }

        attackPoint[firstIndex] = finalPoint[firstIndex];

        if (playerTouchPoint[secondIndex] == obsColl.bounds.min[secondIndex]) {
            finalPoint[secondIndex] = obsColl.bounds.min[secondIndex] + (extentF * (obsColl.bounds.extents[secondIndex] * 2));
        }

        if (playerTouchPoint[secondIndex] == obsColl.bounds.max[secondIndex]) {
            finalPoint[secondIndex] = obsColl.bounds.max[secondIndex] - (extentF * (obsColl.bounds.extents[secondIndex] * 2));
        }

        
        /*if ((obsColl.bounds.extents.y * 2) < (eColl.bounds.extents.y * 2) * 0.75f) {
            otherSide = playerTouchPoint - ((playerTouchPoint - obsColl.bounds.center) * 2);
            randomValue = Random.value;

            if (randomValue <= 0.5f) {
                return CoverCalculation(finalPoint, otherSide - finalPoint, secondIndex, randomValue);
            } else {
                randomValue -= 0.5f;
                return CoverCalculation(otherEnd, otherSide - otherEnd, secondIndex, randomValue);
            }
        }
        

        if (otherEnd[secondIndex] != lastHidingPoint[secondIndex]) { //check if its the same side here?
            attackPoint[firstIndex] = otherEnd[firstIndex];
            lastHidingPoint = otherEnd;
            return otherEnd;
        }
        lastHidingPoint = finalPoint;
        return finalPoint;
    }

    Vector3 CoverCalculation(Vector3 initialPos, Vector3 distXZ, int toCalculateFirst, float probability) {

        int toCalculateSecond;
        float firstProbablityRange;
        float totalDist = (Mathf.Abs(distXZ.x) + Mathf.Abs(distXZ.z));

        toCalculateSecond = toCalculateFirst == 0 ? 2 : 0;

        firstProbablityRange = (Mathf.Abs(distXZ[toCalculateFirst]) / totalDist) * 0.5f;

        if (firstProbablityRange < probability) {
            distXZ[toCalculateSecond] *= (probability - firstProbablityRange) / (0.5f - firstProbablityRange);
        } else {
            distXZ[toCalculateSecond] = 0;
            distXZ[toCalculateFirst] *= probability / firstProbablityRange;
        }

        return initialPos + distXZ;
    }*/
    #endregion
    #endregion
}
