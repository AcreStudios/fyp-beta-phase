using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterMovement))]
public class PlayerInput : MonoBehaviour 
{
	// Components
	private Transform trans;
	private CharacterMovement charMove;
	private WeaponHandler wpnHandler;
	private Transform mainCamTrans;
	private TPCamera tpCamera;
	private CoverSystem coverSystem;

	[Header("-Inputs-"), Range(-1f, 1f)]
	public float _horizontal;
	[Range(-1f, 1f)]
	public float _vertical;
	public bool _LMB, _RMB, _MMB, _spacebar, _leftCtrl, _keyE, _leftShift;

	private float _targetH;
	private float _targetV;

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
		public string coverButton = "Cover";
		public string pickupButton = "Pickup";
		public string toggleRunButton = "ToggleRun";
	}
	[SerializeField]
	private InputStrings inputStrings;

	[Header("-Aiming-")]
	public bool debugAim = false;

	private bool _aiming = false;

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

	[Header("-Movement-")]
	public float walkRunTransitionSpeed = 10f;

	// Movement mirror helper
	[HideInInspector]
	public int mirrorInt = 1;


	void Awake()
	{
		// Cache components
		trans = GetComponent<Transform>();
		charMove = GetComponent<CharacterMovement>();
		wpnHandler = GetComponent<WeaponHandler>();
		coverSystem = GetComponent<CoverSystem>();
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
		HandleInputs();
		CharacterLogic();
		CameraAimLogic();
		WeaponLogic();
	}

	void LateUpdate()
	{
		if(wpnHandler.activeWeapon)
		{
			if(_aiming)
				RotateSpine();
		}
	}

	private void HandleInputs() 
	{
		_horizontal = Input.GetAxis(inputStrings.horizontalAxis);
		_vertical = Input.GetAxis(inputStrings.verticalAxis);
		_LMB = Input.GetButton(inputStrings.fireButton);
		_RMB = Input.GetButton(inputStrings.aimButton);
		_MMB = Input.GetButtonDown(inputStrings.switchShoulderButton);
		_spacebar = Input.GetButtonDown(inputStrings.jumpButton);
		_leftCtrl = Input.GetButtonDown(inputStrings.coverButton);
		_keyE = Input.GetButtonDown(inputStrings.pickupButton);
		_leftShift = Input.GetButton(inputStrings.toggleRunButton);
	}

	private void CharacterLogic() // Handles character logic 
	{
		// Default walk movement, with clamping when walking backwards
		float v = (_leftShift) ? Mathf.Clamp(_vertical, -.5f, 1f) : Mathf.Clamp(_vertical, -.5f, .5f);
		float h = (coverSystem.GetCoverStatus()) ? _horizontal : ((_leftShift) ? ((_vertical < 0) ? Mathf.Clamp(_horizontal, -.5f, .5f) : _horizontal) : Mathf.Clamp(_horizontal, -.5f, .5f));

		// Mirror movement
		if(_MMB)
			tpCamera.SwitchShoulder();
		 h *= mirrorInt;

		if(!_aiming)
			charMove.AnimateCharacter(_targetV, _targetH);
		else
			charMove.AnimateCharacter(_targetV * .49f, _targetH * .49f);

		// Jump
		if(_spacebar)
			charMove.DoJump();

		// Cover
		if(_leftCtrl)
			charMove.DoCrouch();

		// Lerp walk-run transition
		_targetV = Mathf.Lerp(_targetV, v, walkRunTransitionSpeed * Time.deltaTime);
		_targetH = Mathf.Lerp(_targetH, h, walkRunTransitionSpeed * Time.deltaTime);
	}

	private void CameraAimLogic() // Handles camera logic when aiming 
	{
		// Auto turn when aiming
		aimSettings.requireMovementToTurn = !_aiming;
		
		// Auto turn when not aiming?
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
		if(coverSystem.GetCoverStatus())
			return;

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
		if(!wpnHandler)
			return;
		
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

		if(!wpnHandler.activeWeapon) // If no weapon equipped
		{
		//	TurnOffAllCrosshairs();
			return;
		}

		#region Debug aim

		if(debugAim && wpnHandler.activeWeapon)
		{
			Transform bPoint = wpnHandler.activeWeapon.weaponSettings.bulletSpawnPoint;
			Vector3 start = bPoint.position;
			Vector3 dir = bPoint.forward;
			Debug.DrawRay(start, dir, Color.red);
		}

		#endregion

		#region Fire

		if(_aiming && _LMB)
			wpnHandler.FireCurrentWeapon(mainCamTrans);

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

	private void RotateSpine() // Helps the character to look at target, offsetting by camera 
	{
		if(!aimSettings.spine)
			return;

		Vector3 mainCamPos = mainCamTrans.position;
		Vector3 dir = mainCamTrans.forward;
		Ray ray = new Ray(mainCamPos, dir);
		aimSettings.spine.LookAt(ray.GetPoint(50f));

		Vector3 eulerAngleOffset = new Vector3();

		switch(tpCamera.cameraSettings.shoulder)
		{
			case TPCamera.CameraSettings.Shoulder.RIGHT:
				eulerAngleOffset = wpnHandler.activeWeapon.modelSettings.spineRotationRight;
				break;
			case TPCamera.CameraSettings.Shoulder.LEFT:
				eulerAngleOffset = wpnHandler.activeWeapon.modelSettings.spineRotationLeft;
				break;
		}

		aimSettings.spine.Rotate(eulerAngleOffset);
	}
}
