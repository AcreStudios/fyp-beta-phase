using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour 
{
	[Header("-Dev Mode-")]
	public bool setupRequiredComponents = true;

	// Components
	private Transform trans;
	private Animator animator;
	private CharacterController characterController;

	[System.Serializable] // Show in inspector for classes
	public class AnimatorStrings
	{
		public string verticalFloat = "Forward";
		public string horizontalFloat = "Strafe";
		public string groundedBool = "isGrounded";
		public string jumpingBool = "isJumping";
		public string coverBool = "inCover";
	}
	[SerializeField]
	private AnimatorStrings animatorStrings;

	[System.Serializable]
	public class JumpSettings
	{
		[Header("-Jump-")]
		public float jumpSpeed = 10f;

		public float gravityModifier = 9.81f;
		public float baseGravity = 50f;
		public float resetGravityValue = 1.2f;
		public LayerMask groundLayer;

		public float airTime = .25f;
		public float airSpeed = 5f;
	}
	[SerializeField]
	private JumpSettings jumpSettings;

	// Jumping and air control helpers
	private bool _jumping;
	private bool _resetGravity;
	private float _gravity;

	private Vector3 _airControlVector;
	private float _forward;
	private float _strafe;

	// Cover helpers
	private bool _inCover;


	void Awake()
	{
		// Cache components
		trans = GetComponent<Transform>();
		animator = GetComponent<Animator>();
		characterController = GetComponent<CharacterController>();

		SetupComponents();
		SetupAnimator();
	}

	void Update() 
	{
		ApplyGravity();

		if(!CheckGrounded())
			AirControl();
		else
			_jumping = false;
	}

	public void AnimateCharacter(float forward, float strafe) // Animates the character with root motion
	{
		this._forward = forward;
		this._strafe = strafe;

		animator.SetFloat(animatorStrings.verticalFloat, forward);
		animator.SetFloat(animatorStrings.horizontalFloat, strafe);
		animator.SetBool(animatorStrings.groundedBool, CheckGrounded());
		animator.SetBool(animatorStrings.jumpingBool, _jumping);
		animator.SetBool(animatorStrings.coverBool, _inCover);
	}

	private bool CheckGrounded() // Spherecasting downwards to check ground
	{
		RaycastHit hit;
		Vector3 start = trans.position + trans.up;
		Vector3 dir = Vector3.down;

		if(Physics.SphereCast(start, characterController.radius, dir, out hit, characterController.height * .5f, jumpSettings.groundLayer))
			return true;
		else
			return false;
	}

	public void DoJump()
	{
		if(_jumping)
			return;

		if(CheckGrounded())
		{
			_jumping = true;
			StartCoroutine(ResetJump());
		}
	}

	private IEnumerator ResetJump() // Stops jumping
	{
		yield return new WaitForSeconds(jumpSettings.airTime);
		_jumping = false;
	}

	private void ApplyGravity() // Applies gravity constantly and don't when jumping
	{
		if(!CheckGrounded())
		{
			if(!_resetGravity)
			{
				_gravity = jumpSettings.resetGravityValue;
				_resetGravity = true;
			}
			_gravity += Time.deltaTime * jumpSettings.gravityModifier;
		}
		else
		{
			_gravity = jumpSettings.baseGravity;
			_resetGravity = false;
		}

		Vector3 gravityVector = new Vector3();

		if(!_jumping)
			gravityVector.y -= _gravity * Time.deltaTime; // Apply gravity to character
		else
			gravityVector.y = jumpSettings.jumpSpeed * Time.deltaTime; // Let character jump

		characterController.Move(gravityVector);
	}

	private void AirControl() // Allow movement while in the air
	{
		_airControlVector.x = _strafe;
		_airControlVector.z = _forward;
		_airControlVector = trans.TransformDirection(_airControlVector);
		_airControlVector *= jumpSettings.airSpeed;

		characterController.Move(_airControlVector * Time.deltaTime);
	}

	private void SetupComponents() // Initialise the components values in the editor
	{
		#region Animator

		animator.applyRootMotion = true;

		// Check for animator controller
		if(!animator.runtimeAnimatorController)
			Debug.LogError("There is no animator controller in" + name + " found! Please assign the approriate animator controller!");

		#endregion

		#region Character controller

		characterController.skinWidth = 0.0001f;
		characterController.center = new Vector3(0f, .8f, 0f);
		characterController.radius = .25f;
		characterController.height = 1.6f;

		#endregion
	}

	private void SetupAnimator() // Setup the animator avatar with the model avatar
	{
		if(!animator.runtimeAnimatorController)
			return;

		Animator modelAnim = GetComponentsInChildren<Animator>()[1]; // [0] is self, [1] is next
		Avatar modelAva = modelAnim.avatar;

		animator.avatar = modelAva;
		Destroy(modelAnim);
	}

	public void GetInCover()
	{
		_inCover = true;
	}

	public void GetOutCover()
	{
		_inCover = false;
	}
}
