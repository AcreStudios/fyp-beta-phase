using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivillianManager : MonoBehaviour {

    [System.Serializable]
    public struct TaskList {
        public string taskName;
        public TaskLocation[] tasks;
        public string taskTag;
        public int taskCapacity;
        public float taskTimer;

        public Animation animationWhenPerformingTask;
    }

    [System.Serializable]
    public struct TaskLocation {
        public Transform taskLocation;
        public GameObject[] civillianOnTask;
    }

    public static CivillianManager instance;
    public TaskList[] taskLists;

    void Start() {
        instance = this;

        for (var i = 0; i < taskLists.Length; i++) {
            GameObject[] taskLocations = GameObject.FindGameObjectsWithTag(taskLists[i].taskTag);
            taskLists[i].tasks = new TaskLocation[taskLocations.Length];
            for (var j = 0; j < taskLists[i].tasks.Length; j++) {
                taskLists[i].tasks[j].taskLocation = taskLocations[j].transform;
                taskLists[i].tasks[j].civillianOnTask = new GameObject[taskLists[i].taskCapacity];
            }
        }
    }

    public TaskLocation TaskQuery(GameObject query, out float taskDuration, out int taskUser) {
        for (var i = 0; i < taskLists.Length; i++)
            for (var j = 0; j < taskLists[i].tasks.Length; j++)
                for (var k = 0; k < taskLists[i].tasks[j].civillianOnTask.Length; k++)
                    if (!taskLists[i].tasks[j].civillianOnTask[k]) {
                        taskDuration = taskLists[i].taskTimer;
                        taskUser = k;

                        taskLists[i].tasks[j].civillianOnTask[k] = query;
                        return taskLists[i].tasks[j];
                    }

        taskDuration = 0.1f;
        taskUser = 0;
        return new TaskLocation();
    }
}
