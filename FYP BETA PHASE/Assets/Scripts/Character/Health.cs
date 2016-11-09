using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour 
{
	// Components
	private CharacterController characterController;
	private RagdollHandler ragdollHandler;

	[Header("-Debug-")]
	public bool debugDeath = false;

	[Header("-Health-")]
	[Range(0f, 100f)]
	public float curHealth = 100f;

	[Header("-Death-")]
	public MonoBehaviour[] scriptsToDisable;
    public bool destroyOnDeath;

	[Header("-Only Applies To Player-")]
	public bool isPlayer = false;
	public UnityStandardAssets.ImageEffects.Grayscale grayScreenEffect;
	public float fadeScreenDelay = 3f;
	public float fadeSpeed = .1f;
	public float reloadSceneDelay = 1f;
	public int sceneToLoadInt;
	public string sceneToLoadName;


	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
		ragdollHandler = GetComponentInChildren<RagdollHandler>();
	}

	void Update() 
	{
		if(debugDeath)
		{
			debugDeath = false;

			curHealth = 0f;
			Die();
		}
	}

	public void ReceiveDamage(float dmg)
	{
		curHealth -= dmg;

		// If no health, die
		if(curHealth <= 0)
			Die();
	}

	private void Die()
	{
		characterController.enabled = false;

		if(ragdollHandler)
			ragdollHandler.BecomeRagdoll();

        if (destroyOnDeath)
            StartCoroutine(Destroy());

		if(isPlayer && grayScreenEffect)
			StartCoroutine(ScreenFadeGray());

        if(scriptsToDisable.Length == 0)
			return;

		foreach(MonoBehaviour script in scriptsToDisable)
		{
			script.enabled = false;
		}
	}

	private IEnumerator ScreenFadeGray()
	{
		yield return new WaitForSeconds(fadeScreenDelay);

		grayScreenEffect.enabled = true;
		float lerp = 0f;

		while(lerp < 1f)
		{
			lerp += fadeSpeed * Time.deltaTime;
			grayScreenEffect.rampOffset = lerp;

			yield return null;
		}

		lerp = 1f;
		grayScreenEffect.rampOffset = lerp;

		yield return new WaitForSeconds(reloadSceneDelay);

		if(sceneToLoadInt != 0)
			SceneManager.LoadScene(sceneToLoadInt);
		else if(sceneToLoadName != null)
			SceneManager.LoadScene(sceneToLoadName);
		else
			SceneManager.LoadScene(2);
	}

    IEnumerator Destroy() {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
        //Debug.LogWarning("Ded");
    }
}
