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
        public float timer;
        public GameObject[] checkIfDestroyed;
        public KeyCode key;
    }

    [System.Serializable]
    public struct Triggered {
        public GameObject[] spawns;
        public GameObject[] toDestroy;
        public EventResults[] scriptedEventsToTrigger;
        public string levelNameToLoad;
    }

    [System.Serializable]
    public struct AlternateEvents {
        public int listeningToEvent;
        public float timer;
        public int eventToJumpTo;
        public Triggered results;
    }

    public Events[] gameEventFlow;
    //public AlternateEvents[] alternateEventListeners;
    public Text missionUI;
    //public bool ableToEdit;

    int currentGameEvent;
    int prevCount;
    int playerIsInRange;

    bool eventTriggered;
    float timer;

    public KeyCode test;

    void Start() {
        currentGameEvent = 0;
        playerIsInRange = 0;
        eventTriggered = false;
    }

    void Update() {
        if (Application.isPlaying) {
            //if (currentAltEvent < alternateEventListeners.Length) {
            //if (alternateEventListeners[currentAltEvent].listeningToEvent == currentGameEvent) {
            //if (!eventTriggered) {
            //  eventTriggered = true;
            //  timer = Time.time + alternateEventListeners[currentAltEvent].timer;
            // }

            //if (timer < Time.time) {
            //currentGameEvent = alternateEventListeners[currentAltEvent].eventToJumpTo;
            //ActivateEvent(alternateEventListeners[currentAltEvent].results);
            // eventTriggered = false;
            // currentAltEvent++;
            //}
            // }
            // }

            if (currentGameEvent < gameEventFlow.Length) {
                if (missionUI)
                    missionUI.text = gameEventFlow[currentGameEvent].missionUI;

                if (!eventTriggered) {
                    eventTriggered = true;
                    timer = Time.time + gameEventFlow[currentGameEvent].eventTriggers.timer;
                }

                if (gameEventFlow[currentGameEvent].eventTriggers.triggerRadius > 0) {


                    Collider[] temp;

                    temp = Physics.OverlapSphere(gameEventFlow[currentGameEvent].eventTriggers.triggerPosition, gameEventFlow[currentGameEvent].eventTriggers.triggerRadius);

                    if (temp.Length != prevCount) {
                        playerIsInRange = 0;
                        foreach (Collider obj in temp) {
                            if (obj.transform.root.tag == "Player") {
                                playerIsInRange++;
                            }
                        }
                    }

                    if (!(playerIsInRange > 0))
                        return;

                    prevCount = temp.Length;
                }

                foreach (GameObject toCheck in gameEventFlow[currentGameEvent].eventTriggers.checkIfDestroyed) {
                    if (toCheck)
                        return;
                }

                if (timer > Time.time) {
                    return;
                }

                if (gameEventFlow[currentGameEvent].eventTriggers.key != KeyCode.None)
                    if (!(Input.GetKey(gameEventFlow[currentGameEvent].eventTriggers.key)))
                        return;

                eventTriggered = false;
                ActivateEvent(gameEventFlow[currentGameEvent].results);
                currentGameEvent++;
            }
        } else {
            if (gameEventFlow.Length > 0)
                for (var i = 0; i < gameEventFlow.Length; i++) {
                    if (gameEventFlow[i].results.spawns.Length > 0)
                        for (var j = 0; j < gameEventFlow[i].results.spawns.Length; j++) {
                            if (gameEventFlow[i].results.spawns[j] != null) {
                                gameEventFlow[i].results.spawns[j].SetActive(false);
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

        if (endResult.levelNameToLoad != "")
            SceneManager.LoadScene(endResult.levelNameToLoad);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(EventManager))]
public class EventManagerEditor : Editor {
    EventManager t;
    int currentEvent;

    void OnSceneGUI() {
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(90, 0, 0);
        Handles.color = Color.red;

        if (t != null)
            if (t.gameEventFlow.Length > 0)
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
                    t.gameEventFlow[currentEvent].eventTriggers.triggerPosition = hit.point;
                }
            }
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        t = target as EventManager;
        if (t != null)
            if (t.gameEventFlow.Length > 0)
                for (var i = 0; i < t.gameEventFlow.Length; i++) {
                    string buttonName = "Set trigger radius ";

                    if (t.gameEventFlow[i].eventName != "")
                        buttonName = buttonName + t.gameEventFlow[i].eventName;
                    else
                        buttonName = buttonName + i.ToString();

                    if (GUILayout.Button(buttonName)) {
                        currentEvent = i;
                        SceneView sceneView = SceneView.sceneViews[0] as SceneView;
                        sceneView.Focus();
                    }
                }
    }
}
#endif