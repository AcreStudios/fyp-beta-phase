using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class EventManager : MonoBehaviour {

    [System.Serializable]
    public struct EventTree {
        public string eventTreeName;
        //public Color eventColors;       
        public bool treeSupercedeAll;
        public Events[] events;
        [HideInInspector]
        public int prevTreeIndex;
        [HideInInspector]
        public int currentGameEvent;
    }

    [System.Serializable]
    public struct Events {
        public string eventName;
        public string missionUI;
        public Triggers eventTriggers;
        public Triggered results;
        [HideInInspector]
        public bool calibrated;
    }

    [System.Serializable]
    public struct Triggers {
        public Vector3 triggerPosition;
        public float triggerRadius;
        public float timer;
        public GameObject[] checkIfDestroyed;
        public KeyCode key;
    }

    [System.Serializable]
    public struct Triggered {
        public GameObject[] spawns;
        public GameObject[] toDestroy;
        public EventResults[] scriptedEventsToTrigger;
        public int eventTreeToJump;
        public string debugMessage;
        public string levelNameToLoad;
    }

    public EventTree[] eventTrees;

    public int currentTreeIndex;
    public Text missionUI;

    int prevCount;

    void Update() {
        if (Application.isPlaying) {
            if (eventTrees[currentTreeIndex].currentGameEvent < eventTrees[currentTreeIndex].events.Length) {
                if (missionUI)
                    missionUI.text = eventTrees[currentTreeIndex].events[eventTrees[currentTreeIndex].currentGameEvent].missionUI;
                CheckTrigger(eventTrees[currentTreeIndex].events[eventTrees[currentTreeIndex].currentGameEvent], currentTreeIndex, eventTrees[currentTreeIndex].currentGameEvent);
            }
        } else {
            for (var i = 0; i < eventTrees.Length; i++)
                for (var j = 0; j < eventTrees[i].events.Length; j++) {
                    for (var k = 0; k < eventTrees[i].events[j].results.spawns.Length; k++) {
                        if (eventTrees[i].events[j].results.spawns[k] != null)
                            eventTrees[i].events[j].results.spawns[k].SetActive(false);
                    }
                }
        }
    }

    void CheckTrigger(Events currentEvent, int currentTreeIndex, int eventIndex) {

        if (!currentEvent.calibrated) {
            eventTrees[currentTreeIndex].events[eventIndex].eventTriggers.timer += Time.time;
            eventTrees[currentTreeIndex].events[eventIndex].calibrated = true;
        }

        if (currentEvent.eventTriggers.triggerRadius > 0) {
            Collider[] temp;
            int playerIsInRange = 0;

            temp = Physics.OverlapSphere(currentEvent.eventTriggers.triggerPosition, currentEvent.eventTriggers.triggerRadius);

            if (temp.Length != prevCount)
                foreach (Collider obj in temp)
                    if (obj.transform.root.tag == "Player")
                        playerIsInRange++;

            if (!(playerIsInRange > 0))
                return;

            prevCount = temp.Length;
        }

        foreach (GameObject toCheck in currentEvent.eventTriggers.checkIfDestroyed)
            if (toCheck)
                return;

        if (eventTrees[currentTreeIndex].events[eventIndex].eventTriggers.timer > Time.time) {
            return;
        }

        if (currentEvent.eventTriggers.key != KeyCode.None)
            if (!(Input.GetKey(currentEvent.eventTriggers.key)))
                return;

        eventTrees[currentTreeIndex].currentGameEvent++;
        ActivateEvent(currentEvent.results);
    }

    void ActivateEvent(Triggered endResult) {
        foreach (GameObject spawn in endResult.spawns)
            spawn.SetActive(true);

        foreach (GameObject destroy in endResult.toDestroy)
            Destroy(destroy);

        foreach (EventResults results in endResult.scriptedEventsToTrigger)
            results.ScriptedResult();

        if (endResult.debugMessage != "")
            Debug.Log(endResult.debugMessage);

        if (endResult.levelNameToLoad != "") {
            SceneManager.LoadScene(endResult.levelNameToLoad);
        }

        currentTreeIndex = endResult.eventTreeToJump;
    }

    public void CleanUpEventTreeToJump() {
        for (var i = 0; i < eventTrees.Length; i++)
            for (var j = 0; j < eventTrees[i].events.Length; j++)
                if (j < eventTrees[i].events.Length - 1)
                    eventTrees[i].events[j].results.eventTreeToJump = i;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EventManager))]
public class EventManagerEditor : Editor {
    EventManager t;
    int currentTree;
    int currentEvent;

    void OnSceneGUI() {
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(90, 0, 0);
        Handles.color = Color.red;

        if (t != null)
            for (var i = 0; i < t.eventTrees.Length; i++) {
                //Handles.color = t.eventTrees[i].eventColors;
                for (var j = 0; j < t.eventTrees[i].events.Length; j++)
                    Handles.CircleCap(0, t.eventTrees[i].events[j].eventTriggers.triggerPosition, rotation, t.eventTrees[i].events[j].eventTriggers.triggerRadius);
            }


        Event e;
        e = Event.current;

        if (e.type == EventType.keyDown) {
            if (e.keyCode == KeyCode.Q) {
                RaycastHit hit;

                Vector2 temp = e.mousePosition;
                temp.y = Screen.height - e.mousePosition.y;
                Ray ray = Camera.current.ScreenPointToRay(temp);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    t.eventTrees[currentTree].events[currentEvent].eventTriggers.triggerPosition = hit.point;
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        t = target as EventManager;
        if (t != null)
            for (var i = 0; i < t.eventTrees.Length; i++)
                for (var j = 0; j < t.eventTrees[i].events.Length; j++) {
                    string buttonName = "Set trigger radius ";

                    if (t.eventTrees[i].events[j].eventName != "")
                        buttonName = buttonName + "(" + t.eventTrees[i].eventTreeName + ") " + t.eventTrees[i].events[j].eventName;
                    else
                        buttonName = buttonName + i.ToString();

                    if (GUILayout.Button(buttonName)) {
                        SceneView sceneView = SceneView.sceneViews[0] as SceneView;
                        sceneView.Focus();
                        currentTree = i;
                        currentEvent = j;
                    }
                }

        if (GUILayout.Button("Clean up Event Tree To Jump"))
            t.CleanUpEventTreeToJump();
    }
}
#endif