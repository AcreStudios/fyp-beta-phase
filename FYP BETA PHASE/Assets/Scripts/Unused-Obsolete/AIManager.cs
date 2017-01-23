using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

    public static AIManager instance;
    public ColliderReaderModule readerInst;
    public Transform player;
    public AudioClip[] sfxList;
    //public ObstaclesData[] obstacles;

    void Start() {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void PlayRandomSound(Vector3 position) {
        if (sfxList.Length > 0)
            SoundManager.instance.PlaySoundOnce(position, sfxList[Random.Range(0, sfxList.Length)]);
    }
}

