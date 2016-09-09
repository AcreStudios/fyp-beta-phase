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

	// Jumping helpers
	private bool isJumping = false;
	private bool resetGravity;
	private float gravity;

	// Air control helpers
	private Vector3 airControlVector;
	private float forward;
	private float strafe;


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
			isJumping = false;
	}

	public void AnimateCharacter(float forward, float strafe) // Animates the character with root motion
	{
		this.forward = forward;
		this.strafe = strafe;

		animator.SetFloat(animatorStrings.verticalFloat, forward);
		animator.SetFloat(animatorStrings.horizontalFloat, strafe);
		animator.SetBool(animatorStrings.groundedBool, CheckGrounded());
		animator.SetBool(animatorStrings.jumpingBool, isJumping);
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
		if(isJumping)
			return;

		if(CheckGrounded())
		{
			isJumping = true;
			StartCoroutine(ResetJump());
		}
	}

	private IEnumerator ResetJump() // Stops jumping
	{
		yield return new WaitForSeconds(jumpSettings.airTime);
		isJumping = false;
	}

	private void ApplyGravity() // Applies gravity constantly and don't when jumping
	{
		if(!CheckGrounded())
		{
			if(!resetGravity)
			{
				gravity = jumpSettings.resetGravityValue;
				resetGravity = true;
			}
			gravity += Time.deltaTime * jumpSettings.gravityModifier;
		}
		else
		{
			gravity = jumpSettings.baseGravity;
			resetGravity = false;
		}

		Vector3 gravityVector = new Vector3();

		if(!isJumping)
			gravityVector.y -= gravity * Time.deltaTime; // Apply gravity to character
		else
			gravityVector.y = jumpSettings.jumpSpeed * Time.deltaTime; // Let character jump

		characterController.Move(gravityVector);
	}

	private void AirControl() // Allow movement while in the air
	{
		airControlVector.x = strafe;
		airControlVector.z = forward;
		airControlVector = trans.TransformDirection(airControlVector);
		airControlVector *= jumpSettings.airSpeed;

		characterController.Move(airControlVector * Time.deltaTime);
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
}
