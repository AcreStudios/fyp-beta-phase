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
	public bool DebugDamage = false;

	[Header("-Health-")]
	[Range(0f, 100f)]
	public float curHealth = 100f;

	[Header("-Death-")]
	public MonoBehaviour[] scriptsToDisable;
    public bool destroyOnDeath;

	[Header("------Only Applies To Player------")]
	public bool isPlayer = false;
	[Header("-Death Effect-")]
	public UnityStandardAssets.ImageEffects.Grayscale grayScreenEffect;
	public float fadeScreenDelay = 3f;
	public float fadeSpeed = .1f;
	public float reloadSceneDelay = 1f;
	public int sceneToLoadInt;
	public string sceneToLoadName;
	[Header("-Screen Flash When Damaged-")]
	public Color BloodSplatterShowColor = new Color(1f, 1f, 1f, .75f);
	public Color BloodSplatterFadeColor = new Color(1f, 1f, 1f, 0f);
	public Color ShowBloodTintColor = new Color(1f, 1f, 1f, 1f);
	public Color CurrentBloodTintColor = new Color(1f, 1f, 1f, 0f);
	public float DamageFlashSpeed = 3f;
	public UnityEngine.UI.Image CurrentBloodSpatterImage;
	public UnityEngine.UI.Image CurrentBloodTintImage;
	public UnityEngine.Sprite[] BloodSpatterSprites;

	private bool damaged = false;


	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
		ragdollHandler = GetComponentInChildren<RagdollHandler>();
	}

	void Update() 
	{
		// Debug in inspector only
		if(debugDeath)
		{
			debugDeath = false;

			curHealth = 0f;
			Die();
		}
		
		if(DebugDamage)
		{
			DebugDamage = false;

			ReceiveDamage();
		}

		// Fade out screen damage flash
		CurrentBloodSpatterImage.color = Color.Lerp(CurrentBloodSpatterImage.color, new Color(1f, 1f, 1f, 0f), DamageFlashSpeed * Time.deltaTime);
		CurrentBloodTintImage.color = Color.Lerp(CurrentBloodTintImage.color, CurrentBloodTintColor, DamageFlashSpeed * Time.deltaTime);
	}

	public void ReceiveDamage(float dmg = 5f)
	{
		curHealth -= dmg;

		FlashScreenOnDamage();

		// If no health, die
		if(curHealth <= 0)
			Die();
	}

	private void FlashScreenOnDamage()
	{
		if(BloodSpatterSprites.Length == 0) return;

		damaged = true;

		CurrentBloodSpatterImage.sprite = BloodSpatterSprites[Random.Range(0, BloodSpatterSprites.Length)];
		CurrentBloodSpatterImage.color = BloodSplatterShowColor;
		CurrentBloodTintImage.color = ShowBloodTintColor;
		CurrentBloodTintColor.a = 0f + (1f - (curHealth / 100f));
}

	private void Die()
	{
		characterController.enabled = false;
		CurrentBloodSpatterImage.color = new Color(0f, 0f, 0f, 0f);
		CurrentBloodTintImage.color = new Color(0f, 0f, 0f, 0f);

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
