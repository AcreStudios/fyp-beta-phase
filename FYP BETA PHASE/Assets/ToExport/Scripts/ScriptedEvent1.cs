using UnityEngine;
using System.Collections;

public class ScriptedEvent1 : EventResults {
    public PatrolModule toEditLimit;
    
	public override void ScriptedResult() {
        Debug.Log("Working");
        toEditLimit.limit = toEditLimit.patrolLocations.Length - 1;
    }
}
