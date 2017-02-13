using UnityEngine;
using System.Collections;

public class PlayerTrigger : MonoBehaviour {

    Collider[] troops;
    int prevTroops;

    void Start() {

    }

    void Update() {
        troops = Physics.OverlapSphere(transform.position, 1);

        if (troops.Length != prevTroops) {
            foreach (Collider troop in troops) {
                if (troop.tag == "Vision") {
                    //troop.transform.root.gameObject.GetComponent<AIFunctions>().Triggered(transform);
                }
            }
        }
        prevTroops = troops.Length;
    }
}
