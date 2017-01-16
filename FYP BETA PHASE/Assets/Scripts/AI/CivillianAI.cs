using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivillianAI : MonoBehaviour {

    public enum Actions {
        CheckForTask, Talk, PerformTask
    }

    public Actions actions;

    [Range(0, 1)]
    public float talkativity = 0.5f; //slider value  
    [Range(0, 1)]
    public float stmValue = 0.5f; //chances of finding new task after talking
    public float attentionSpan = 30;
    public bool randomlyGeneratedValues;   
    public GameObject target;

    public bool leaveTask;

    CivillianManager.TaskLocation instance;
    float timer;
    int currentTaskUser;

    void Start() {
        if (randomlyGeneratedValues) {
            talkativity = Random.value;
            stmValue = Random.value;
            attentionSpan = Random.Range(5, 60);
        }
    }

    void Update() {

        switch (actions) {
            case Actions.CheckForTask:
                if (timer <= Time.time) {
                    instance = CivillianManager.instance.TaskQuery(gameObject, out timer,out currentTaskUser);
                    timer += Time.time;

                    if (instance.taskLocation)
                        target = instance.taskLocation.gameObject;

                    if (target)
                        actions = Actions.PerformTask;
                }
                break;

            case Actions.PerformTask:
                if (timer <= Time.time) {
                    actions = Actions.CheckForTask;
                }
                break;
        }

        if (leaveTask) {
            leaveTask = false;
            StopTask();
        }
    }

    public void StopTask() {
        instance.civillianOnTask[currentTaskUser] = null;
        target = null;
        timer = Time.time;
    }

    public void TalkToThis() {

    }
}
