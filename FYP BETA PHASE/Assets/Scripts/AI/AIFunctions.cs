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

    public Vector3 GetDestinationPoint() {
        Collider[] colliders = Physics.OverlapSphere(target.position, 100);

        for (var i = 0; i < colliders.Length; i++) {
            if (colliders[i].transform.CompareTag("Obstacles")) {
                Vector3 temp = Vector3.zero;
                temp = colliders[i].bounds.center - target.position;

                temp.x = (temp.x / Mathf.Abs(temp.x)) * colliders[i].bounds.extents.x;
                temp.y = 0;
                temp.z = (temp.z / Mathf.Abs(temp.z)) * colliders[i].bounds.extents.z;

                if (CheckIfPosAvail(colliders[i].bounds.center + temp))
                    return colliders[i].bounds.center + temp;
            }
        }
        return ArcBasedPosition(new Vector3(20, 0, 0), target.position);
    }

    //This method still needs some working on...
    public Vector3 ArcBasedPosition(Vector3 givenVector, Vector3 targetPos) {
        float total = Mathf.Abs(givenVector.x) + Mathf.Abs(givenVector.z);

        for (var i = -total; i < total + 1; i++) {
            Vector3 temp = new Vector3();

            temp.x = i;
            temp.z = total - Mathf.Abs(i);
            temp = Vector3.Normalize(temp) * total;
            //Debug.DrawLine(targetPos, targetPos + temp, Color.red, 5f);
            if (CheckIfPosAvail(targetPos + temp))
                return targetPos + temp;

            temp.z *= -1;
            //Debug.DrawLine(targetPos, targetPos + temp, Color.blue, 5f);
            if (CheckIfPosAvail(targetPos + temp))
                return targetPos + temp;
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
