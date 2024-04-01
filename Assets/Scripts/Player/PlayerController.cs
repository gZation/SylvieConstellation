using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private float speed = 7f;

    public bool canMove = true;
    public bool canShoot = true;
    public Transform spawn;

    private Vector2 inputVector;
    private Rigidbody2D rb;

    private PlayerInput input;
    private InputAction movement;

    private Animator anim;

    public float acceleration;
    public float decceleration;
    public float velPower;

    [SerializeField] private Material bgClose;
    [SerializeField] private Material bgFar;
    [SerializeField] private float parallax;

    private Vector2 bgCloseOffset;
    private Vector2 bgFarOffset;

    [SerializeField] private float boostTime;
    private float holdSpeed;
    public float currTime;
    private bool isBoosted;
    public ParticleSystem starBits;
    private Vector3 lastPosition;

    public int ConstellationSceneTransfer = 2;

    #region Player Abilities
    private OrbitControl m_orbitControl;
    private GunAbility m_gunAbility;
    #endregion

    private void Awake() 
    {
        InitializeSingleton();
        input = new PlayerInput();
        isBoosted = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Adding references
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        m_orbitControl = GetComponent<OrbitControl>(); 
        m_gunAbility = GetComponent<GunAbility>();

        // Initialize Input C# Events
        input.Player.Boost.performed += DoBoost;
        input.Player.OrbitControl.performed += m_orbitControl.OrbitObjects;
        input.Player.OrbitControl.canceled += m_orbitControl.PushObjects;
        input.Player.GunShoot.started += m_gunAbility.GunInput;
        input.Player.GunShoot.canceled += m_gunAbility.GunInput;
        // Initial Controls
        canMove = true;
        canShoot = true;
        holdSpeed = speed;
        lastPosition = transform.position;
        SetBGOffset();

        boostTime = 0;
        isBoosted = false;
        transform.position = TransitionManager.Instance.holdPos;


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isBoosted) {
            currTime += Time.deltaTime;
        }
        if (currTime >= boostTime) {
            speed = holdSpeed;
            isBoosted = false;
            boostTime = 0;
        }
        if (canMove) {
            inputVector = input.Player.Move.ReadValue<Vector2>();

            anim.SetFloat("X", inputVector.x);
            anim.SetFloat("Y", inputVector.y);

            float movementX = CalculateMovement(inputVector.x, rb.velocity.x);
            float movementY = CalculateMovement(inputVector.y, rb.velocity.y);
            rb.AddForce(movementX * Vector2.right);
            rb.AddForce(movementY * Vector2.up);
            if (inputVector.magnitude > 0) {
                Vector2 normMovement = inputVector.normalized;
                anim.SetBool("isMoving", true);
                if (!starBits.isPlaying) {
                    starBits.Play();
                }
            } else {
                anim.SetBool("isMoving", false);
                starBits.Stop();
                if (starBits.isPlaying) {
                    starBits.Stop();
                }
            }
        }
        if (canShoot)
        {
            m_gunAbility.UpdateAimDirection(input.Player.GunShoot.ReadValue<Vector2>());
        }
    }

    void Update() {
        
        var currentPosition = transform.position;
        if (currentPosition != lastPosition)
        {
            SetBGOffset();
        }
        lastPosition = currentPosition;

        // DEBUGGING Reset Key
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            transform.position = spawn.transform.position;
        }
    }

    private void OnMove(InputValue movementValue) {
    }

    private void DoBoost(InputAction.CallbackContext obj) {
        isBoosted = true;
        currTime = 0;
        speed = holdSpeed * 3;
        boostTime = 3;
    }

    private void OnEnable() {
        movement = input.Player.Move;
        //movement.Enable();
        //input.Player.Boost.Enable(); 
        input.Player.Enable();
    }

    private void OnDisable() {
        movement = input.Player.Move;
        //movement.Disable();
        //input.Player.Boost.Disable();
        input.Player.Disable();
    }
    
    private float CalculateMovement(float value, float velocityVal) {
        float targetSpeed = value * speed;
        float speedDiff = targetSpeed - velocityVal;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velPower) * Mathf.Sign(speedDiff);
        return movement;
    }

    private void SetBGOffset() {
        //BG stars offset
        //Close stars
        bgCloseOffset.x = transform.position.x / transform.localScale.x / parallax;
        bgCloseOffset.y = transform.position.y / transform.localScale.y / parallax;
        bgClose.SetVector("_Offset", new Vector2(bgCloseOffset.x, bgCloseOffset.y));

        //Far stars
        bgFarOffset.x = transform.position.x / transform.localScale.x / (parallax * 10);
        bgFarOffset.y = transform.position.y / transform.localScale.y / (parallax * 10);
        bgFar.SetVector("_Offset", new Vector2(bgFarOffset.x, bgFarOffset.y));
    }

    private IEnumerator OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "BoostCircle")
        {
            if (!isBoosted)
            {
                isBoosted = true;
                currTime = 0;
                speed = holdSpeed * 3;
                boostTime = 3;
            }
            //allow Sylvie to move through the rings then destroy them
            yield return new WaitForSeconds(.2f);
            Destroy(collision.gameObject);
        }
    }

    public void DeathSequence() {
        StartCoroutine(Die());
    }

    IEnumerator Die() {
		canMove = false;
        yield return new WaitForSeconds(0f);
        transform.position = spawn.transform.position;
        canMove = true;
    }

    public Vector2 GetPosition() {
        return this.gameObject.transform.position;
    }

    public void SetPosition(Vector2 yes) {
        this.gameObject.transform.position = yes;
    }

}
