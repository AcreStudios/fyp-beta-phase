using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterMovement))]
public class CoverSystem : MonoBehaviour 
{
	// Components
	private Transform trans;
	private PlayerInput playerInput;
	private CharacterMovement characterMove;

	[Header("-Cover Settings-")]
	public bool debugCover = true;
	public LayerMask ignoreLayer;
	public float offsetFromWall = .6f;
	public float peekDistance = .25f;
	public float minCoverSize = .2f;
	public Vector3 relativeInput;

	private GameObject _debugCube;
	[SerializeField]
	private bool _inCover;
	private CoverPosition _coverPosition;
	private Transform _helperTrans;
	private bool _movePositive;
	//private bool _crouchCover;

	

	[Header("-Before Cover-")]
	public bool autoSearchCover = true;
	public float searchDistance = 3f;

	private bool _initCover;
	private bool _initLerp;
	private float _coverPositionLength;
	private Vector3 _startPos;
	private Vector3 _targetPos;
	private float _lerpSpeed = 2f;
	private float _tLerp;

	[Header("-During Cover-")]
	public float coverMovespeed = 4f;
	public bool twoPointValidation;
	public float coverToleranceAngle = 45f;

	// States
	//private int _coverDirection;
	//private bool _crouching;
	//private bool _canAim;
	//private bool _aimAtSides;
	private bool _canManualCover = true;

	[Header("-After Cover-")]
	public float exitCoverCooldown = 1f;


	void Awake() 
	{
		// Cache
		trans = GetComponent<Transform>();
		playerInput = GetComponent<PlayerInput>();
		characterMove = GetComponent<CharacterMovement>();
	}

	void Start() 
	{
		_coverPosition = new CoverPosition();
		_helperTrans = new GameObject("Cover Helper").transform;

		if(debugCover)
		{
			_debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Vector3 s = Vector3.one * .2f;
			_debugCube.transform.localScale = s;
			_debugCube.GetComponent<MeshRenderer>().material.color = Color.red;
			Destroy(_debugCube.GetComponent<BoxCollider>());
		}
	}

	void Update() 
	{
        if (_inCover) {
            // Debug helper
            if (debugCover) {
                _debugCube.transform.position = _helperTrans.position;
                _debugCube.transform.rotation = _helperTrans.rotation;
            }

            if (!_initCover) {
                DisableController();
                GetIntoCover();
            }
            else
                CoverMovement();
        }
        else if (autoSearchCover)
            RaycastForCover();
        else
            _canManualCover = true;
	}

	public void RaycastForCover() // Raycast to check if in range for cover 
	{
		if(_inCover)
			return;

		Vector3 ori = trans.position - trans.forward;
		ori += Vector3.up * .5f;
		Vector3 dir = trans.forward;
		RaycastHit hit;

		Debug.DrawRay(ori, dir * searchDistance, Color.yellow);
		if(Physics.Raycast(ori, dir, out hit, searchDistance, ignoreLayer))
		{
			_helperTrans.position = PosWithOffset(ori, hit.point);
			_helperTrans.rotation = Quaternion.LookRotation(-hit.normal);

			bool right = CheckCover(_helperTrans, true);
			bool left = CheckCover(_helperTrans, false);

			// If valid cover
			if(right && left)
			{
				_inCover = true;
				_coverPosition.initialHitPoint = hit.point;
			}
		}
		else
			_canManualCover = true;
	}

	public bool GetCoverStatus() // Returns true if player is in cover 
	{
		return _inCover;
	}

	public void ManualGetIntoCover() // Call this to manual cover 
	{
		if(_canManualCover && !_inCover)
		{
			_canManualCover = false;

			RaycastForCover();
		}
		else
			EnableController();
	}

	private Vector3 PosWithOffset(Vector3 ori, Vector3 target) // Position the helper with offset 
	{
		Vector3 dir = ori - target;
		dir.Normalize();
		Vector3 offset = dir * offsetFromWall;
		Vector3 retVal = target + offset;

		return retVal;
	}

	private bool CheckCover(Transform h, bool right) // Check if the cover is of an approriate size 
	{
		bool retVal = false;

		Vector3 side = (right) ? h.right : -h.right;
		side *= minCoverSize;

		Vector3 ori = h.transform.position + side + -h.transform.forward;
		Vector3 dir = h.transform.forward;
		RaycastHit hit;

		Debug.DrawRay(ori, dir * 2f, Color.green);
		if(Physics.Raycast(ori, side, out hit, minCoverSize, ignoreLayer)) // If there is an obstacle on the left/right, cover is invalid
			return false;
		else // Do another raycast to determine size of cover
		{
			ori += side;
			RaycastHit towards;

			if(Physics.Raycast(ori, dir, out towards, searchDistance, ignoreLayer)) // Its a viable cover position from this side
			{
				retVal = true;

				if(right)
					_coverPosition.pos2 = PosWithOffset(ori, towards.point);
				else
					_coverPosition.pos1 = PosWithOffset(ori, towards.point);
			}
			else
				return false;
		}

		return retVal;
	} 

