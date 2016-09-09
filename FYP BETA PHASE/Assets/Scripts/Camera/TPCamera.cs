using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TPCamera : MonoBehaviour 
{
	// Components and transforms
	private Transform trans;
	private Transform camPivot;
	private Transform camTrans;

	[Header("-Inputs-")]
	public float _mouseX;
	public float _mouseY;
	public bool _RMB;

	[System.Serializable]
	public class InputStrings
	{
		public string mouseAxisX = "Mouse X";
		public string mouseAxisY = "Mouse Y";
		public string aimButton = "Fire2";
	}
	[SerializeField]
	private InputStrings inputStrings;

	[System.Serializable]
	public class CameraSettings
	{
		[Header("-Positioning-")]
		public Vector3 camPosOffsetLeft = new Vector3(-.75f, .5f, -2f);
		public Vector3 camPosOffsetRight = new Vector3(.75f, .5f, -2f);
		public enum Shoulder { RIGHT, LEFT }
		public Shoulder shoulder;

		[Header("-Camera Options-")]
		public float mouseSensitivityX = 2.5f;
		public float mouseSensitivityY = 2.5f;
		public float minAngle = 30f;
		public float maxAngle = 60f;
		public float cameraRotateSpeed = 5f;
		public float cameraMoveSpeed = 5f;
		public float cameraSmoothing = .05f;
		public bool invertY = true;

		[Header("-FOV Settings-")]
		public float defaultFOV = 70f;
		public float aimingFOV = 30f;
		public float zoomSpeed = 10f;

		public Camera mainCam;
		public Camera UICam;
		public Camera visionCam;

		[Header("-Visual Options-")]
		public float wallCheckDist = .1f;
		public float hideMeshDistance = .5f;
		public LayerMask wallLayer;
		[HideInInspector]
		public SkinnedMeshRenderer[] meshes;
		[HideInInspector]
		public MeshRenderer[] wpnMeshes;
	}
	[SerializeField]
	public CameraSettings cameraSettings;

	[Header("-Camera Target-")]
	public Transform target;
	public bool autoTargetPlayer = true;

	// Camera helpers
	private Vector3 _velocity;
	private float _newX, _newY = 0f;

	// Singleton
	public static TPCamera instance;
	public static TPCamera GetInstance()
	{
		return instance;
	}


	void Awake()
	{
		// Implement singleton
		instance = this;
		
		// Cache components and transforms
		trans = GetComponent<Transform>();
		camPivot = trans.GetChild(0);
		camTrans = camPivot.GetChild(0);
	}

	void Start() 
	{
		ParentAndResetMainCamera();
		TargetPlayer();

		// Cache main camera for changing FOV
		cameraSettings.mainCam = Camera.main;

		// Cache all meshes to hide when camera is near, AFTER getting target
		cameraSettings.meshes = target.GetComponentsInChildren<SkinnedMeshRenderer>();
		cameraSettings.wpnMeshes = target.GetComponentsInChildren<MeshRenderer>();
	}

	void Update() 
	{
		HandleInput();
		RotateCamera();
		CameraWallCollision();
		CheckMeshDistance();
		ChangeFOV(_RMB);
	}

	void LateUpdate()
	{
		Vector3 targetPos = target.position;
		FollowTarget(targetPos);
	}

	private void FollowTarget(Vector3 targetPos) // Follow the target smoothly
	{
		Vector3 newPos = Vector3.SmoothDamp(trans.position, targetPos, ref _velocity, cameraSettings.cameraSmoothing);
		trans.position = newPos;
	}

	private void HandleInput()
	{
		_mouseX = Input.GetAxis(inputStrings.mouseAxisX);
		_mouseY = Input.GetAxis(inputStrings.mouseAxisY);
		_RMB = Input.GetButton(inputStrings.aimButton);
	}

	private void RotateCamera() // Rotate the camera with input
	{
		// Get mouse movement
		_newX += cameraSettings.mouseSensitivityX * _mouseX;
		_newY += (cameraSettings.invertY) ? cameraSettings.mouseSensitivityY * _mouseY * -1f : cameraSettings.mouseSensitivityY * _mouseY;

		// Clamping
		_newX = Mathf.Repeat(_newX, 360f);
		_newY = Mathf.Clamp(_newY, -Mathf.Abs(cameraSettings.minAngle), cameraSettings.maxAngle);

		// Rotation
		Vector3 eulerAngleAxis = new Vector3(_newY, _newX);
		Quaternion newRotation = Quaternion.Slerp(camPivot.localRotation, Quaternion.Euler(eulerAngleAxis), cameraSettings.cameraRotateSpeed * Time.deltaTime);
		camPivot.localRotation = newRotation;
	}

	private void CameraWallCollision() // Spherecast to prevent collision with walls and also switch shoulders
	{
		Transform mainCamTrans = camTrans;
		Vector3 mainCamPos = mainCamTrans.position;
		Vector3 pivotPos = camPivot.position;

		// Do spherecast
		RaycastHit hit;
		Vector3 start = pivotPos;
		Vector3 dir = mainCamPos - pivotPos;
		float dist = Mathf.Abs(cameraSettings.shoulder == CameraSettings.Shoulder.LEFT ? cameraSettings.camPosOffsetLeft.z : cameraSettings.camPosOffsetRight.z);
		if(Physics.SphereCast(start, cameraSettings.wallCheckDist, dir, out hit, dist, cameraSettings.wallLayer))
			RepositionCamera(hit, pivotPos, dir, mainCamTrans);
		else
		{
			switch(cameraSettings.shoulder)
			{
				case CameraSettings.Shoulder.LEFT:
					PositionCamera(cameraSettings.camPosOffsetLeft);
					break;
				case CameraSettings.Shoulder.RIGHT:
					PositionCamera(cameraSettings.camPosOffsetRight);
					break;
			}
		}
	}

	private void RepositionCamera(RaycastHit hit, Vector3 pivotPos, Vector3 dir, Transform mainCamTrans) // Moves camera forward when we hit a wall
	{
		float hitDist = hit.distance;
		Vector3 sphereCastCenter = pivotPos + (dir.normalized * hitDist);
		mainCamTrans.position = sphereCastCenter;
	}

	private void PositionCamera(Vector3 camPos) // Position camera's localPosition to a given location
	{
		Transform mainCamTrans = camTrans;
		Vector3 mainCamPos = mainCamTrans.localPosition;

		Vector3 newPos = Vector3.Lerp(mainCamPos, camPos, cameraSettings.cameraMoveSpeed * Time.deltaTime);
		mainCamTrans.localPosition = newPos;
	}

	private void CheckMeshDistance() // Hide the meshes if within set distance
	{
		Transform mainCamTrans = camTrans;
		Vector3 mainCamPos = mainCamTrans.position;
		Vector3 targetPos = target.position;
		float dist = Vector3.Distance(mainCamPos, (targetPos + target.up));

		// Check model meshes
		if(cameraSettings.meshes.Length > 0)
		{
			for(int i = 0; i < cameraSettings.meshes.Length; i++)
			{
				cameraSettings.meshes[i].enabled = (dist < cameraSettings.hideMeshDistance) ? false : true;
			}
		}

		// Check weapon meshes
		if(cameraSettings.wpnMeshes.Length > 0)
		{
			for(int i = 0; i < cameraSettings.wpnMeshes.Length; i++)
			{
				cameraSettings.wpnMeshes[i].enabled = (dist < cameraSettings.hideMeshDistance) ? false : true;
			}
		}
	}

	private void ChangeFOV(bool aim) // Switch camera FOV for aiming
	{
		float targetFOV = (aim) ? cameraSettings.aimingFOV : cameraSettings.defaultFOV;

		float newFOV = Mathf.Lerp(cameraSettings.mainCam.fieldOfView, targetFOV, cameraSettings.zoomSpeed * Time.deltaTime);
		cameraSettings.mainCam.fieldOfView = newFOV;

		// Other cameras
		if(cameraSettings.UICam)
			cameraSettings.UICam.fieldOfView = newFOV;

		if(cameraSettings.visionCam)
			cameraSettings.visionCam.fieldOfView = newFOV;
	}

	public void SwitchShoulder() // Change shoulder side
	{
		switch(cameraSettings.shoulder)
		{
			case CameraSettings.Shoulder.LEFT:
				cameraSettings.shoulder = CameraSettings.Shoulder.RIGHT;
				target.GetComponent<Animator>().SetBool("idleRight", false); // <<< Cache it later
				target.GetComponent<PlayerInput>().mirrorInt = 1;
				break;
			case CameraSettings.Shoulder.RIGHT:
				cameraSettings.shoulder = CameraSettings.Shoulder.LEFT;
				target.GetComponent<Animator>().SetBool("idleRight", true); // <<< Cache it later
				target.GetComponent<PlayerInput>().mirrorInt = -1;
				break;
		}
	}

	public void ParentAndResetMainCamera() // Setup main camera in any scene as TP camera
	{
		Transform mainCamTrans = Camera.main.transform;
		mainCamTrans.parent = camTrans;
		mainCamTrans.localPosition = Vector3.zero;
		mainCamTrans.localRotation = Quaternion.Euler(Vector3.zero);
	}

	private void TargetPlayer() // Finds player and set it as target
	{
		if(target || !autoTargetPlayer)
			return;

		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if(player)
			target = player.transform;
		else
			Debug.LogError("There is no GameObject tagged as Player in the scene found!");
	}
}
