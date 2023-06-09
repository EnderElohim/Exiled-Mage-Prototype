using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using DigitalRuby.LightningBolt;
using UnityEngine.UI;
//using Kit;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class scrPlayer : MonoBehaviour
{

    [Header("Assignment")]
    public GameObject[] headObjects;
    public Transform verticalRay;
    public Transform horizontalRay;

    [Header("Stats")]
    public soPlayerData playerStats;
    public LayerMask platformLayerMask;
    public bool isHeadOpen;

    [Header("Information")]
    [SerializeField] Vector3 currentMoveDirection = Vector3.zero;
    public float jumperMultiplierInfo;

    private bool canMove = true;
    private bool isGrounded;
    private int jumpCount;

    //Private
    private float jumpQueueTimer;
    private CharacterController characterController;
    private Vector3 teleportDirection = Vector3.zero;
    private float rotationX = 0;
    private bool isRightWallOn;
    private bool isLeftWallOn;
    private GameObject leftWall;
    private GameObject rightWall;
    private GameObject currentWall;
    private float walkingSpeed;
    private float modifiedSpeed;
    private Collider currentWallCollider;
    private Camera _playerCamera;
    private bool isFlying;
    private GameObject lastSpecialJumper;
    private AudioSource speaker;
    private float jumpTimer;
    [SerializeField] private float wallDegree;
    private GameObject currentMovingPlatformParent;
    private bool isRespawning;
    [SerializeField] private float wallGlideTimer;
    private Vector3 startingPos;
    private Quaternion startingRot;
    private Vector3 startingScale;
    private MovementStateEnum movementState;
    private LightningBoltScript hookshotRenderer;
    private Vector3 characterVelocityMomentum;
    private Vector3 characterVelocityMomentumFromWallJump;
    //private Image cursorImage; enable later
    private Transform currentHookshotTarget;
    private float hookshotCooldown;
    private Image hookshotCooldownDisplayer;
    private Image jumpCounterDisplayer;
    //private GameObject endPoint;
    private Vector3 projectilePos;
    private Animator anim;
    private scrIkController ikController;
    private float animMove;
    private float airTime;
    private int invertVerticalAimVal;
    private float sensitivity;
    private Transform currentCheckpoint;
    private List<SkillsEnum> currentSkills = new List<SkillsEnum>();
    private soPlayerSave currentSave;
    private soSettingsData settingsData;
    private Sequence dashSequence;
    private int dashChargeCounter = 3;
    private const float DEAD_DELAY = 0.25f;
    private bool isStickyWallOn;


    private Camera playerCamera
    {
        get
        {

            if (!_playerCamera)
                _playerCamera = Camera.main;

            return _playerCamera;
        }
    }

    //Dash
    private float dashTimer;
    private bool isDashing;
    private bool isPaused;
    private List<GameObject> dashRefreshObjects = new List<GameObject>();
    private GameObject[] dashChargeIcons;
    private bool isFinishedLevel;
    private bool isFirstMoveDone;

    private float hor;
    private float ver;
    private bool isHookshotHaveValidTarget;
    //New input stuff
    private PlayerInputAction myInputAction;
    private InputAction movementAction;
    private InputAction mouseAction;
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;
    private Vector3 dashDirection;

    //Stomp
    private bool isStompOn;

    //Ledge Climb
    private GameObject verticalRayTarget;
    private GameObject horizontalRayTarget;
    private bool isLedgeJumping;

    private void Awake()
    {
        //pia = new PlayerInputAction();
        if (scrInputManager.playerActions == null)
        {
            Debug.LogError("scrInputManager.playerActions == null");
            SceneManager.LoadScene(0);
            this.enabled = false;
        }
        myInputAction = scrInputManager.playerActions;
    }

    private void OnEnable()
    {
        if (scrInputManager.playerActions == null)
        {
            return;
        }

        movementAction = myInputAction.Player.Movement;
        mouseAction = myInputAction.Player.Mouse;

        myInputAction.Player.Jump.performed += HandleJump;
        myInputAction.Player.Mouse.performed += HandleCameraLookEvent;
        myInputAction.Player.Escape.performed += TriggerEscapeButton;
        myInputAction.Player.Hookshot.performed += TriggerHookshot;
        //myInputAction.Player.ReturnToCheckpoint.performed += TriggerReturnToCheckpointButton;
        //myInputAction.Player.Restart.performed += TriggerRestartLevel;
        myInputAction.Player.Stomp.performed += TriggerStomp;
        myInputAction.Player.Stomp.canceled += TriggerStomp;
        myInputAction.Player.Dash.performed += TriggerDash;

        EnableInputs();
        myInputAction.Player.Escape.Enable();
        myInputAction.Player.Restart.Enable();
    }

    private void OnDisable()
    {
        if (scrInputManager.playerActions == null)
        {
            return;
        }

        myInputAction.Player.Jump.performed -= HandleJump;
        myInputAction.Player.Mouse.performed -= HandleCameraLookEvent;
        myInputAction.Player.Escape.performed -= TriggerEscapeButton;
        myInputAction.Player.Hookshot.performed -= TriggerHookshot;
        //myInputAction.Player.ReturnToCheckpoint.performed -= TriggerReturnToCheckpointButton;
        //myInputAction.Player.Restart.performed -= TriggerRestartLevel;
        myInputAction.Player.Stomp.performed -= TriggerStomp;
        myInputAction.Player.Stomp.canceled -= TriggerStomp;
        myInputAction.Player.Dash.performed -= TriggerDash;


        DisabeInputs();
        myInputAction.Player.Escape.Disable();
        myInputAction.Player.Restart.Disable();
    }

    private void EnableInputs()
    {
        movementAction.Enable();
        mouseAction.Enable();
        myInputAction.Player.Jump.Enable();
        myInputAction.Player.Stomp.Enable();
        myInputAction.Player.Hookshot.Enable();
        myInputAction.Player.ReturnToCheckpoint.Enable();
        myInputAction.Player.Dash.Enable();

    }

    private void DisabeInputs()
    {
        movementAction.Disable();
        mouseAction.Disable();
        myInputAction.Player.Jump.Disable();
        myInputAction.Player.Stomp.Disable();
        myInputAction.Player.Hookshot.Disable();
        myInputAction.Player.ReturnToCheckpoint.Disable();
        myInputAction.Player.Dash.Disable();
    }

    private void Start()
    {
        dashSequence = DOTween.Sequence();
        currentSave = Resources.Load("Save Data") as soPlayerSave;
        settingsData = Resources.Load("Settings Data") as soSettingsData;

        playerStats.skills = currentSave.skills;


        DOTween.SetTweensCapacity(2000, 100);
        speaker = GetComponent<AudioSource>();

        invertVerticalAimVal = settingsData.invertVerticalAimVal;
        sensitivity = settingsData.sensitivity;

        if (isHeadOpen == true)
        {
            for (int i = 0; i < headObjects.Length; i++)
            {
                headObjects[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < headObjects.Length; i++)
            {
                headObjects[i].SetActive(false);
            }
        }
        //if (GetComponent<FPSCounter>() == null)
        //{
        //    gameObject.AddComponent<FPSCounter>();
        //}
        anim = GetComponentInChildren<Animator>();
        ikController = anim.gameObject.GetComponent<scrIkController>();

        startingPos = transform.position;
        startingRot = transform.rotation;
        startingScale = transform.localScale;
        walkingSpeed = playerStats.baseSpeed;
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        /* Cursor Stuff
        cursorImage = Instantiate(scrGameData.values.cursor, GameObject.FindGameObjectWithTag("Player Canvas").transform).GetComponent<Image>();
        hookshotCooldownDisplayer = cursorImage.transform.GetChild(0).GetComponent<Image>();
        jumpCounterDisplayer = cursorImage.transform.GetChild(1).GetComponent<Image>();
        */

        //endPoint = GameObject.FindGameObjectWithTag("End Point");

        for (int i = 0; i < playerStats.skills.Length; i++)
        {
            if (playerStats.skills[i].isOn == true)
            {
                currentSkills.Add(playerStats.skills[i].skill);
            }
        }



        //scrSkillUnlocker ssu = GameObject.FindObjectOfType<scrSkillUnlocker>();
        //if (ssu != null)
        //{
        //    if (IsHaveSkill(ssu.skillToUnlock) == false)
        //    {
        //        currentSkills.Add(ssu.skillToUnlock);
        //        for (int i = 0; i < playerStats.skills.Length; i++)
        //        {
        //            if (playerStats.skills[i].skill == ssu.skillToUnlock)
        //            {
        //                playerStats.skills[i].isOn = true;
        //            }
        //        }
        //    }
        //}

        //For dash visual
        //dashChargeIcons = new GameObject[3];

        //for (int i = 0; i < 3; i++)
        //{
        //    dashChargeIcons[i] = cursorImage.transform.GetChild(2).GetChild(i).gameObject;
        //    dashChargeIcons[i].gameObject.SetActive(IsHaveSkill(SkillsEnum.Dash));

        //}

    }

    private void Update()
    {

        if (isLedgeJumping == true) return;
        if (isPaused == true) return;


        if (isRespawning) return;

        if (isPaused == true) return;

        if (jumpQueueTimer > 0)
        {
            jumpQueueTimer -= Time.deltaTime;
        }

        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }


        if (movementState == MovementStateEnum.HookshotFlying)
        {
            HandleHookshotMovement();
            HandleCameraLook();
            //HandleJump();
            //HandleParry();
            return;
        }

        LedgeJump();
        CheckCeiling();
        modifiedSpeed = isGrounded ? walkingSpeed : walkingSpeed * playerStats.airSpeedMultipliar;

        //Debug Inputs
        //DebugInputs();
        CheckWalls();
        CheckMovingPlatform();

        //if((hor == 0 & ver == 0) || settingsData.suddenStopVal == 1)
        //{
        //    hor = Input.GetAxisRaw("Horizontal");
        //    ver = Input.GetAxisRaw("Vertical");
        //}
        //else
        //{
        //    hor = Input.GetAxis("Horizontal");
        //    ver = Input.GetAxis("Vertical");
        //}

        Vector2 _rawInput = movementAction.ReadValue<Vector2>();

        if (_rawInput == Vector2.zero)
        {
            currentInputVector = Vector2.SmoothDamp(currentInputVector, _rawInput, ref smoothInputVelocity, Time.deltaTime * playerStats.smoothInputSpeed);

            //if (Mathf.Abs(currentInputVector.x) < 0.01f)
            //{
            //    currentInputVector.x = 0;
            //}
            //else
            //{
            //}
            //if (Mathf.Abs(currentInputVector.y) < 0.01f)
            //{
            //    currentInputVector.y = 0;
            //}
        }
        else
        {
            currentInputVector = _rawInput;
        }

        if (settingsData.suddenStopVal == 0)
        {
            if (_rawInput.x != 0 || _rawInput.y != 0)
            {
                //KickstartGame();
            }

            hor = currentInputVector.x;
            ver = currentInputVector.y;
        }
        else
        {
            hor = _rawInput.x;
            ver = _rawInput.y;
        }


        if (hor != 0 || ver != 0)
        {
            //KickstartGame();
        }

        anim.SetFloat("hor", hor);
        anim.SetFloat("ver", ver);



        float curSpeedX = canMove ? (modifiedSpeed) * ver : 0;
        float curSpeedY = canMove ? (modifiedSpeed) * hor : 0;

        float movementDirectionY = currentMoveDirection.y;

        currentMoveDirection = (transform.TransformDirection(Vector3.forward) * curSpeedX) + (transform.TransformDirection(Vector3.right) * curSpeedY);

        currentMoveDirection = Vector3.ClampMagnitude(currentMoveDirection, (modifiedSpeed));
        currentMoveDirection.y = movementDirectionY;

        if (isStompOn == true)
        {
            currentMoveDirection.x = 0;
            currentMoveDirection.z = 0;
        }

        dashDirection = currentMoveDirection;

        HandleDash();

        if (characterController.isGrounded)
        {
            anim.SetBool("in air", false);

            if (!isGrounded)
            {
                isGrounded = true;
                isStompOn = false;
                if (jumpCount != 0)
                {
                    if (airTime > 1)
                    {
                        anim.SetTrigger("jump finish");
                    }
                }

                if (jumpQueueTimer > 0 && jumpTimer <= 0)
                {
                    jumpQueueTimer = 0;
                    jumpCount++;
                    ExecuteJump(false);
                }
                else
                {
                    RefreshJump();
                }
            }

            if (jumpQueueTimer > 0 && jumpTimer <= 0)
            {
                jumpQueueTimer = 0;
                jumpCount++;
                ExecuteJump(false);
            }
            else
            {
                if (jumpTimer <= 0)
                    RefreshJump();
            }



            if (jumpTimer <= 0)
            {
                currentMoveDirection.y = -2f;
            }
            airTime = 0;
        }
        else
        {
            airTime += Time.deltaTime;
            if (airTime > 0.2f)
            {
                anim.SetBool("in air", true);
            }
        }

        isGrounded = characterController.isGrounded;

        //if (playerStats.flyingAvalible)
        //{
        //    if (Input.GetButton("Jump"))
        //    {
        //        isFlying = true;
        //        moveDirection.y = Mathf.Clamp(moveDirection.y + Time.deltaTime * 10, 0, playerStats.jumpSpeed);

        //        //moveDirection.y = playerStats.jumpSpeed; //TODO:Increase with time
        //    }
        //    else
        //    {
        //        isFlying = false;
        //    }
        //}
        //else
        //{
        //    HandleJump();
        //}



        Vector3 animDir = currentMoveDirection;
        animDir.y = 0;

        bool isMoving = animDir == Vector3.zero ? false : true;

        if (isMoving == false)
        {
            if (animMove > 0.3f)
            {
                animMove = Mathf.Lerp(animMove, 0, Time.deltaTime * 7);
            }
            else
            {
                animMove = 0;
            }
        }
        else
        {
            if (animMove < 0.7f)
            {
                animMove = Mathf.Lerp(animMove, 1, Time.deltaTime * 7);

            }
            else
            {
                animMove = 1;
            }
        }


        if (ver < 0)
        {
            anim.SetFloat("locomotion", animMove * -1);
        }
        else
        {
            anim.SetFloat("locomotion", animMove);
        }


        if (!characterController.isGrounded)
        {
            if ((isLeftWallOn || isRightWallOn) && (currentMoveDirection.x != 0 || currentMoveDirection.z != 0))
            {
                if (isDashing == false)
                {
                    if (currentMoveDirection.y < 0)
                    {
                        if (currentMoveDirection.y > -2)
                        {
                            currentMoveDirection.y -= playerStats.gravity * Time.deltaTime;
                        }
                        else
                        {
                            currentMoveDirection.y = -2;
                        }
                    }
                    else if (jumpTimer <= 0)
                    {
                        currentMoveDirection.y -= playerStats.gravity * 2 * Time.deltaTime;
                    }
                }

                currentMoveDirection.x *= playerStats.wallRunSpeedMultiplier;
                currentMoveDirection.z *= playerStats.wallRunSpeedMultiplier;
            }
            else if (isDashing == false)
            {
                currentMoveDirection.y -= playerStats.gravity * Time.deltaTime * (isStompOn ? 4 : 1);
            }

            currentMoveDirection.y = Mathf.Clamp(currentMoveDirection.y, (isStompOn ? playerStats.maximumStompFallSpeed : playerStats.maximumFallSpeed), Mathf.Infinity);
        }
        else
        {
            currentWall = null;
        }



        //Move the controller
        // Player and Camera rotation
        if (canMove)
        {
            currentMoveDirection += characterVelocityMomentum + characterVelocityMomentumFromWallJump;

            if (characterController.enabled == true)
            {
                characterController.Move(currentMoveDirection * (isFlying ? 2 : 1) * Time.deltaTime);
            }


            HandleWallRunTilt(!isGrounded);
            HandleCameraLook();
            //HandleCameraLook();
        }

        if (characterVelocityMomentum.magnitude >= 0f)
        {
            float momentumDrag = 3f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;

            if (characterVelocityMomentum.magnitude < 0f)
            {
                characterVelocityMomentum = Vector3.zero;
            }
        }

        if (characterVelocityMomentumFromWallJump.magnitude >= 0f)
        {
            float wallJumpDrag = 20f;
            characterVelocityMomentumFromWallJump -= characterVelocityMomentumFromWallJump * wallJumpDrag * Time.deltaTime;

            if (characterVelocityMomentumFromWallJump.magnitude < 0f)
            {
                characterVelocityMomentumFromWallJump = Vector3.zero;
            }
        }


        if (hookshotCooldown > 0)
        {
            hookshotCooldown -= Time.deltaTime;
            hookshotCooldownDisplayer.fillAmount = 1 - hookshotCooldown / playerStats.hookshotCooldown;
            //cursorImage.color = Color.red;
            hookshotCooldownDisplayer.color = Color.red;
        }
        else
        {
            hookshotCooldown = 0;
            HandleHookshotStart();
        }

        //HandleParry();

    }


    private void LearnSkill(SkillsEnum _skill)
    {

    }



    private bool IsHaveSkill(SkillsEnum _skill)
    {
        return currentSkills.Contains(_skill);
    }

    public void ChangeSensitivity(float _val)
    {
        sensitivity = _val;
    }

    public void ResumePlaying()
    {
        EnableInputs();
        isPaused = false;
        //cursorImage.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        HandleJumpImage();
    }

    private void HandleJumpImage()
    {
        if (jumpCounterDisplayer == null) return;

        bool isActive = false;

        if (jumpCount < playerStats.jumpLimit && jumpTimer <= 0)
        {
            isActive = true;
        }
        else
        {
            if (isLeftWallOn && currentWall != leftWall)
            {
                isActive = true;
            }
            else if (isRightWallOn && currentWall != rightWall)
            {
                isActive = true;
            }
        }

        jumpCounterDisplayer.gameObject.SetActive(isActive);
    }

    private void TriggerStomp(InputAction.CallbackContext obk)
    {
        if (obk.performed && isGrounded == false)
        {
            if (isStompOn == false)
            {
                characterVelocityMomentum = Vector3.zero;
                characterVelocityMomentumFromWallJump = Vector3.zero;
                if (characterController.isGrounded == false) currentMoveDirection.y = playerStats.maximumFallSpeed;
            }

            isStompOn = !isStompOn;
        }
    }


    private void TriggerEscapeButton(InputAction.CallbackContext obk)
    {
        if (isLedgeJumping == true) return;

        //KickstartGame();

        if (isPaused == true)
        {
            //scrGameManager.manager.GetCanvasManager().ContinuePlaying();
        }
        else
        {
            DisabeInputs();
            //cursorImage.gameObject.SetActive(false);

            isPaused = true;

            //scrGameManager.manager.PauseGame();
        }
    }

    private void HandleCameraLookEvent(InputAction.CallbackContext obk)
    {
        HandleCameraLook();
    }

    private void HandleCameraLook()
    {
        Vector2 _rawInput = mouseAction.ReadValue<Vector2>();

        float mouseX = _rawInput.x * sensitivity * Time.fixedUnscaledDeltaTime * 2;//Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = _rawInput.y * sensitivity * Time.fixedUnscaledDeltaTime * 2;//Input.GetAxis("Mouse Y") * sensitivity;


        if (mouseY != 0)
        {
            if (invertVerticalAimVal == 0)
            {
                rotationX -= mouseY * playerStats.lookSpeed;
            }
            else
            {
                rotationX += mouseY * playerStats.lookSpeed;
            }

            rotationX = Mathf.Clamp(rotationX, playerStats.lookXLimit.y, playerStats.lookXLimit.x);
        }

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, wallDegree);

        if (mouseX != 0)
        {
            transform.rotation *= Quaternion.Euler(0, mouseX * playerStats.lookSpeed, 0);
        }
    }


    #region Hookshot
    private void HandleHookshotStart()
    {
        if (IsHaveSkill(SkillsEnum.Hookshot) == false)
        {
            isHookshotHaveValidTarget = false;
            return;
        }

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit errorControl, playerStats.maxHookRange, playerStats.hookShotErrorLayer))
        {
            Color _magenta = Color.magenta;
            _magenta.a = 0.5f;
            //cursorImage.color = _magenta;
            isHookshotHaveValidTarget = false;
            return;
        }


        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastControl, Mathf.Infinity, scrGameData.values.hookShotLayer))
        {
            if (raycastControl.distance > playerStats.maxHookRange)
            {
                Color _red = Color.red;
                _red.a = 0.5f;
                //cursorImage.color = _red;
                isHookshotHaveValidTarget = false;
            }
            else
            {
                Color _green = Color.green;
                _green.a = 0.5f;
                //cursorImage.color = _green;
                isHookshotHaveValidTarget = true;
            }
        }
        else
        {
            isHookshotHaveValidTarget = false;
            Color _white = Color.white;
            _white.a = 0.5f;
            //cursorImage.color = _white;
        }

        //hookshotCooldownDisplayer.color = cursorImage.color;

    }

    private void TriggerHookshot(InputAction.CallbackContext obk)
    {
        if (isLedgeJumping == true) return;
        if (IsHaveSkill(SkillsEnum.Hookshot) == false) return;
        if (!isHookshotHaveValidTarget) return;
        if (hookshotCooldown > 0) return;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, Mathf.Infinity, scrGameData.values.hookShotLayer))
        {
            //KickstartGame();
            RefreshJump();
            RefreshDashCharge(raycastHit.collider.gameObject, 1);
            currentHookshotTarget = new GameObject().transform;
            //currentHookshotTarget.position = raycastHit.point;
            currentHookshotTarget.position = raycastHit.collider.gameObject.transform.position;
            currentHookshotTarget.SetParent(raycastHit.collider.transform);
            movementState = MovementStateEnum.HookshotFlying;

            characterVelocityMomentum = Vector3.zero;
            characterVelocityMomentumFromWallJump = Vector3.zero;
            DeleteHookshotRenderer();

            anim.ResetTrigger("castEnd");
            anim.SetTrigger("castStart");

            hookshotRenderer = Instantiate(scrGameData.values.hookshotRenderer, transform.position, Quaternion.identity);

            hookshotRenderer.EndPosition = raycastHit.point;
            hookshotCooldown = playerStats.hookshotCooldown;

            anim.SetBool("in air", true);
        }
    }

    private void HandleHookshotMovement()
    {
        float currentHookDistance = Vector3.Distance(transform.position, currentHookshotTarget.position);
        Vector3 hookshotDir = (currentHookshotTarget.position - transform.position).normalized;

        if (currentHookDistance < playerStats.hookshotStopDistance || !currentHookshotTarget.gameObject.transform.parent.gameObject.activeSelf)
        {
            FinishHookshot();
            HookshotMomentum(hookshotDir);
            return;
        }
        Transform _hand = anim.GetBoneTransform(HumanBodyBones.RightHand).GetChild(0).GetChild(0).GetChild(0).transform;
        hookshotRenderer.StartPosition = _hand.position;//rightHand.position - rightHand.forward;
        hookshotRenderer.EndPosition = currentHookshotTarget.position;
        float modifiedHookshotSpeed = Mathf.Clamp(playerStats.hookshotSpeedMultiplier * currentHookDistance, playerStats.hookshotSpeedMin, playerStats.hookshotSpeedMax);
        characterController.Move(hookshotDir * modifiedHookshotSpeed * Time.deltaTime);
    }

    private void FinishHookshot()
    {
        movementState = MovementStateEnum.Normal;
        currentMoveDirection.y = 0;
        if (anim != null)
        {
            anim.ResetTrigger("castStart");
            anim.SetTrigger("castEnd");
        }

        DeleteHookshotRenderer();

        if (currentHookshotTarget) Destroy(currentHookshotTarget.gameObject);
    }

    private void HookshotMomentum(Vector3 _dir)
    {
        characterVelocityMomentum = _dir * playerStats.hookshotSpeedMultiplier * playerStats.momentumExtraSpeed;
        if (currentMoveDirection.y <= 0)
        {
            currentMoveDirection.y = playerStats.jumpSpeed * 1.3f;
        }
        else
        {
            currentMoveDirection.y += playerStats.jumpSpeed * 1.3f;
        }
        characterVelocityMomentum.y = 0;
    }

    private void DeleteHookshotRenderer()
    {
        if (hookshotRenderer)
            Destroy(hookshotRenderer.gameObject);
    }
    #endregion Hookshot

    //private void HandleParry()//TODO:Remove extra variables. Try to fix problem with position change on rotation
    //{
    //    if (Input.GetButton("Fire2"))
    //    {
    //        if (currentParriedProjectile == null)
    //        {
    //            Vector3 _myPos = transform.position;
    //            Collider[] hits = Physics.OverlapSphere(playerCamera.transform.position + (playerCamera.transform.forward * 3), 3f);
    //            for (int i = 0; i < hits.Length; i++)
    //            {
    //                if (hits[i].gameObject.TryGetComponent(out scrProjectile _projectile))
    //                {
    //                    _projectile.Parry();
    //                    currentParriedProjectile = _projectile;
    //                    projectilePos = playerCamera.WorldToScreenPoint(_projectile.transform.position);
    //                    return;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            currentParriedProjectile.transform.position = Vector3.Lerp(currentParriedProjectile.transform.position, playerCamera.ScreenToWorldPoint(projectilePos) + playerCamera.transform.forward * 5, Time.deltaTime * modifiedSpeed / 3);
    //        }

    //        return;
    //    }
    //    ReleaseParry();
    //}

    //private void ReleaseParry()
    //{
    //    if (currentParriedProjectile == null) return;

    //    currentParriedProjectile.transform.forward = Camera.main.transform.forward;
    //    currentParriedProjectile.Release();
    //    currentParriedProjectile = null;
    //}

    //private void OnDrawGizmosSelected()
    //{
    //    GizmosExtend.DrawSphere(playerCamera.transform.position + (playerCamera.transform.forward * 3), 3f, Color.red);
    //}

    private void HandleWallRunTilt(bool _isOn)
    {
        if (_isOn)
        {
            if (isLeftWallOn)
            {
                if (wallDegree > -10)
                {
                    wallDegree -= Time.deltaTime * 20;
                }

                wallGlideTimer = 0;
            }
            else if (isRightWallOn)
            {
                if (wallDegree < 10)
                {
                    wallDegree += Time.deltaTime * 20;
                }

                wallGlideTimer = 0;
            }
            else
            {
                if (wallDegree != 0)
                {
                    wallDegree = Mathf.Lerp(wallDegree, 0, wallGlideTimer);
                    wallGlideTimer = Mathf.Clamp(wallGlideTimer + Time.deltaTime, 0, 1);
                }
                else
                {
                    wallGlideTimer = 0;
                }
                //wallDegree = 0;
            }
        }
        else
        {
            wallDegree = 0;
            if (wallDegree != 0)
            {
                wallDegree = Mathf.Lerp(wallDegree, 0, wallGlideTimer);
                wallGlideTimer = Mathf.Clamp(wallGlideTimer + Time.deltaTime, 0, 1);
            }
            else
            {
                wallGlideTimer = 0;
            }
        }
    }

    public void SetupMasteries(bool _flyingAvalible, int _jumpLimit)
    {
        playerStats.flyingAvalible = _flyingAvalible;
        playerStats.jumpLimit = _jumpLimit;
    }

    //private void KickstartGame()
    //{
    //    if (isFirstMoveDone == true) return;

    //    scrGameManager.manager.KickstartGame();
    //    isFirstMoveDone = true;
    //    Time.timeScale = 1;
    //}

    private void HandleJump(InputAction.CallbackContext obk)
    {
        //KickstartGame();
        jumpQueueTimer = 0.21f;
        if (isLeftWallOn && currentWall != leftWall)
        {
            currentWall = leftWall;
            characterVelocityMomentumFromWallJump = (transform.right * 50);
            ExecuteJump(false, true);
        }
        else if (isRightWallOn && currentWall != rightWall)
        {
            currentWall = rightWall;
            characterVelocityMomentumFromWallJump = (-transform.right * 50);
            ExecuteJump(false, true);
        }
        else if (jumpCount < playerStats.jumpLimit && jumpTimer <= 0)
        {
            if (jumpCount != 0)
            {
                ExecuteJump(true);
            }
            else
            {
                ExecuteJump();
            }

            jumpCount++;
        }

    }

    private void ExecuteJump(bool isExtraJump = false, bool isWallJump = false)
    {
        if (isLedgeJumping == true) return;
        isStompOn = false;
        FinishHookshot();
        anim.SetTrigger("jump");
        jumpQueueTimer = 0;

        jumpTimer = 0.1f;
        float oldVal = currentMoveDirection.y;

        if (currentMoveDirection.y < 0)
        {
            currentMoveDirection.y = 0;
        }


        currentMoveDirection.y += playerStats.jumpSpeed * (isWallJump ? 1.3f : 1) * (isExtraJump ? 1.3f : 1);
    }

    private void RefreshJump()
    {
        jumpCount = 0;
    }

    private void RefreshDashCharge(GameObject _source, int _val)
    {
        if (dashChargeCounter >= 3) return;

        if (dashRefreshObjects.Contains(_source) == true) return;
        dashRefreshObjects.Add(_source);

        dashChargeCounter = Mathf.Clamp(dashChargeCounter + _val, 0, 3);


        for (int i = 0; i < 3; i++)
        {
            if (dashChargeCounter > i)
            {
                dashChargeIcons[i].SetActive(true);
            }
            else
            {
                dashChargeIcons[i].SetActive(false);
            }
        }


        if (dashChargeCounter == 3)
        {
            dashRefreshObjects.Clear();
            return;
        }

    }

    private void RefreshDash()
    {
        if (dashChargeCounter == 3) return;

        dashChargeCounter = 3;

        foreach (GameObject item in dashChargeIcons)
        {
            item.SetActive(true);
        }

        dashRefreshObjects.Clear();
    }

    private void ResetDashFov()
    {
        dashSequence.Kill();
        Camera.main.fieldOfView = playerStats.defaultFov;
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            if (dashTimer > playerStats.dashDuration)
            {
                isDashing = false;
                dashTimer = 0;
                walkingSpeed = playerStats.baseSpeed;
            }
            else
            {
                dashTimer += Time.deltaTime;
                characterController.Move(new Vector3(teleportDirection.x * (playerStats.dashSpeedX / (dashTimer * playerStats.dashSlowMultiplierX)), 0, teleportDirection.z * (playerStats.dashSpeedX / (dashTimer * playerStats.dashSlowMultiplierX))) * Time.deltaTime);
            }
        }
    }

    private void CheckCeiling()
    {
        //Debug.DrawLine(anim.GetBoneTransform(HumanBodyBones.Head).position, anim.GetBoneTransform(HumanBodyBones.Head).position + Vector3.up, Color.blue);

        Ray ray = new Ray(anim.GetBoneTransform(HumanBodyBones.Head).position, Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1, scrGameData.values.ceilingLayer))
        {
            if (currentMoveDirection.y > 0)
            {
                currentMoveDirection.y = Mathf.Clamp(currentMoveDirection.y / -2, playerStats.maximumFallSpeed, 0);
            }
        }

    }

    private void LedgeJump()//TODO:optimize
    {
        if (isRespawning == true) return;

        // Debug.DrawLine(verticalRay.position                           , verticalRay.position + (verticalRay.up * -200), Color.red);
        //Debug.DrawLine(verticalRay.position + (transform.forward/1.5f), (verticalRay.position + (transform.forward/2)) + (verticalRay.up * -200), Color.red);
        // Debug.DrawLine(verticalRay.position - (transform.forward/2), (verticalRay.position - (transform.forward/2)) + (verticalRay.up * -200), Color.red);
        //Debug.DrawLine(verticalRay.position - transform.forward, (verticalRay.position - (transform.forward / 2)) + (verticalRay.up * -200), Color.yellow);
        // Debug.DrawLine(verticalRay.position - (transform.forward * 1.5f), (verticalRay.position - (transform.forward / 2)) + (verticalRay.up * -200), Color.green);

        // Debug.DrawLine(horizontalRay.position, horizontalRay.position + (horizontalRay.forward), Color.blue);
        RaycastHit hitVer = new RaycastHit();
        Ray[] rayVerAr = new Ray[3];
        rayVerAr[0] = new Ray(verticalRay.position - transform.forward, verticalRay.up * -1);
        rayVerAr[1] = new Ray(verticalRay.position - (transform.forward * 0.5f), verticalRay.up * -1);
        rayVerAr[2] = new Ray(verticalRay.position, verticalRay.up * -1);
        //rayVerAr[3] = new Ray(verticalRay.position + (transform.forward / 2), verticalRay.up * -1);


        //Debug.DrawRay(verticalRay.position - transform.forward, verticalRay.up * -1, Color.yellow);
        //Debug.DrawRay(verticalRay.position - (transform.forward * 0.5f), verticalRay.up * -1, Color.black);
        //Debug.DrawRay(verticalRay.position, verticalRay.up * -1, Color.white);
        //Debug.DrawRay(verticalRay.position + (transform.forward / 2), verticalRay.up * -1, Color.cyan);

        //Debug.DrawRay(verticalRay.position - (transform.forward / 2), verticalRay.up * -1, Color.red);

        //Ray rayVer = new Ray(verticalRay.position, verticalRay.up * -1);


        //Ray rayhor = new Ray(horizontalRay.position, horizontalRay.forward);
        RaycastHit hitHor = new RaycastHit();
        Ray[] rarHorArray = new Ray[4];
        rarHorArray[0] = new Ray(horizontalRay.position, horizontalRay.forward);
        rarHorArray[1] = new Ray(horizontalRay.position + transform.up, horizontalRay.forward);
        rarHorArray[2] = new Ray(horizontalRay.position + (transform.up / 2), horizontalRay.forward);
        rarHorArray[3] = new Ray(horizontalRay.position + (transform.up / 1.5f), horizontalRay.forward);

        //Debug.DrawRay(horizontalRay.position, horizontalRay.forward, Color.yellow);
        //Debug.DrawRay(horizontalRay.position + transform.up, horizontalRay.forward, Color.red);
        //Debug.DrawRay(horizontalRay.position + (transform.up /2), horizontalRay.forward, Color.green);
        //Debug.DrawRay(horizontalRay.position + (transform.up / 1.5f), horizontalRay.forward, Color.cyan);

        //if (Physics.Raycast(rayRight, out hit, playerStats.wallDetectRange, playerStats.wallLayer))
        Vector3 _normalDir = Vector3.zero;

        verticalRayTarget = null;
        for (int i = 0; i < rayVerAr.Length; i++)
        {
            if (Physics.Raycast(rayVerAr[i], out hitVer, Mathf.Infinity, platformLayerMask))
            {
                verticalRayTarget = hitVer.collider.gameObject;
                _normalDir = hitVer.normal;
                break;
            }
        }

        if (verticalRayTarget == null) return;

        horizontalRayTarget = null;
        for (int i = 0; i < rarHorArray.Length; i++)
        {
            if (Physics.Raycast(rarHorArray[i], out hitHor, 2, platformLayerMask))
            {
                horizontalRayTarget = hitHor.collider.gameObject;
                break;
            }
        }

        if (horizontalRayTarget == null) return;


        if (verticalRayTarget != horizontalRayTarget) return;

        if (isLedgeJumping == false)
        {
            if (verticalRayTarget != null)
            {
                if (horizontalRayTarget != null)
                {
                    scrInteractive _val = horizontalRayTarget.GetComponent<scrInteractive>();

                    isLedgeJumping = true;
                    characterController.enabled = false;
                    ikController.LedgeJumpAnim();

                    transform.DOMove(hitVer.point + (transform.forward * 2) + (transform.up * 2), 0.1f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        RefreshJump();
                        currentMoveDirection.y = 0;
                        isLedgeJumping = false;
                        characterController.enabled = true;

                        if (_val != null)
                        {
                            if (_val.type == InteractiveType.Jumper)
                            {
                                ExecuteJumper(_val, _normalDir);
                            }
                        }
                    });
                }
            }
        }
    }

    private void TriggerDash(InputAction.CallbackContext obk)
    {
        if (isLedgeJumping == true) return;
        if (isDashing == true) return;
        if (IsHaveSkill(SkillsEnum.Dash) == false) return;
        if (dashChargeCounter != 3) return;
        dashChargeCounter = 0;

        //foreach (GameObject item in dashChargeIcons)
        //{
        //    item.SetActive(false);
        //}

        isDashing = true;
        currentMoveDirection.y = 0;
        teleportDirection = dashDirection;
        teleportDirection.Normalize();
        RefreshJump();

        teleportDirection.y = 0;

        walkingSpeed = playerStats.dashWalkingSpeed;

        ResetDashFov();

        dashSequence = DOTween.Sequence();

        dashSequence.Append(Camera.main.DOFieldOfView(playerStats.dashFov, playerStats.dashFovToInDuration));
        dashSequence.AppendInterval(playerStats.dashFovDuration);
        dashSequence.Append(Camera.main.DOFieldOfView(playerStats.defaultFov, playerStats.dashFovToOffDuration));
    }

    private void CheckWalls()//TODO: Optimize
    {
        RaycastHit hit;

        //Debug.DrawRay(transform.position, transform.right * playerStats.wallDetectRange, Color.yellow);
        //Debug.DrawRay(transform.position, ((transform.right + transform.forward).normalized) * playerStats.wallDetectRange, Color.cyan);
        //Debug.DrawRay(transform.position, ((transform.right + (transform.forward * -1)).normalized) * playerStats.wallDetectRange, Color.blue);


        //Debug.DrawRay(transform.position, -transform.right * playerStats.wallDetectRange, Color.yellow);
        //Debug.DrawRay(transform.position, ((-transform.right + transform.forward).normalized) * playerStats.wallDetectRange, Color.cyan);
        //Debug.DrawRay(transform.position, ((-transform.right + (transform.forward * -1)).normalized) * playerStats.wallDetectRange, Color.blue);


        Ray rayRight = new Ray(transform.position, transform.right);
        Ray rayRightUp = new Ray(transform.position, (transform.right + transform.forward).normalized);
        Ray rayRightDown = new Ray(transform.position, (transform.right + (transform.forward * -1)).normalized);
        if (Physics.Raycast(rayRight, out hit, playerStats.wallDetectRange, scrGameData.values.wallLayer))
        {
            isRightWallOn = true;
            rightWall = hit.collider.gameObject;
            currentWallCollider = hit.collider;
        }
        else if (Physics.Raycast(rayRightUp, out hit, playerStats.wallDetectRange, scrGameData.values.wallLayer))
        {
            isRightWallOn = true;
            rightWall = hit.collider.gameObject;
            currentWallCollider = hit.collider;
        }
        else if (Physics.Raycast(rayRightDown, out hit, playerStats.wallDetectRange, scrGameData.values.wallLayer))
        {
            isRightWallOn = true;
            rightWall = hit.collider.gameObject;
            currentWallCollider = hit.collider;
        }
        else
        {
            rightWall = null;
            isRightWallOn = false;
        }

        Ray rayLeft = new Ray(transform.position, -transform.right);
        Ray rayLeftUp = new Ray(transform.position, (-transform.right + transform.forward).normalized);
        Ray rayLeftDown = new Ray(transform.position, (-transform.right + (transform.forward * -1)).normalized);
        if (Physics.Raycast(rayLeft, out hit, playerStats.wallDetectRange, scrGameData.values.wallLayer))
        {
            leftWall = hit.collider.gameObject;
            currentWallCollider = hit.collider;
            isLeftWallOn = true;
        }
        else if (Physics.Raycast(rayLeftUp, out hit, playerStats.wallDetectRange, scrGameData.values.wallLayer))
        {
            leftWall = hit.collider.gameObject;
            currentWallCollider = hit.collider;
            isLeftWallOn = true;
        }
        else if (Physics.Raycast(rayLeftDown, out hit, playerStats.wallDetectRange, scrGameData.values.wallLayer))
        {
            leftWall = hit.collider.gameObject;
            currentWallCollider = hit.collider;
            isLeftWallOn = true;
        }
        else
        {
            leftWall = null;
            isLeftWallOn = false;
        }
    }
    Quaternion TurretLookRotation(Vector3 approximateForward, Vector3 exactUp)
    {
        Quaternion zToUp = Quaternion.LookRotation(exactUp, -approximateForward);
        Quaternion yToz = Quaternion.Euler(90, 0, 0);
        return zToUp * yToz;
    }

    private void CheckMovingPlatform()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, -transform.up * playerStats.groundDetectRange, Color.yellow);
        Ray myFoot = new Ray(transform.position, -transform.up * playerStats.groundDetectRange);
        if (Physics.Raycast(myFoot, out hit, playerStats.groundDetectRange))
        {
            if (!currentMovingPlatformParent)
            {
                if (hit.collider.gameObject.TryGetComponent(out scrInteractive _val))
                {
                    if (_val.type == InteractiveType.MovingPlatform)
                    {
                        if (_val.interactionValue == 11)
                        {
                            transform.parent = _val.gameObject.transform;
                        }
                        else
                        {
                            transform.parent = _val.gameObject.transform.parent;
                        }

                        currentMovingPlatformParent = _val.gameObject;
                    }
                }
            }
            else
            {
                //transform.up = hit.normal;
                //var targetRotation = TurretLookRotation(transform.forward, hit.normal);
                //isStickyWallOn = true;
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 100 * Time.deltaTime);
            }
        }
        else
        {
            if (currentMovingPlatformParent)
            {
                currentMovingPlatformParent = null;
                transform.parent = null;
                transform.localScale = startingScale;
            }
            isStickyWallOn = false;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), 0.1f);
        }

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isRespawning) return;

        //if (hit.gameObject.CompareTag("Checkpoint Platform"))
        //{
        //    RefreshDash();
        //}

        if (hit.gameObject.CompareTag("Enemy"))
        {
            jumpQueueTimer = 0;
            GetHit();
            return;
        }

        if (!characterController.isGrounded && hit.normal.y < 0.1f)
        {
            Debug.DrawRay(hit.point, hit.normal, Color.blue, 2f);
        }

        if (hit.gameObject.TryGetComponent(out scrInteractive _val))
        {

            if (_val.type == InteractiveType.Jumper)
            {
                if (hit.normal.y > 0.3f)
                {
                    ExecuteJumper(_val, hit.transform.forward);
                }
                else
                {
                    print(hit.normal.y);
                }


            }
            else if (_val.type == InteractiveType.SpecialJumper)
            {
                if (lastSpecialJumper != null)
                {
                    if (lastSpecialJumper == hit.gameObject) return;

                    lastSpecialJumper = hit.gameObject;

                }
                else
                {
                    lastSpecialJumper = hit.gameObject;
                }

                Vector3 dir = (hit.normal * _val.interactionValue) + transform.position;
                RefreshJump();

                transform.DOMove(dir, 1).OnComplete(() => {
                    HandleSpecialJump();
                });
            }
        }
    }

    private void ExecuteJumper(scrInteractive _val, Vector3 _dir)
    {
        isStompOn = false;
        jumpTimer = 0.1f;
        float jumpMultiplier = 1;
        if (characterController.velocity.y < 0)
        {
            jumpMultiplier = Mathf.Clamp(0.9f + (characterController.velocity.y * -0.007f), 1, 1.6f);
            jumperMultiplierInfo = jumpMultiplier;
        }
        else
        {
            jumperMultiplierInfo = 0;
        }

        currentMoveDirection.y = playerStats.jumpSpeed * _val.interactionValue * 2 * jumpMultiplier;
        characterVelocityMomentum = playerStats.jumpSpeed * _val.interactionValue * jumpMultiplier * _dir * 15;

        characterVelocityMomentum.y = 0;
        //print("_dir: " + _dir + " CVM:" + characterVelocityMomentum + " CMD" + currentMoveDirection);
        RefreshJump();
    }

    private void HandleSpecialJump()
    {
        lastSpecialJumper = null;
    }

    private void TeleportPreparations()
    {
        isRespawning = true;
        isStompOn = false;
        DisabeInputs();
        RefreshDash();
        RefreshJump();
        FinishHookshot();
        ToggleBody(false);
        jumpQueueTimer = 0;
        characterController.enabled = false;
        transform.parent = null;
        currentMoveDirection = Vector3.zero;
        characterVelocityMomentum = Vector3.zero;
        characterVelocityMomentumFromWallJump = Vector3.zero;
    }

    public void TeleportBack(bool _isForced)
    {
        if (isRespawning) return;
        TeleportPreparations();

        if (_isForced == true)
        {
            transform.rotation = Quaternion.Euler(startingRot.eulerAngles);

            playerCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);

            transform.position = startingPos;

            rotationX = 0;
            currentMoveDirection.y = 0;
            characterController.enabled = true;
            isRespawning = false;
            EnableInputs();
            //anim.SetFloat("Action Speed", 1);
            ToggleBody(true);
        }
        else
        {
            Camera.main.DOShakeRotation(DEAD_DELAY).OnComplete(() =>
            {
                Camera.main.DOShakeRotation(playerStats.respawnCamShakeDuration, playerStats.respawnCamShakePower).OnComplete(() => {
                    transform.DORotate(startingRot.eulerAngles, playerStats.respawnSpeed);
                    playerCamera.transform.DOLocalRotate(Vector3.zero, playerStats.respawnSpeed);
                });

                transform.DOMove(startingPos, playerStats.respawnSpeed).OnComplete(() => {
                    rotationX = 0;
                    currentMoveDirection.y = 0;
                    characterController.enabled = true;
                    isRespawning = false;
                    EnableInputs();
                    //anim.SetFloat("Action Speed", 1);
                    ToggleBody(true);
                });
            });
        }
    }

    public void TeleportToCheckpoint(Transform _checkpoint, bool _isForced)
    {
        if (isRespawning) return;
        TeleportPreparations();

        if (_isForced == true)
        {
            transform.rotation = Quaternion.Euler(_checkpoint.eulerAngles);
            playerCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);

            transform.position = _checkpoint.position;

            rotationX = 0;
            currentMoveDirection.y = 0;
            characterController.enabled = true;
            isRespawning = false;
            EnableInputs();
            //anim.SetFloat("Action Speed", 1);
            ToggleBody(true);
        }
        else
        {
            Camera.main.DOShakeRotation(DEAD_DELAY).OnComplete(() =>
            {
                Camera.main.DOShakeRotation(playerStats.respawnCamShakeDuration, playerStats.respawnCamShakePower).OnComplete(() => {
                    transform.DORotate(_checkpoint.eulerAngles, playerStats.respawnSpeed);
                    playerCamera.transform.DOLocalRotate(Vector3.zero, playerStats.respawnSpeed);
                });

                transform.DOMove(_checkpoint.position, playerStats.respawnSpeed).OnComplete(() => {
                    rotationX = 0;
                    currentMoveDirection.y = 0;
                    characterController.enabled = true;
                    isRespawning = false;
                    EnableInputs();
                    //anim.SetFloat("Action Speed", 1);
                    ToggleBody(true);
                });
            });
        }

    }

    private void ToggleBody(bool _enable)
    {
        anim.gameObject.SetActive(_enable);
    }

    private void OnTriggerEnter(Collider other)
    {
        /*
        if (isRespawning) return;

        if (other.gameObject.TryGetComponent(out scrCheckpoint cp))
        {
            scrGameManager.manager.Checkpoint(cp);
            RefreshDash();
        }

        if (other.gameObject.TryGetComponent(out scrPortal portal))
        {
            if (portal.CanTeleport())
            {
                scrGameManager.manager.TeleportToLevel(portal.portalDestinationName);
            }
        }

        if (other.CompareTag("Enemy"))
        {
            GetHit();
        }

        if (other.gameObject.TryGetComponent(out IIntractable _val))
        {
            _val.Interact();
        }

        if (other.CompareTag("End Portal"))
        {
            if (other.TryGetComponent(out scrLevelEndPortal _endPortal))
            {
                scrGameManager.manager.EndPortal(_endPortal.portalType);
                this.enabled = false;
            }
            else
            {
                characterController.enabled = false;
                transform.position = endPoint.transform.position;
                transform.forward = endPoint.transform.forward;

                rotationX = 0;

                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                characterController.enabled = true;
                scrGameManager.manager.TeleportToPurgatory();
                isFinishedLevel = true;
            }
        }
        */
    }

    public void GetHit()
    {
        //speaker.Stop();
        //speaker.pitch = Random.Range(0.95f, 1.1f);
        //speaker.PlayOneShot(scrGameData.values.getHitSounds[Random.Range(0, scrGameData.values.getHitSounds.Length)]);
        //scrGameManager.manager.PlayerGetHit(false);
        //scrGameManager.manager.GetCanvasManager().OpenDeadScreen(DEAD_DELAY, playerStats.respawnSpeed);
        //isStompOn = false;
    }

    public void EndLevelTrigger()
    {
        //isEndLevelTrigger = true;
    }

    private void RayTargeting()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, playerStats.interactionRange, scrGameData.values.interactionLayer))
        {
            if (hit.collider.gameObject.TryGetComponent(out IIntractable _val))
            {
                _val.Interact();
            }
        }
    }

    private enum MovementStateEnum
    {
        Normal,
        HookshotFlying
    }


}