	private void GetIntoCover() // Lerp into cover position 
	{
		if(!_initLerp)
		{
			_initLerp = true;

			_coverPositionLength = Vector3.Distance(_coverPosition.pos1, _coverPosition.pos2);
			float hitDist = Vector3.Distance(_coverPosition.initialHitPoint, _coverPosition.pos1);
			float coverPerc = hitDist / _coverPositionLength;
			_targetPos = Vector3.Lerp(_coverPosition.pos1, _coverPosition.pos2, coverPerc);
			_startPos = trans.position;
			_tLerp = 0f;

			//_crouchCover = !CheckCoverType();
			//_coverDirection = 1;
			//_crouching = _crouchCover;
		}

		float movement = _lerpSpeed * Time.deltaTime;
		//float lerp = movement / _coverPositionLength;
		_tLerp += movement;

		if(_tLerp > 1f)
		{
			_tLerp = 1f;
			_initCover = true;
		}

		Vector3 pos = Vector3.Lerp(_startPos, _targetPos, _tLerp);
		pos.y = trans.position.y;
		trans.position = pos;

		Quaternion rot = Quaternion.LookRotation(_helperTrans.forward);
		trans.rotation = Quaternion.Slerp(trans.rotation, rot, _tLerp);
	}

	private void CoverMovement() // Movement of player while in cover 
	{
		relativeInput.x = playerInput._horizontal;
		relativeInput.z = playerInput._vertical;

		if(relativeInput.z < 0f)
		{
			EnableController();
			return;
		}

		if(relativeInput.x == 0f)
			return;

		_movePositive = (relativeInput.x > 0f);
		//_coverDirection = (_movePositive) ? 1 : -1;
		//_crouchCover = !CheckCoverType();

		//if(_crouchCover)
		//	_crouching = _crouchCover;

		bool isCover = CanMoveOnSide(_movePositive);

		if(twoPointValidation)
		{
			if(!isCover)
				isCover = CanMoveOnSide(_movePositive, .1f);
		}

		Vector3 targetDir = (_helperTrans.position - trans.position).normalized;
		targetDir *= Mathf.Abs(relativeInput.x);

		if(!isCover)
		{
			targetDir = Vector3.zero;
			relativeInput.x = 0f;
		}

		//_aimAtSides = !isCover;
		//_canAim = _aimAtSides;

		characterMove.Move(targetDir * coverMovespeed);
		Quaternion targetRot = _helperTrans.rotation;
		trans.rotation = Quaternion.Slerp(trans.rotation, targetRot, Time.deltaTime * 5f);
		//HandleCoverAnimation(relativeInput);

		if(playerInput.mirrorInt == 1 && relativeInput.x < 0f)
			TPCamera.GetInstance().SwitchShoulder();
		else if(playerInput.mirrorInt == -1 && relativeInput.x > 0f)
			TPCamera.GetInstance().SwitchShoulder();
	}

	private bool CheckCoverType() // Check if its a full wall or half wall 
	{
		bool retVal = false;

		Vector3 ori = _helperTrans.position + Vector3.up;
		Vector3 dir = _helperTrans.forward;
		RaycastHit hit;
		
		if(Physics.Raycast(ori, dir, out hit, 1f, ignoreLayer))
		{
			retVal = true;
		}

		//_crouchCover = !retVal;
		return retVal;
	}

	private bool CanMoveOnSide(bool right, float offset = 0f)
	{
		bool retVal = false;

		Vector3 side = (right) ? _helperTrans.right : -_helperTrans.right;
		side *= peekDistance + offset;
		Vector3 ori = trans.position + side;
		ori += Vector3.up * .5f;
		Vector3 dir = _helperTrans.forward;
		RaycastHit hit;

		if(Physics.Raycast(ori, side, out hit, minCoverSize, ignoreLayer))
			return false;
		else
		{
			RaycastHit towards;
			ori += side;

			if(Physics.Raycast(ori, dir, out towards, 1f, ignoreLayer))
			{
				float angle = Vector3.Angle(_helperTrans.forward, -towards.normal);

				if(angle < coverToleranceAngle)
				{
					retVal = true;
					_helperTrans.position = PosWithOffset(ori, towards.point);
					_helperTrans.rotation = Quaternion.LookRotation(-towards.normal);
				}
			}
			else
				return false;
		}

		return retVal;
	}

	private void DisableController() // In cover, disable other stuffs 
	{
		GetComponent<CharacterController>().detectCollisions = true;
		characterMove.GetInCover();
	}

	public void EnableController() // Exiting cover, enable stuffs 
	{
		_initLerp = false;
		_initCover = false;
		GetComponent<CharacterController>().detectCollisions = false;
		characterMove.GetOutCover();
		_inCover = false;
		_canManualCover = true;

		if(autoSearchCover)
		{
			autoSearchCover = false;
			StartCoroutine(EnableAutoSearchCooldown());
		}

	}

	private IEnumerator EnableAutoSearchCooldown()
	{
		yield return new WaitForSeconds(exitCoverCooldown);
		autoSearchCover = true;
		_canManualCover = true;
	}
}

public class CoverPosition
{
	public Vector3 pos1;
	public Vector3 pos2;
	public Vector3 initialHitPoint;
}
