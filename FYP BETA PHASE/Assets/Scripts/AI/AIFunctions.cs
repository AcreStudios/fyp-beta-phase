using UnityEngine;
using System.Collections;

public class AIFunctions : MonoBehaviour {

    public Vector3 destination;
    public Collider destinationMarker;
    public Transform target;
    protected Transform[] guns = new Transform[1];

    protected Transform linecastCheck;
    protected Vector3 startingPoint;
    protected bool showGunEffect;
    protected Animator animator;
    protected UnityEngine.AI.NavMeshAgent agent;

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

    public IEnumerator ChangeObjectLocation(GameObject obj, Vector3 location) {
        yield return new WaitForSeconds(0.1f);
        obj.transform.position = location;
    }

    public Vector3 GetDestinationPoint(float range) {
        Collider[] colliders = Physics.OverlapSphere(target.position, range);

        for (var i = 0; i < colliders.Length; i++) {
            if (colliders[i].transform.CompareTag("Obstacles")) {
                Vector3 temp = Vector3.zero;
                temp = colliders[i].bounds.center - target.position;

                temp.x = (temp.x / Mathf.Abs(temp.x)) * colliders[i].bounds.extents.x;
                temp.y = 0;
                temp.z = (temp.z / Mathf.Abs(temp.z)) * colliders[i].bounds.extents.z;

                if (colliders[i].bounds.center.y > 0.5f) {
                    if (Mathf.Abs(colliders[i].bounds.center.x - target.position.x) > Mathf.Abs(colliders[i].bounds.center.z - target.position.z))
                        temp.x *= -1;
                    else
                        temp.z *= -1;
                }

                temp += colliders[i].bounds.center;
                temp.y = 0;
                if (CheckIfPosAvail(temp))
                    return temp;
            }
        }
        return ArcBasedPosition(target.position - transform.position, target.position, range);
    }

    public Vector3 ArcBasedPosition(Vector3 givenVector, Vector3 targetPos, float givenLength) {
        Vector3 gradient = Mathf.Abs(givenVector.x) >= Mathf.Abs(givenVector.z) ? givenVector / Mathf.Abs(givenVector.x) : givenVector / Mathf.Abs(givenVector.z);

        for (var i = -givenLength; i < givenLength + 1; i++) {
            Vector3 currentPosInCircle = gradient * i;
            Vector3 reflexedGradient = new Vector3(-(gradient.z), 0, gradient.x) * (givenLength - Mathf.Abs(i));

            //Debug.DrawLine(targetPos, targetPos + (Vector3.Normalize(currentPosInCircle - reflexedGradient) * givenLength), Color.red, 10);
            //Debug.DrawLine(targetPos, targetPos + (Vector3.Normalize(currentPosInCircle + reflexedGradient) * givenLength), Color.blue, 10);

            if (CheckIfPosAvail(targetPos + (Vector3.Normalize(currentPosInCircle - reflexedGradient) * givenLength)))
                return targetPos + (Vector3.Normalize(currentPosInCircle - reflexedGradient) * givenLength);

            if (CheckIfPosAvail(targetPos + (Vector3.Normalize(currentPosInCircle + reflexedGradient) * givenLength)))
                return targetPos + (Vector3.Normalize(currentPosInCircle + reflexedGradient) * givenLength);
        }
        return transform.position;
    }

    public bool CheckIfPosAvail(Vector3 temp) {
        temp.y = 0.1f;
        bool hitFloor = false;

        Collider[] inCollision = Physics.OverlapCapsule(temp, temp, 1);

        foreach (Collider collision in inCollision) {
            if (collision != destinationMarker)
                if (collision.transform.tag == "Marker")
                    return false;

            if (collision.transform.tag == "Floor")
                hitFloor = true;
        }

        if (hitFloor)
            destinationMarker.transform.position = temp;

        return hitFloor;
    }
}
