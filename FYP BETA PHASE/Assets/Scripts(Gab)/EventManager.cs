using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode]
public class EventManager : MonoBehaviour {

    [System.Serializable]
    public struct Events {
        public string eventName;
        public string missionUI;
        public Triggers eventTriggers;
        public Triggered results;
    }

    [System.Serializable]
    public struct Triggers {
        public Vector3 triggerPosition;
        public float triggerRadius;
        public Collider toCalculate;
        public GameObject checkIfDestroyed;
    }

    [System.Serializable]
    public struct Triggered {
        public GameObject[] spawns;
        public GameObject[] toDestroy;
        public EventResults[] scriptedEventsToTrigger;
    }

    [System.Serializable]
    public struct AlternateEvents {
        public int listeningToEvent;
        public float timer;
        public int eventToJumpTo;
        public Triggered results;
    }

    public Events[] gameEventFlow;
    public AlternateEvents[] alternateEventListeners;
    public Text missionUI;
    public bool ableToEdit;

    int currentGameEvent;
    int currentAltEvent;
    int prevCount;
    bool eventTriggered;
    float timer;

    void Start() {
        currentGameEvent = 0;
        currentAltEvent = 0;
        eventTriggered = false;
    }

    void Update() {

        if (ableToEdit) {
            for (var i = 0; i < gameEventFlow.Length; i++) {
                if (gameEventFlow[i].eventTriggers.toCalculate != null) {
                    gameEventFlow[i].eventTriggers.triggerPosition = gameEventFlow[i].eventTriggers.toCalculate.bounds.center;
                    gameEventFlow[i].eventTriggers.triggerRadius = gameEventFlow[i].eventTriggers.toCalculate.bounds.extents.x;
                }
                for (var j = 0; j < gameEventFlow[i].results.spawns.Length; j++) {
                    if (gameEventFlow[i].results.spawns[j] != null) {
                        gameEventFlow[i].results.spawns[j].SetActive(false);
                    }
                }

                for (var j = 0; j < alternateEventListeners[i].results.spawns.Length; j++) {
                    if (alternateEventListeners[i].results.spawns[j] != null) {
                        alternateEventListeners[i].results.spawns[j].SetActive(false);
                    }
                }
            }
        }

        if (Application.isPlaying) {
            if (currentGameEvent < gameEventFlow.Length) {
                if (missionUI)
                    missionUI.text = gameEventFlow[currentGameEvent].missionUI;

                if (gameEventFlow[currentGameEvent].eventTriggers.triggerRadius > 0 || gameEventFlow[currentGameEvent].eventTriggers.toCalculate) {
                    Collider[] temp;

                    temp = Physics.OverlapSphere(gameEventFlow[currentGameEvent].eventTriggers.triggerPosition, gameEventFlow[currentGameEvent].eventTriggers.triggerRadius);

                    if (temp.Length != prevCount) {
                        foreach (Collider obj in temp) {
                            if (obj.tag == "Player") {
                                ActivateEvent(gameEventFlow[currentGameEvent].results);
                                currentGameEvent++;
                            }
                        }
                    }

                    prevCount = temp.Length;
                } else {
                    if (!gameEventFlow[currentGameEvent].eventTriggers.checkIfDestroyed) {
                        ActivateEvent(gameEventFlow[currentGameEvent].results);
                        currentGameEvent++;
                    }
                }
            }

            if (currentAltEvent < alternateEventListeners.Length) {
                if (alternateEventListeners[currentAltEvent].listeningToEvent == currentGameEvent) {
                    if (!eventTriggered) {
                        eventTriggered = true;
                        timer = Time.time + alternateEventListeners[currentAltEvent].timer;
                    }

                    if (timer < Time.time) {
                        currentGameEvent = alternateEventListeners[currentAltEvent].eventToJumpTo;
                        ActivateEvent(alternateEventListeners[currentAltEvent].results);
                        eventTriggered = false;
                        currentAltEvent++;
                    }
                }
            }
        }
    }

    void ActivateEvent(Triggered endResult) {
        foreach (GameObject spawn in endResult.spawns)
            spawn.SetActive(true);

        foreach (GameObject destroy in endResult.toDestroy)
            Destroy(destroy);

        foreach (EventResults results in endResult.scriptedEventsToTrigger)
            results.ScriptedResult();
    }
}

[CustomEditor(typeof(EventManager))]
public class EventManagerEditor : Editor {
    EventManager t;
    int currentEvent;

    void OnSceneGUI() {
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(90, 0, 0);
        Handles.color = Color.red;

        if (t != null)
            if (t.ableToEdit)
                for (var i = 0; i < t.gameEventFlow.Length; i++)
                    Handles.CircleCap(0, t.gameEventFlow[i].eventTriggers.triggerPosition, rotation, t.gameEventFlow[i].eventTriggers.triggerRadius);


        Event e;
        e = Event.current;

        if (e.type == EventType.keyDown) {
            if (e.keyCode == KeyCode.Q) {
                RaycastHit hit;

                Vector2 temp = e.mousePosition;
                temp.y = Screen.height - e.mousePosition.y;
                Ray ray = Camera.current.ScreenPointToRay(temp);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                    //Debug.DrawLine(Vector3.zero, hit.point, Color.black);
                    t.gameEventFlow[currentEvent].eventTriggers.triggerPosition = hit.point;
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        t = target as EventManager;
        if (t != null)
            if (t.ableToEdit)
                if (t.gameEventFlow.Length > 0)
                    for (var i = 0; i < t.gameEventFlow.Length; i++) {
                        if (GUILayout.Button("Set trigger radius " + i.ToString())) {
                            currentEvent = i;
                            SceneView sceneView = SceneView.sceneViews[0] as SceneView;
                            sceneView.Focus();
                        }
                    }
    }
}


