using UnityEngine;
using System.Collections;

public class HitFeedbackManager : MonoBehaviour {

    public SpriteRenderer sprite;
    public float alpha;
    public float rateOfDecay;
    public bool test;
    public static HitFeedbackManager instance;

    void Start() {
        instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (test) {
            test = false;
            RetriggerHitEvent();
        }

        if (alpha >= 0) {
            alpha -= rateOfDecay;
            sprite.color = new Color(1, 1, 1, alpha);
        }
    }

    public void RetriggerHitEvent() {
        alpha = 1;
        //sprite.color = new Color(1, 1, 1, 1);
    }
}
