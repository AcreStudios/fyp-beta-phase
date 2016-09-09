﻿using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(WeaponHandler))]
[RequireComponent(typeof(CharacterMovement))]
public class PlayerInput : MonoBehaviour 
{
	// Components
	private Transform trans;
	private CharacterMovement charMove;
	private WeaponHandler wpnHandler;
	private Transform mainCamTrans;
	private TPCamera tpCamera;

	[Header("-Inputs-"), Range(-1f, 1f)]
	public float _horizontal;
	[Range(-1f, 1f)]
	public float _vertical;
	public bool _LMB, _RMB, _MMB, _spacebar;

	[System.Serializable]
	public class InputStrings
	{
		[Header("-From Input Manager-")]
		public string horizontalAxis = "Horizontal";
		public string verticalAxis = "Vertical";
		public string fireButton = "Fire1";
		public string aimButton = "Fire2";
		public string switchShoulderButton = "Fire3";
		public string jumpButton = "Jump";
		public string switchButton = "Switch";
		public string reloadButton = "Reload";
	}
	[SerializeField]
	private InputStrings inputStrings;

	[Header("-Aiming-")]
	public bool debugAim = false;

	[System.Serializable]
	public class AimSettings
	{
		[Header("-Aim Settings-")]
		public bool requireMovementToTurn = true;
		public float turnSpeed = 100f;
		public float lookDistance = 10f;
		public Transform spine;
	}
	[SerializeField]
	private AimSettings aimSettings;

	// Movement mirror helper
	[HideInInspector]
	public int mirrorInt = 1;

	// Aim helper
	private bool _aiming = false;


	void Awake()
	{
		// Cache components
		trans = GetComponent<Transform>();
		charMove = GetComponent<CharacterMovement>();
		wpnHandler = GetComponent<WeaponHandler>();
	}

	void Start() 
	{
		// Cache TP Camera
		tpCamera = TPCamera.GetInstance();
		
		// Cache camera transform
		mainCamTrans = Camera.main.transform;
	}

	void Update() 
	{
		HandleInput();
		CharacterLogic();
		CameraAimLogic();
		WeaponLogic();
	}

	void LateUpdate()
	{
		//if(wpnManager.activeWeapon)
		//{
		//	if(_aiming)
		//		RotateSpine();
		//}
	}

	private void HandleInput()
	{
		_horizontal = Input.GetAxis(inputStrings.horizontalAxis);
		_vertical = Input.GetAxis(inputStrings.verticalAxis);
		_LMB = Input.GetButton(inputStrings.fireButton);
		_RMB = Input.GetButton(inputStrings.aimButton);
		_MMB = Input.GetButtonDown(inputStrings.switchShoulderButton);
		_spacebar = Input.GetButtonDown(inputStrings.jumpButton);
	}

	private void CharacterLogic() // Handles character logic
	{
		// Movement, with clamping when walking backwards
		float v = Mathf.Clamp(_vertical, -.5f, 1f);
		float h = (_vertical < 0) ? Mathf.Clamp(_horizontal, -.5f, .5f) : _horizontal;

		// Mirror movement
		if(_MMB)
			tpCamera.SwitchShoulder();
		h *= mirrorInt;

		if(!_aiming)
			charMove.AnimateCharacter(v, h);
		else
			charMove.AnimateCharacter(v * .49f, h * .49f);

		// Jump
		if(_spacebar)
			charMove.DoJump();
	}

	private void CameraAimLogic() // Handles camera logic when aiming
	{
		// Auto turn when aiming?
		if(aimSettings.requireMovementToTurn)
		{
			if(_vertical != 0 || _horizontal != 0)
				CharacterLook();
		}
		else
			CharacterLook();
	}

	private void CharacterLook() // Make the character look at the same direction as the camera
	{
		Transform pivot = mainCamTrans.parent.parent;
		Vector3 pivotPos = pivot.position;
		Vector3 lookTarget = pivotPos + (pivot.forward * aimSettings.lookDistance);
		Vector3 thisPos = trans.position;
		Vector3 lookDir = lookTarget - thisPos;
		Quaternion lookRot = Quaternion.LookRotation(lookDir);
		lookRot.x = 0f;
		lookRot.z = 0f;

		Quaternion newRot = Quaternion.Lerp(trans.rotation, lookRot, aimSettings.turnSpeed * Time.deltaTime);
		trans.rotation = newRot;
	}

	private void WeaponLogic() // Handles all weapon logic + inputs
	{
		#region Aim

		_aiming = _RMB || debugAim;
		wpnHandler.AimWeapon(_aiming);

		#endregion

		#region Switch

		if(Input.GetButtonDown(inputStrings.switchButton))
		{
			wpnHandler.SwitchNextWeapon();
			//UpdateCrosshairs();
		}

		#endregion

		//if(!weaponHandler.activeWeapon) // If no weapon equipped
		//{
		//	TurnOffAllCrosshairs();
		//	return;
		//}

		Ray ray = new Ray(mainCamTrans.position, mainCamTrans.forward);

		#region Fire

		if(_aiming && _LMB)
			wpnHandler.FireCurrentWeapon(ray);

		#endregion

		#region Reload

		if(Input.GetButtonDown(inputStrings.reloadButton))
			wpnHandler.ReloadCurrentWeapon();

		#endregion

		#region Crosshair

		//if(isAiming)
		//{
		//	ToggleCrosshair(true, weaponHandler.currentWeapon);
		//	PositionCrosshair(ray, weaponHandler.currentWeapon);
		//}
		//else
		//	ToggleCrosshair(false, weaponHandler.currentWeapon);

		#endregion
	}

	private void RotateSpine() // Helps the character to look vertically at target
	{
		if(!aimSettings.spine)
			return;

		Vector3 mainCamPos = mainCamTrans.position;
		Vector3 dir = mainCamTrans.forward;
		Ray ray = new Ray(mainCamPos, dir);
		aimSettings.spine.LookAt(ray.GetPoint(50f));

		//Vector3 eulerAngleOffset = weaponHandler.currentWeapon.modelSettings.spineRotation;
		//aimSettings.spine.Rotate(eulerAngleOffset);
	}
}
