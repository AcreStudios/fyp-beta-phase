using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterMovement))]
public class CoverSystem : MonoBehaviour 
{
	// Components
	private Transform trans;
	private CharacterMovement characterMove;
	private PlayerInput playerInput;
	private TPCamera tpCamera;

	[Header("-Cover Settings-")]
	public bool autoSwitchSides = true;
	public float wallDist = 1f;
	public LayerMask wallLayer;
	public float coverOffsetZ = .01f;

	// Cover helpers
	[SerializeField]
	private bool _inCover;
	public bool inCover { get { return _inCover; } set { _inCover = value; } }
	private RaycastHit _hit;
	private bool _startWallCheck = false;
	private Vector3 _coverPosition;


	void Awake()
	{
		// Cache components
		trans = GetComponent<Transform>();
		characterMove = GetComponent<CharacterMovement>();
		playerInput = GetComponent<PlayerInput>();
	}

	void Start() 
	{
		// Cache tp camera
		tpCamera = TPCamera.GetInstance();
	}

	void Update()
	{
		if(!characterMove)
			return;

		if(_inCover)
		{
			AutoSwitchSide();

			if(_startWallCheck)
				ContinueCheckWall();

			characterMove.GetInCover();
		}
		else
		{
			characterMove.GetOutCover();
			_inCover = false;
			_startWallCheck = false;
		}
	}

	public void CheckNearestWall() // Initial raycast to check if we are near a wall
	{
		Debug.DrawRay(trans.position + trans.up, trans.forward, Color.blue);
		if(Physics.Raycast(trans.position + trans.up, trans.forward, out _hit, wallDist, wallLayer))
		{
			_inCover = true;
			Vector3 pos = _hit.point;
			pos.y = trans.position.y;
			trans.position = pos + (-transform.forward * coverOffsetZ);

			StartCoroutine(WaitToCheckWall());
		}
	}

	private IEnumerator WaitToCheckWall()
	{
		yield return new WaitForSeconds(.5f);
		_startWallCheck = true;
	}

	private void ContinueCheckWall()
	{
		Debug.DrawRay(trans.position + trans.up, trans.forward, Color.cyan);
		if(Physics.Raycast(trans.position + trans.up, trans.forward, out _hit, wallDist, wallLayer))
			_inCover = true;
		else
		{
			_inCover = false;
			_startWallCheck = false;
		}
	}

	private void AutoSwitchSide()
	{
		if(!autoSwitchSides)
			return;
	}
}
