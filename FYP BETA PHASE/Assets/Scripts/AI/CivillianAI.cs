using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CivillianAI : MonoBehaviour {

    public enum Actions {
        CheckForTask, PerformTask, Walk, PrepareForWalk
    }

    public Actions actions;

    [Range(0, 1)]
    public float talkativity = 0.5f; //slider value  
    [Range(0, 1)]
    public float socialbility = 0.5f; 
    public float attentionSpan = 30;
    public bool randomlyGeneratedValues;

    public GameObject target;
    public List<CivillianAI> friends;
    CivillianManager.TaskLocation instance;
    float timer;
    int currentTaskUser;
    NavMeshAgent agent;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        friends = new List<CivillianAI>();
        instance = new CivillianManager.TaskLocation();

        if (randomlyGeneratedValues) {
            talkativity = Random.value;
            socialbility = Random.value;
            attentionSpan = Random.Range(5, 60);
        }
    }

    void Update() {
        switch (actions) {
            case Actions.CheckForTask:
                if (timer <= Time.time) {
                    FindNewTask(instance, currentTaskUser);

                    if (instance.taskLocation) {
                        target = instance.taskLocation.gameObject;
                        actions = Actions.PrepareForWalk;
                        agent.destination = target.transform.position;
                    } else {
                        //Look for someone to talk to?
                    }
                }
                break;

            case Actions.PerformTask:
                if (timer <= Time.time)
                    actions = Actions.CheckForTask;
                break;

            case Actions.PrepareForWalk:
                if (agent.velocity.sqrMagnitude > 0)
                    actions = Actions.Walk;
                break;

            case Actions.Walk:
                if (agent.velocity.sqrMagnitude == 0) {
                    actions = Actions.PerformTask;
                    timer += Time.time;
                }
                break;
        }
    }

    public void FindNewTask(CivillianManager.TaskLocation inst, int taskUser) {
        instance = CivillianManager.instance.TaskQuery(gameObject, out timer, out currentTaskUser);

        if (inst.taskLocation)
            inst.civillianOnTask[taskUser] = null;
    }

    public void Talk(CivillianAI query) {
        if (Random.value <= talkativity) {
            timer = attentionSpan;
            target = query.gameObject;
            actions = Actions.Walk;
        }
    }
}
