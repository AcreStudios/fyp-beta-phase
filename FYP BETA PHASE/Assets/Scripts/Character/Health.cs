using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Health : MonoBehaviour 
{
	// Components
	private CharacterController characterController;
	private RagdollHandler ragdollHandler;

	[Header("-Health-")]
	[Range(0f, 100f)]
	public float curHealth = 100f;

	[Header("-Death-")]
	public MonoBehaviour[] scriptsToDisable;


	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
		ragdollHandler = GetComponentInChildren<RagdollHandler>();
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

		if(scriptsToDisable.Length == 0)
			return;

		foreach(MonoBehaviour script in scriptsToDisable)
		{
			script.enabled = false;
		}
	}
}
