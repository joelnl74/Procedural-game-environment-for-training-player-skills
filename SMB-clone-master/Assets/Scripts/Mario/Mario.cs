using UnityEngine;

public class Mario : MonoBehaviour {
	private LevelManager t_LevelManager;
	private Transform m_GroundCheck1, m_GroundCheck2;
	private Animator m_Animator;
	private Rigidbody2D m_Rigidbody2D;
	private CapsuleCollider2D m_capsuleCollider2D;

	public LayerMask GroundLayers;
	public GameObject Fireball;
	public Transform FirePos;
	private float waitBetweenFire = .2f;
	private float fireTimeDelayTime = 0;

	private float faceDirectionX;
	private float moveDirectionX;

	[SerializeField] private float normalGravity;

	private float currentSpeedX;
	private float speedXBeforeJump;

	[SerializeField] private float minWalkSpeedX = .28f;
	[SerializeField] private float walkAccelerationX = .14f;
	[SerializeField] private float runAccelerationX = .21f;
	[SerializeField] private float releaseDecelerationX = .19f;
	[SerializeField] private float skidDecelerationX = .38f;
	[SerializeField] private float skidTurnaroundSpeedX = 2.11f;
	[SerializeField] private float maxWalkSpeedX = 5.86f;
	[SerializeField] private float maxRunSpeedX = 9.61f;

	[SerializeField] private float minJumpSpeed = 3.76f;
	[SerializeField] private float maxJumpSpeed = 8.67f;

	[SerializeField] private float minJumpSpeedY = 15f;
	[SerializeField] private float minJumpSpeedUpGravity = 0.47f;
	[SerializeField] private float minJumpSpeedDownGravity = 1.64f;

	[SerializeField] private float maxJumpSpeedY = 15f;
	[SerializeField] private float maxJumpSpeedUpGravity = 0.44f;
	[SerializeField] private float maxJumpSpeedDownGravity = 1.41f;

	[SerializeField] private float normaleJumpSpeedY = 18.75f;
	[SerializeField] private float normaleSpeedUpGravity = .59f;
	[SerializeField] private float normaleJumpSpeedDownGravity = 2.11f;

	[SerializeField] private float midAircurrentSpeedX = 5.86f;
	[SerializeField] private float midmidairAccelerationX = .14f;
	[SerializeField] private float midAirspeedXBeforeJump = 6.8f;
	[SerializeField] private float midairDecelerationXJump = 0.14f;
	[SerializeField] private float midairDecelerationXJumpIncreased = 0.19f;
	[SerializeField] private float midairDecelerationNormale = 0.21f;

	private float jumpSpeedY;
	private float jumpUpGravity;
	private float jumpDownGravity;
	private float midairAccelerationX;
	private float midairDecelerationX;

	private float automaticWalkSpeedX;
	private float automaticGravity;

	private float deadUpTimer = .25f;

	public float castleWalkSpeedX = 5.86f;
	public float levelEntryWalkSpeedX = 3.05f;

	private bool isGrounded;
	private bool isDashing;
	private bool isFalling;
	private bool isJumping;
	private bool isChangingDirection;
	private bool wasDashingBeforeJump;
	private bool isShooting;
	private bool isFacingRight;
	
	public bool isCrouching;

	private bool jumpButtonHeld;
	private bool jumpButtonReleased;

	public bool inputFreezed;

	public bool isDying = false;

	public Vector2 respawnPositionPCG;

	// Use this for initialization
	void Awake () 
	{
		t_LevelManager = FindObjectOfType<LevelManager>();
		m_GroundCheck1 = transform.Find ("Ground Check 1");
		m_GroundCheck2 = transform.Find ("Ground Check 2");
		m_capsuleCollider2D = GetComponent<CapsuleCollider2D>();
		m_Animator = GetComponent<Animator> ();
		m_Rigidbody2D = GetComponent<Rigidbody2D> ();
		normalGravity = m_Rigidbody2D.gravityScale;

		// Set correct size
		UpdateSize ();

		jumpButtonReleased = true;
		fireTimeDelayTime = 0;
	}

	/****************** Movement control */
	void SetJumpParams() 
	{
		if (currentSpeedX <= minJumpSpeed) 
		{
			jumpSpeedY = minJumpSpeedY;
			jumpUpGravity = minJumpSpeedUpGravity;
			jumpDownGravity = minJumpSpeedDownGravity;
		} 
		else if (currentSpeedX <= maxJumpSpeed) 
		{
			jumpSpeedY = maxJumpSpeedY;
			jumpUpGravity = maxJumpSpeedUpGravity;
			jumpDownGravity = maxJumpSpeedDownGravity;
		} 
		else 
		{
			jumpSpeedY = normaleJumpSpeedY;
			jumpUpGravity = normaleSpeedUpGravity;
			jumpDownGravity = normaleJumpSpeedDownGravity;
		}
	}

