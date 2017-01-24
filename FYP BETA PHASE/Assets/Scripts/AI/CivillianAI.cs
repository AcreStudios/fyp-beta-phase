﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CivillianAI : AIFunctions {

    public enum Actions {
        CheckForTask, PerformTask, Walk, PrepareForWalk, WaitingForQuery
    }

    public Actions actions;

    [Range(0, 1)]
    public float talkativity = 0.5f; //slider value  
    //[Range(0, 1)]
    //public float socialbility = 0.5f;
    public float attentionSpan = 5;
    public float sightRange = 10;
    public bool randomlyGeneratedValues;

    CivillianManager.TaskLocation instance;
    CivillianAI civillianTarget;
    float timer;
    int currentTaskUser;

    Renderer colorChanging;

    void Start() {
        colorChanging = GetComponent<Renderer>();
        CivillianManager.instance.civillianAIList.Add(this);
        gameObject.tag = "Civillian";
        gameObject.name = (CivillianManager.instance.civillianAIList.Count - 1).ToString();
        agent = GetComponent<NavMeshAgent>();
        instance = new CivillianManager.TaskLocation();

        if (randomlyGeneratedValues) {
            talkativity = Random.value;
            //socialbility = Random.value;
            attentionSpan = Random.Range(5, 60);
        }
    }

    void Update() {
        switch (actions) {
            case Actions.CheckForTask:
                if (timer <= Time.time) {
                    FindNewTask(instance, currentTaskUser);

                    if (instance.taskLocation) {
                        target = instance.taskLocation;
                        destination = ArcBasedPosition(target.position - transform.position, target.position, 2f);
                        actions = Actions.PrepareForWalk;
                    } else if (Random.value <= talkativity)
                        FindSomeoneToTalkTo();
                }
                break;

            case Actions.PerformTask:
                if (timer <= Time.time) {
                    actions = Actions.CheckForTask;
                    colorChanging.material.color = Color.blue;
                }
                break;

            case Actions.PrepareForWalk:
                agent.destination = destination;

                if (destination == transform.position) {
                    target = null;
                    FindSomeoneToTalkTo();
                }

                if (agent.velocity.sqrMagnitude > 0) {
                    actions = Actions.Walk;
                }
                break;

            case Actions.Walk:
                if (agent.velocity.sqrMagnitude == 0) {
                    actions = Actions.PerformTask;
                    timer += Time.time;
                    colorChanging.material.color = Color.green;

                    if (civillianTarget) {
                        civillianTarget.actions = Actions.PerformTask;
                        civillianTarget.timer += Time.time;
                        //Debug.LogFormat("{0} is talking to {1}", gameObject.name, civillianTarget.name);
                        colorChanging.material.color = Color.red;
                    }
                }
                break;
        }
        destinationMarker.transform.position = destination;
    }

    public void FindNewTask(CivillianManager.TaskLocation inst, int taskUser) {
        instance = CivillianManager.instance.TaskQuery(gameObject, out timer, out currentTaskUser);
        //Debug.Log(instance.taskLocation);
        if (inst.taskLocation)
            inst.civillianOnTask[taskUser] = null;
    }

    public void FindSomeoneToTalkTo() {
        Collider[] inRadius = Physics.OverlapSphere(transform.position, sightRange);

        foreach (Collider stuff in inRadius)
            if (stuff.transform.CompareTag("Civillian") && stuff.gameObject != gameObject)
                if (CivillianManager.instance.civillianAIList[int.Parse(stuff.name)].Talk(this))
                    return;
    }

    public bool Talk(CivillianAI query) {
        if (Random.value <= talkativity && !target) {
            //To handle AI being queried.
            target = query.transform;
            destination = ArcBasedPosition(new Vector3(1, 0, 0), transform.position, 2);
            agent.destination = destination;
            timer = attentionSpan;
            actions = Actions.WaitingForQuery;
            colorChanging.material.color = Color.red;

            //To handle the other AI querying.
            query.timer = attentionSpan;
            query.target = transform;
            query.destination = ArcBasedPosition(transform.position - query.transform.position, transform.position, 2);
            query.actions = Actions.PrepareForWalk;
            query.civillianTarget = this;
            return true;
        }
        return false;
    }
}
