using UnityEngine;
using System.Collections;

public class ScriptedEvent2 : EventResults {

    public override void ScriptedResult() {
        GameObject[] temp = GameObject.FindGameObjectsWithTag(gameObject.tag);

        for (var i=0; i < temp.Length; i++) {
            temp[i].GetComponent<CivillianAIExperiment>().currentState = CivillianAIExperiment.CivillianStates.Panic;
        }
    }
}