	void SetMidairParams() 
	{
		if (currentSpeedX <= midAircurrentSpeedX) 
		{
			midairAccelerationX = midmidairAccelerationX;
			if (speedXBeforeJump <= midAirspeedXBeforeJump) 
			{
				midairDecelerationX = midairDecelerationXJump;
			} else 
			{
				midairDecelerationX = midairDecelerationXJumpIncreased;
			}
		}
		else 
		{
			midairAccelerationX = midairDecelerationNormale;
			midairDecelerationX = midairDecelerationNormale;
		}
	}

	void FixedUpdate ()
	{
		/******** Horizontal movement on ground */
		if (isGrounded)
		{
			// If holding directional button, accelerate until reach max walk speed
			// If holding Dash, accelerate until reach max run speed
			if (faceDirectionX != 0)
			{
				if (currentSpeedX == 0) 
				{
					currentSpeedX = minWalkSpeedX;
				} else if (currentSpeedX < maxWalkSpeedX)
				{
					currentSpeedX = IncreaseWithinBound (currentSpeedX, walkAccelerationX, maxWalkSpeedX);
				} else if (isDashing && currentSpeedX < maxRunSpeedX)
				{
					currentSpeedX = IncreaseWithinBound (currentSpeedX, runAccelerationX, maxRunSpeedX);
				}
			} 

			// Decelerate upon release of directional button
			else if (currentSpeedX > 0)
			{
				currentSpeedX = DecreaseWithinBound (currentSpeedX, releaseDecelerationX, 0);
			}

			// If change direction, skid until lose all momentum then turn around
			if (isChangingDirection && isGrounded)
			{
				if (currentSpeedX > skidTurnaroundSpeedX) 
				{
					moveDirectionX = -faceDirectionX;
					m_Animator.SetBool ("isSkidding", isJumping ? false : true);
					currentSpeedX = DecreaseWithinBound (currentSpeedX, skidDecelerationX, 0);
				} 
				else
				{
					moveDirectionX = faceDirectionX;
					m_Animator.SetBool ("isSkidding", false);
				}
			} 
			else
			{
				m_Animator.SetBool ("isSkidding", false);
			}

			// Freeze horizontal movement while crouching
			if (isCrouching)
			{
				currentSpeedX = 0;
			}

		/******** Horizontal movement on air */
		} 
		else
		{
			SetMidairParams ();

			// Holding Dash while in midair has no effect
			if (faceDirectionX != 0 && isChangingDirection == false) 
			{
				if (currentSpeedX == 0) 
				{
					currentSpeedX = minWalkSpeedX;
				} else if (currentSpeedX < maxWalkSpeedX) 
				{
					currentSpeedX = IncreaseWithinBound (currentSpeedX, midairAccelerationX, maxWalkSpeedX);
				} else if (wasDashingBeforeJump && currentSpeedX < maxRunSpeedX) {
					currentSpeedX = IncreaseWithinBound (currentSpeedX, midairAccelerationX, maxRunSpeedX);
				}
			}
			else if (currentSpeedX > 0) 
			{
				currentSpeedX = DecreaseWithinBound (currentSpeedX, releaseDecelerationX, -1);
			}

			// If change direction, decelerate but keep facing move direction
			if (isChangingDirection)
			{
				faceDirectionX = moveDirectionX;
				currentSpeedX = DecreaseWithinBound (currentSpeedX, midairDecelerationX, -1);
			}
		}

		/******** Vertical movement */
		if (isGrounded) 
		{
			isJumping = false;
			m_Rigidbody2D.gravityScale = normalGravity;
		}

		if (isJumping == false) 
		{
			if (isGrounded && jumpButtonHeld && jumpButtonReleased) 
			{
				SetJumpParams ();
				m_Rigidbody2D.velocity = new Vector2 (m_Rigidbody2D.velocity.x, jumpSpeedY);
				isJumping = true;
				jumpButtonReleased = false;
				speedXBeforeJump = currentSpeedX;
				wasDashingBeforeJump = isDashing;
				if (t_LevelManager.marioSize == 0) 
				{
					t_LevelManager.soundSource.PlayOneShot (t_LevelManager.jumpSmallSound);
				} 
				else 
				{
					t_LevelManager.soundSource.PlayOneShot (t_LevelManager.jumpSuperSound);
				}
			}
		}
		else 
		{  // lower gravity if Jump button held; increased gravity if released
			if (m_Rigidbody2D.velocity.y > 0 && jumpButtonHeld) {
				m_Rigidbody2D.gravityScale = normalGravity * jumpUpGravity;
			} else 
			{
				m_Rigidbody2D.gravityScale = normalGravity * jumpDownGravity;
			}
		}

		/******** Horizontal orientation */
		if (isGrounded && faceDirectionX != 0)
        {
			if (isFacingRight)
			{
				transform.localScale = new Vector2(1, 1); // facing right
			}
			else
            {
				transform.localScale = new Vector2(-1, 1);
			}
		}

		/******** Reset params for automatic movement */
		if (inputFreezed)
		{
			currentSpeedX = automaticWalkSpeedX;
			m_Rigidbody2D.gravityScale = automaticGravity;
		}

		/******** Shooting */
		if (isShooting && t_LevelManager.marioSize == 2)
		{

			if (fireTimeDelayTime >= waitBetweenFire)
			{
				m_Animator.SetTrigger ("isFiring");
				GameObject fireball = Instantiate (Fireball, FirePos.position, Quaternion.identity);
				fireball.GetComponent<MarioFireball> ().directionX = transform.localScale.x;
				t_LevelManager.soundSource.PlayOneShot (t_LevelManager.fireballSound);
				fireTimeDelayTime = 0;
			}
		}

		/******** Set params */
		m_Rigidbody2D.velocity = new Vector2 (moveDirectionX * currentSpeedX, m_Rigidbody2D.velocity.y);

		m_Animator.SetBool ("isJumping", isJumping);
		m_Animator.SetBool ("isFallingNotFromJump", isFalling && !isJumping);
		m_Animator.SetBool ("isCrouching", isCrouching);
		m_Animator.SetFloat ("absSpeed", Mathf.Abs (currentSpeedX));

		if (faceDirectionX != 0 && !isChangingDirection)
		{
			moveDirectionX = faceDirectionX;
		}		
	}

