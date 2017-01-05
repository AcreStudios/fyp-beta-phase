using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour 
{
	public enum HealthRegen { ONESHOT, OVERTIME }

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
	public UnityEngine.UI.Image BloodSpatterImage;
	public UnityEngine.UI.Image RedTintImage;
	public UnityEngine.Sprite[] BloodSpatterSprites;
	[Range(0f, 1f)]
	public float BloodSplatterFlashTransparency = .75f, RedTintFlashTransparency = 1f, RedTintStayIntensity = .5f;
	public float DamageFlashSpeed = 3f;

	private Color currentRedTintTransparency;

	[Header("-Health Regeneration-")]
	public HealthRegen HealthRegenMode;

	[Header("One Shot")]
	public float TriggerOneShotRegenDelay = 5f;
	public float OneShotRecoverSpeed = 40f;

	private float triggerOneShotRegenTimer;
	private bool oneShotRegenerating = false;

	[Header("Over Time")]
	public float OverTimeRecoverSpeed = 10f;
	[Range(0f, 1f)]
	public float BaseOverTimeRedTintFlash = .5f;


	void Awake()
	{
		characterController = GetComponent<CharacterController>();
		ragdollHandler = GetComponentInChildren<RagdollHandler>();

		// Initialise
		currentRedTintTransparency = new Color(1f, 1f, 1f, 0f);
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

		// Update damage feedback
		LerpTargetColor();

		// Health regeneration
		curHealth = Mathf.Clamp(curHealth, 0f, 100f);

		switch(HealthRegenMode)
		{
			case HealthRegen.ONESHOT:
				OneShotHealthRegen();
				break;
			case HealthRegen.OVERTIME:
				OverTimeRegen();
				break;
		}
	}

	private void LerpTargetColor() // Continuosly lerp to target color 
	{
		if(!BloodSpatterImage || !RedTintImage) return;

		currentRedTintTransparency.a = 0f + (1f - (curHealth / 100f)) * RedTintStayIntensity;


		BloodSpatterImage.color = Color.Lerp(BloodSpatterImage.color, Color.clear, DamageFlashSpeed * Time.deltaTime);
		RedTintImage.color = Color.Lerp(RedTintImage.color, currentRedTintTransparency, DamageFlashSpeed * Time.deltaTime);
	}

	private void OneShotHealthRegen()
	{
		// If health is not full
		if(curHealth > 0 && curHealth < 100f)
		{
			// Run timer
			if(triggerOneShotRegenTimer > 0f)
				triggerOneShotRegenTimer -= Time.deltaTime;

			// Check timer
			if(triggerOneShotRegenTimer <= 0f && !oneShotRegenerating)
				StartCoroutine(RegenToFullHealth());
		}
	}

	private IEnumerator RegenToFullHealth() // One shot regen to full health 
	{
		oneShotRegenerating = true;

		while(curHealth < 100f)
		{
			curHealth += OneShotRecoverSpeed * Time.deltaTime;
			yield return null;
		}

		curHealth = 100f;
		oneShotRegenerating = false;
	}

	private void OverTimeRegen()
	{
		// If health is not full
		if(curHealth > 0 && curHealth < 100f)
			curHealth += OverTimeRecoverSpeed * Time.deltaTime;
	}

	public void ReceiveDamage(float dmg = 15f)
	{
		if(oneShotRegenerating) return;

		curHealth -= dmg;

		// Damage feedback
		FlashScreenOnDamage();

		// Reset oneshot regeneration timer
		triggerOneShotRegenTimer = TriggerOneShotRegenDelay;

		// If no health, die
		if(curHealth <= 0)
			Die();
	}

	private void FlashScreenOnDamage()
	{
		if(BloodSpatterSprites.Length == 0 || !BloodSpatterImage || !RedTintImage) return;

		// Blood Splatter
		BloodSpatterImage.sprite = BloodSpatterSprites[Random.Range(0, BloodSpatterSprites.Length)];
		BloodSpatterImage.color = new Color(1f, 1f, 1f, BloodSplatterFlashTransparency);

		// Red Tint
		switch(HealthRegenMode)
		{
			case HealthRegen.ONESHOT:
				RedTintImage.color = new Color(1f, 1f, 1f, RedTintFlashTransparency);
				break;
			case HealthRegen.OVERTIME:
				RedTintImage.color = new Color(1f, 1f, 1f, (Mathf.Abs(((curHealth / 100f) - 1f)) * .8f) + .5f);
				break;
		}
	}

	private void Die()
	{
		characterController.enabled = false;
		BloodSpatterImage.color = new Color(0f, 0f, 0f, 0f);
		RedTintImage.color = new Color(0f, 0f, 0f, 0f);

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
