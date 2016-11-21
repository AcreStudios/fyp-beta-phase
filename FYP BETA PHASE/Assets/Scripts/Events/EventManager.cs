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
        //public bool treeSupercedeAll;
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

        public EventBody[] eventBody;
        [HideInInspector]
        public bool calibrated;
    }

    [System.Serializable]
    public struct EventBody {
        public string eventBodyName;
        public Triggers eventTriggers;
        public Results results;
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
    public struct Results {
        public GameObject[] spawns;
        public GameObject[] toDestroy;
        //public EventResults[] scriptedEventsToTrigger;
        public int eventTreeToJump;
        public string debugMessage;
        public string levelNameToLoad;
        public bool unableToProceed;
    }

    public EventTree[] eventTrees;

    public int currentTreeIndex;
    public Text missionUI;


    void Awake() {
    }

    void Update() {
        if (Application.isPlaying) {
            if (eventTrees[currentTreeIndex].currentGameEvent < eventTrees[currentTreeIndex].events.Length) {
                if (missionUI)
                    missionUI.text = eventTrees[currentTreeIndex].events[eventTrees[currentTreeIndex].currentGameEvent].missionUI;
                foreach (EventBody eventB in eventTrees[currentTreeIndex].events[eventTrees[currentTreeIndex].currentGameEvent].eventBody)
                    CheckTrigger(eventB, currentTreeIndex, eventTrees[currentTreeIndex].currentGameEvent);
            }
        } else {
            for (var i = 0; i < eventTrees.Length; i++)
                for (var j = 0; j < eventTrees[i].events.Length; j++) {
                    for (var k = 0; k < eventTrees[i].events[j].eventBody.Length; k++) {
                        for (var l = 0; l < eventTrees[i].events[j].eventBody[k].results.spawns.Length; l++)
                            if (eventTrees[i].events[j].eventBody[k].results.spawns[l] != null)
                                eventTrees[i].events[j].eventBody[k].results.spawns[l].SetActive(false);
                    }
                }
        }
    }

    void CheckTrigger(EventBody currentEvent, int currentTreeIndex, int eventIndex) {
        //Debug.Log(eventTrees[currentTreeIndex].events[eventIndex].eventName);

        if (!eventTrees[currentTreeIndex].events[eventIndex].calibrated) {
            for (var i = 0; i < eventTrees[currentTreeIndex].events[eventIndex].eventBody.Length; i++)
                eventTrees[currentTreeIndex].events[eventIndex].eventBody[i].eventTriggers.timer += Time.time;
            eventTrees[currentTreeIndex].events[eventIndex].calibrated = true;
        }

        if (currentEvent.eventTriggers.triggerRadius > 0) {
            Collider[] temp;
            int playerIsInRange = 0;
            temp = Physics.OverlapSphere(currentEvent.eventTriggers.triggerPosition, currentEvent.eventTriggers.triggerRadius);

            foreach (Collider obj in temp)
                if (obj.transform.root.tag == "Player")
                    playerIsInRange++;

            if (!(playerIsInRange > 0))
                return;
        }

        foreach (GameObject toCheck in currentEvent.eventTriggers.checkIfDestroyed)
            if (toCheck)
                return;

        if (currentEvent.eventTriggers.timer > Time.time) {
            return;
        }

        if (currentEvent.eventTriggers.key != KeyCode.None)
            if (!(Input.GetKey(currentEvent.eventTriggers.key)))
                return;

        ActivateEvent(currentEvent.results);
    }

    void ActivateEvent(Results endResult) {
        foreach (GameObject spawn in endResult.spawns)
            spawn.SetActive(true);

        foreach (GameObject destroy in endResult.toDestroy)
            Destroy(destroy);

        //foreach (EventResults results in endResult.scriptedEventsToTrigger)
        //results.ScriptedResult();

        if (endResult.debugMessage != "")
            Debug.Log(endResult.debugMessage);

        if (endResult.levelNameToLoad != "")
            SceneManager.LoadScene(endResult.levelNameToLoad);

        if (!endResult.unableToProceed) {
            eventTrees[currentTreeIndex].currentGameEvent++;
            currentTreeIndex = endResult.eventTreeToJump;
        }
    }

    public void CleanUpEventTreeToJump() {
        for (var i = 0; i < eventTrees.Length; i++)
            for (var j = 0; j < eventTrees[i].events.Length; j++)
                if (j < eventTrees[i].events.Length - 1)
                    for (var k = 0; k < eventTrees[i].events[j].eventBody.Length; k++)
                        eventTrees[i].events[j].eventBody[k].results.eventTreeToJump = i;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EventManager))]
public class EventManagerEditor : Editor {
    EventManager t;
    int currentTree;
    int currentEvent;
    int currentTrigger;

    void OnSceneGUI() {
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(90, 0, 0);
        Handles.color = Color.red;

        if (t != null)
            for (var i = 0; i < t.eventTrees.Length; i++) {
                //Handles.color = t.eventTrees[i].eventColors;
                for (var j = 0; j < t.eventTrees[i].events.Length; j++)
                    for (var k = 0; k < t.eventTrees[i].events[j].eventBody.Length; k++)
                        Handles.CircleCap(0, t.eventTrees[i].events[j].eventBody[k].eventTriggers.triggerPosition, rotation, t.eventTrees[i].events[j].eventBody[k].eventTriggers.triggerRadius);
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
                    t.eventTrees[currentTree].events[currentEvent].eventBody[currentTrigger].eventTriggers.triggerPosition = hit.point;
                    t.eventTrees[currentTree].events[currentEvent].eventBody[currentTrigger].eventTriggers.triggerRadius = 1;
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        t = target as EventManager;

        if (t != null)
            for (var i = 0; i < t.eventTrees.Length; i++)
                for (var j = 0; j < t.eventTrees[i].events.Length; j++)
                    for (var k = 0; k < t.eventTrees[i].events[j].eventBody.Length; k++) {
                        string buttonName = "Set trigger radius ";

                        if (t.eventTrees[i].events[j].eventName != "")
                            buttonName = buttonName + "(" + t.eventTrees[i].eventTreeName + ") " + t.eventTrees[i].events[j].eventName + " [" + t.eventTrees[i].events[j].eventBody[k].eventBodyName + "]";
                        else
                            buttonName = buttonName + i.ToString();

                        if (GUILayout.Button(buttonName)) {
                            SceneView sceneView = SceneView.sceneViews[0] as SceneView;
                            sceneView.Focus();
                            currentTree = i;
                            currentEvent = j;
                            currentTrigger = k;
                        }
                    }

        if (GUILayout.Button("Clean up Event Tree To Jump"))
            t.CleanUpEventTreeToJump();
    }
}
#endif