	/****************** Automatic movement sequences */
	void Update() {
		if (!inputFreezed) 
		{
			faceDirectionX = Input.GetAxisRaw ("Horizontal"); // > 0 for right, < 0 for left
			isDashing = Input.GetButton ("Dash");
			isCrouching = Input.GetButton ("Crouch");
			isShooting = Input.GetButtonDown ("Dash");
			jumpButtonHeld = Input.GetButton ("Jump");
			if (Input.GetButtonUp ("Jump")) {
				jumpButtonReleased = true;
			}
		}

		fireTimeDelayTime += Time.deltaTime;

		if (faceDirectionX != 0 && faceDirectionX > 0)
        {
			isFacingRight = true;
        }
		else if (faceDirectionX != 0)
        {
			isFacingRight = false;
        }

		isChangingDirection = currentSpeedX > 0 && faceDirectionX * moveDirectionX < 0;
		isGrounded = Physics2D.OverlapPoint(m_GroundCheck1.position, GroundLayers) || Physics2D.OverlapPoint(m_GroundCheck2.position, GroundLayers);
		isFalling = m_Rigidbody2D.velocity.y < 0 && !isGrounded;

		if (inputFreezed && !t_LevelManager.gamePaused) 
		{
			if (isDying) 
			{
				deadUpTimer -= Time.unscaledDeltaTime;
				if (deadUpTimer > 0) 
				{
					gameObject.transform.position += Vector3.up * .22f;
				} else 
				{
					gameObject.transform.position += Vector3.down * .2f;
				}
			} 
			else if (isClimbingFlagPole) 
			{
				m_Rigidbody2D.MovePosition (m_Rigidbody2D.position + climbFlagPoleVelocity * Time.deltaTime);
			}
		}
	}

	public void FreezeAndDie() {
		FreezeUserInput ();
		isDying = true;
		m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		m_Animator.SetTrigger ("respawn");
		gameObject.layer = LayerMask.NameToLayer ("Falling to Kill Plane");
		gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground Effect";
	}

	public void UnFreeAndLive()
    {
		var cameraPos = Camera.main.transform.position;

		transform.position = respawnPositionPCG;
		Camera.main.transform.position = new Vector3(respawnPositionPCG.x, cameraPos.y, cameraPos.z);

		isDying = false;
		deadUpTimer = .25f;

		gameObject.layer = LayerMask.NameToLayer("Mario");
		gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Player";

		m_Animator.ResetTrigger("respawn");
		m_Animator.Rebind();

		m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

		UnfreezeUserInput();
	}

	public void EnablePhysics()
    {
		if(m_Rigidbody2D == null)
        {
			m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }

		m_Rigidbody2D.WakeUp();
	}

	bool isClimbingFlagPole = false;
	Vector2 climbFlagPoleVelocity = new Vector2 (0, -5f);

	public void ClimbFlagPole() 
	{
		FreezeUserInput ();
		isClimbingFlagPole = true;
		m_Animator.SetBool ("climbFlagPole", true);
		m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		Debug.Log (this.name + ": Mario starts climbing flag pole");
	}

	void JumpOffPole() 
	{ 
		// get off pole and start walking right
		transform.position = new Vector2 (transform.position.x + .5f, transform.position.y);
		m_Animator.SetBool ("climbFlagPole", false);
		AutomaticWalk(castleWalkSpeedX);
		m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
		Debug.Log (this.name + ": Mario jumps off pole and walks to castle");
	}

	/****************** Automatic movement (e.g. walk to castle sequence) */
	public void UnfreezeUserInput() 
	{
		inputFreezed = false;
		Debug.Log (this.name + " UnfreezeUserInput called");
	}

	public void FreezeUserInput() 
	{
		inputFreezed = true;
		jumpButtonHeld = false;
		jumpButtonReleased = true;

		faceDirectionX = 0;
		moveDirectionX = 0;

		currentSpeedX = 0;
		speedXBeforeJump = 0;
		automaticWalkSpeedX = 0;
		automaticGravity = normalGravity;

		isDashing = false;
		wasDashingBeforeJump = false;
		isCrouching = false;
		isChangingDirection = false;
		isShooting = false;

		gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero; // stop all momentum
		Debug.Log (this.name + " FreezeUserInput called");
	}

	public void AutomaticWalk(float walkVelocityX)
	{
		FreezeUserInput ();
		if (walkVelocityX != 0) {
			faceDirectionX = walkVelocityX / Mathf.Abs (walkVelocityX);
		}
		automaticWalkSpeedX = Mathf.Abs(walkVelocityX);
		Debug.Log (this.name + " AutomaticWalk: speed=" + automaticWalkSpeedX.ToString());
	}

	public void AutomaticCrouch()
	{
		FreezeUserInput ();
		isCrouching = true;
	}

	/****************** Misc */
	public void UpdateSize() 
	{
		var marioSize = t_LevelManager.marioSize;

		m_Animator.SetInteger("marioSize", marioSize);

		if (marioSize >= 1)
        {
			m_capsuleCollider2D.offset = new Vector2(0, 1);
			m_capsuleCollider2D.size = new Vector2(0.9f, 2);
		}
		else
        {
			m_capsuleCollider2D.offset = new Vector2(0, 0.5f);
			m_capsuleCollider2D.size = new Vector2(0.9f, 1);
		}
	}

	float IncreaseWithinBound(float val, float delta, float maxVal = Mathf.Infinity)
	{
		val += delta;
		if (val > maxVal) 
		{
			val = maxVal;
		}

		return val;
	}

	float DecreaseWithinBound(float val, float delta, float minVal = 0) {
		val -= delta;

		if (val < minVal) 
		{
			val = minVal;
		}

		return val;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		Vector2 normal = other.contacts[0].normal;
		Vector2 bottomSide = new Vector2 (0f, 1f);
		bool bottomHit = normal == bottomSide;

		if (other.gameObject.tag.Contains ("Enemy"))
		{
			var enemy = other.gameObject.GetComponent<Enemy> ();
			var shell = other.gameObject.GetComponent<KoopaShell>();
			var enemyTransform = enemy.transform;

			if (shell != null && shell.isRolling == false)
            {
				shell.StompedByMario();

				return;
            }

			if (transform.position.y > enemy.transform.position.y 
				&& other.gameObject.tag != "Enemy/Piranha"
				&& other.gameObject.tag != "Enemy/Bowser")
            {
				Debug.Log(this.name + " OnTriggerEnter2D: recognizes " + other.gameObject.name);
				t_LevelManager.MarioStompEnemy(enemy);
				Debug.Log(this.name + " OnTriggerEnter2D: finishes calling stomp method on " + other.gameObject.name);
			}
			else if (t_LevelManager.isInvincible () == false) 
			{
				if (shell != null && shell.isRolling && enemyTransform.position.y >= transform.position.y  ||  // non-rolling shell should do no damage
					enemy.isBeingStomped == false && enemyTransform.position.y >= transform.position.y) 
				{
					Debug.Log (this.name + " OnCollisionEnter2D: Damaged by " + other.gameObject.name
						+ " from " + normal.ToString () + "; isFalling=" + isFalling);

					t_LevelManager.MarioPowerDown ();
				}

			} 
			else if (t_LevelManager.isInvincibleStarman) 
			{
				t_LevelManager.MarioStarmanTouchEnemy (enemy);
			}
		
		} 
		else if (other.gameObject.tag == "Goal" && isClimbingFlagPole && bottomHit)
		{
			Debug.Log (this.name + ": Mario hits bottom of flag pole");
			isClimbingFlagPole = false;
			JumpOffPole ();
		}
	}
}
