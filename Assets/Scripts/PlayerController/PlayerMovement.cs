using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using JSAM;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviourPunCallbacks, IDamageable
{
    public GameObject bloodSplat;

    [Header("Death Catch")]
    public bool isDead = false;
    public GameObject GunCamera;

    public Transform HipPosition;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    [HideInInspector] public bool isMovingBackwards;
    [HideInInspector] public bool isStrafing;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Ground Check")]
    public Transform GroundCheck;
    public float GroundDistance;
    public LayerMask Enviroment;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Backend")]
    public GameObject CameraRig;
    private CameraRigManager cRigManager;

    public Animator animator;
    public Transform orientation;
    public Transform headPos;
    public GravityGun gravGun;
    public BuildSystem buildSys;

    [Header("UI")]
    public GameObject uiCanvas;
    public GameObject endGameCanvas;
    public TMP_Text healthNum;
    public Image healthCircle;
    public Image healthLine;
    public float circlePercent = 0.3f;
    private const float circleFillAmount = 0.75f;
    public GameObject overheadCanvas;
    public Image overheadUIHealth;
    public GameObject deathMessageItem;
    public GameObject deathPrompt;
    public Transform deathBoard;

    [Space(15)]
    public UnityEngine.Rendering.Volume volume;
    private UnityEngine.Rendering.VolumeProfile volumeProf;
    private UnityEngine.Rendering.Universal.Vignette vignette;

    [Header("UI Backend")]
    public GameObject UI_Game;
    public GameObject UI_WeaponSelect;
    public GameObject UI_Pause;
    [HideInInspector] public bool isInWeaponSelect = false;
    [HideInInspector] public bool isInPauseMenu = false;

    [Header("Emotes")]
    public GameObject emoteMenu;
    public MeshRenderer playerFace;
    public bool isEmote = false;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    [HideInInspector] public PhotonView PV;
    PlayerVisualsManager PVM;

    [HideInInspector] public PlayerManager playerManager;

    const float maxHealth = 250f;
    float currentHealth = maxHealth;
    string deathMessage;

    Transform emotePos;
    bool emoteMusicPlaying;

    Sounds emoteMusic;

    //
    // Testing
    //
    public Hitmarker HM;
    public ScoreUp scoreUp;


    public MovementState state;

    public Texture2D[] playerFaces;
    public enum MovementState
    {
        walking, sprinting, air
    }

    private void Awake()
    {
        PV = GetComponentInParent<PhotonView>();
        PVM = GetComponentInChildren<PlayerVisualsManager>();
        rb = GetComponent<Rigidbody>();

        volumeProf = volume.profile;

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
        if (!PV.IsMine) //Other players objects
        {
            Destroy(CameraRig);
            Destroy(rb);
            Destroy(uiCanvas);

            return;
        }
        volumeProf.TryGet(out vignette);

        GameObject.Find("Audio Manager").GetComponent<AudioManager>().listener = this.transform.parent.gameObject.GetComponentInChildren<AudioListener>();

        overheadCanvas.SetActive(false);

        rb.freezeRotation = true;
        readyToJump = true;

        healthNum.text = currentHealth + " HP";
        cRigManager = CameraRig.GetComponent<CameraRigManager>();
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine || isDead || playerManager.isPaused || isEmote)
            return;

        if (vignette.intensity.value > 0)
            vignette.intensity.value -= .015f;

        Move();
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        SpeedControl();

        grounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, Enviroment);

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0; 
        }
        Vector3 tempHolder = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (!grounded)
        {
            animator.SetBool("Fall", true);
            animator.SetBool("Run", false);
        }
        else if(tempHolder.magnitude > 0.7f && grounded)
        {
            animator.SetBool("Run", true);
            animator.SetBool("Fall", false);
        }
        else
        {
            animator.SetBool("Fall", false);
            animator.SetBool("Run", false);
        }

        StateHandle();
        UpdateHealthBar();

        //Temp
        if (transform.position.y < -15f)
        {
            TakeDamage(1000f);
        }
        if(!ChatManager.Instance.isChatting || !isInPauseMenu)
            GetInput();
    }

    void GetInput()
    {
        if(ChatManager.Instance.isChatting || isInPauseMenu)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            isInWeaponSelect = !isInWeaponSelect;

            if (isInWeaponSelect)
            {
                //Enter Select Menu
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                UI_Game.SetActive(false);
                UI_WeaponSelect.SetActive(true);
            }
            else
            {
                //Exit Pause Menu
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                UI_Game.SetActive(true);
                UI_WeaponSelect.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(isInWeaponSelect)
            {
                isInWeaponSelect = !isInWeaponSelect;

                //Exit Pause Menu
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                UI_Game.SetActive(true);
                UI_WeaponSelect.SetActive(false);
                return;
            }

            ////////////////////////////////
            
            isInPauseMenu = !isInPauseMenu;

            if (isInPauseMenu)
            {
                //Enter Pause Menu
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                UI_Pause.SetActive(true);

                if(PhotonNetwork.LocalPlayer.CustomProperties["GameMode"].ToString() == "Sandbox")
                {
                    UI_Pause.transform.Find("Pause Menu UI/ButtonContainer/Save Level Layout").GetComponent<Button>().interactable = true;
                }
            }
            else
            {
                //Exit Pause Menu
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                UI_Pause.SetActive(false);
            }
        }

        if (isDead || playerManager.isPaused || isInWeaponSelect || isInPauseMenu || ChatManager.Instance.isChatting)
            return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) && readyToJump && grounded)
        {
            Jump();
        }

        if(Input.GetKeyDown(KeyCode.G) && !emoteMenu.activeInHierarchy && !isEmote && grounded)
        {
            emoteMenu.SetActive(true);
        }

        if (animator.GetBool("Fall") == true && isEmote || animator.GetBool("Run") == true && isEmote || !grounded && isEmote || Input.GetKeyDown(KeyCode.G) && isEmote)
        {
            PV.RPC(nameof(EndEmote), RpcTarget.AllBuffered);
            if(!isDead)
                cRigManager.ChangeCamera(0);
            PVM.DisableModel();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage(75);
        }        
    }

    public void ResumeGame()
    {
        isInPauseMenu = false;

        //Exit Pause Menu
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UI_Pause.SetActive(false);
    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(0);
    }

    //
    //
    //Glorp crubgle is int he buulding baby you know how we get shi done

    void StateHandle()
    {
        if(grounded && Input.GetKey(KeyCode.LeftShift) && !isEmote)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            animator.speed = 2;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            animator.speed = 1;
        }
        else
        {
            state = MovementState.air;
        }
    }

    void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        Vector3 movement = moveDirection.normalized * moveSpeed;

        float forwardAmount = Vector3.Dot(orientation.forward, movement);

        if (forwardAmount < -.5f)
        {
            isMovingBackwards = true;
            isStrafing = false;
        }

        else if (forwardAmount < .5f)
        {
            isMovingBackwards = false;
            isStrafing = true;
        }

        else
        {
            isMovingBackwards = false;
            isStrafing = false;
        }

        float playerSpeed = moveSpeed;
        if (isMovingBackwards) playerSpeed /= 2f;
        if (isStrafing) playerSpeed /= 1.5f;

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * playerSpeed * 20f, ForceMode.Force);
        }

        if(grounded)
            rb.AddForce(moveDirection.normalized * playerSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * playerSpeed * airMultiplier * 10f, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        readyToJump = false;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);
    }
    void ResetJump()
    {
        readyToJump = true;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public void ApplyForce(Vector3 dir, float power, float upForce)
    {
        PV.RPC(nameof(RPC_ApplyForce), RpcTarget.All, dir, power, upForce);
    }
    [PunRPC]
    void RPC_ApplyForce(Vector3 dir, float power, float upForce)
    {
        if (!PV.IsMine)
            return;

        Vector3 dirFixed = new Vector3(dir.x, 0f, dir.z);
        //Vector3 dirFixed = new Vector3(dir.x, Mathf.Abs(dir.y), dir.z);
        transform.position += Vector3.up * 0.5f;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce((power * dirFixed) + Vector3.up * upForce, ForceMode.Impulse);
    }

    public void TakeDamage(float damage) //Local run
    {
        if (isDead)
            return;

        PV.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info) //Server run
    {
        if (isDead)
            return;
        
        currentHealth -= damage;

        GameObject t_Blood = Instantiate(bloodSplat, HipPosition.position, Quaternion.identity);
        Destroy(t_Blood, 20f);

        overheadUIHealth.fillAmount = currentHealth / maxHealth;

        if(currentHealth <= 0)
        {
            playerFace.material.SetTexture("_MainTex", playerFaces[1]);
        }

        if (!PV.IsMine) //Adjusts to Local run
            return;

        if(info.Sender != PhotonNetwork.LocalPlayer)
        {
            PlayerManager.Find(info.Sender).getDamage((int)damage);
        }

        JSAM.AudioManager.PlaySound(Sounds.takeDamage);

        healthCircle.transform.parent.gameObject.GetComponent<Animator>().Play("Damage");
        vignette.intensity.Override(.5f);

        UpdateHealthBar();

        if(currentHealth <= 0)
        {
            healthNum.text = "Dead!";
            KillPlayer(rb.velocity);

            if (gravGun.enabled && gravGun.grabbedRB != null)
                gravGun.stopGrabbing = true;

            buildSys.isBuilding = false;

            if (info.Sender != PhotonNetwork.LocalPlayer)
            {
                PlayerManager.Find(info.Sender).getDamage(100);
            }
            PV.RPC(nameof(RPC_SendDeathMessage), RpcTarget.All, PlayerManager.Find(info.Sender).GetComponent<PhotonView>().Owner.NickName.ToString(), PhotonNetwork.NickName);
            Instantiate(deathPrompt, uiCanvas.transform).GetComponent<DeathPrompt>().Initialize(info.Sender);
        }
        else
        {
            healthNum.text = currentHealth + " HP";
        }
    }

    [PunRPC]
    void RPC_SendDeathMessage(string killer, string victim)
    {
        GameObject dm = Instantiate(deathMessageItem);
        dm.GetComponent<DeathMessage>().Initialize(killer, victim);
    }


    [PunRPC]
    public void PlayEmoteAnimation(string _animName)
    {
        if(isDead)
        {
            return;
        }
        animator.SetLayerWeight(1, 0);
        animator.Play(_animName);

        if(gravGun.enabled && gravGun.grabbedRB != null)
            gravGun.stopGrabbing = true;

        if (!emoteMusicPlaying)
        {
            switch (_animName)
            {
                case "Funnyman Dance":
                    emoteMusic = Sounds.funnymandanceemote;
                    JSAM.AudioManager.PlaySoundLoop(emoteMusic, headPos);
                    emoteMusicPlaying = true;
                    break;
                case "Funnyman Shuffle":
                    emoteMusic = Sounds.funnymanshuffleemote;
                    JSAM.AudioManager.PlaySoundLoop(emoteMusic, headPos);
                    emoteMusicPlaying = true;
                    break;
                case "Hadoken":
                    emoteMusic = Sounds.ryutheme;
                    JSAM.AudioManager.PlaySoundLoop(emoteMusic, headPos);
                    emoteMusicPlaying = true;
                    break;
                case "Seizure":
                    emoteMusic = Sounds.seizure;
                    JSAM.AudioManager.PlaySoundLoop(emoteMusic, headPos);
                    emoteMusicPlaying = true;
                    break;
                case "Stewie":
                    emoteMusic = Sounds.stewie;
                    JSAM.AudioManager.PlaySoundLoop(emoteMusic, headPos);
                    emoteMusicPlaying = true;
                    break;
                default:
                    break;
            }
        }

        if (!PV.IsMine) // Return to Local Run
        {
            return;
        }

        PVM.EnableModel();
        PVM.CurrentEquip.GetComponent<MeshRenderer>().enabled = false;
        PVM.CRM.ChangeCamera(1);
        if(JSAM.AudioManager.GetMusicVolumeAsInt() <= 5)
        {
            JSAM.AudioManager.SetMusicVolume(0);
        }
        else
        {
            JSAM.AudioManager.SetMusicVolume(5);
        }

        isEmote = true;
    }

    [PunRPC]
    public void EndEmote()
    {
        isEmote = false;
        if (emoteMusicPlaying)
        {
            JSAM.AudioManager.StopSoundLoop(emoteMusic, headPos, true);
            emoteMusicPlaying = false;
        }
        animator.Play("Breathing Idle");
        animator.SetLayerWeight(1, 1);

        if (!PV.IsMine) // Return to Local Run
        {
            return;
        }

        if(!isDead)
        {
            PVM.DisableModel();
        }

        PVM.CurrentEquip.GetComponent<MeshRenderer>().enabled = true;

        if(!isDead)
            PVM.CRM.ChangeCamera(0);

        JSAM.AudioManager.SetMusicVolume(PlayerPrefs.GetFloat("musicvolume"));
    }

    public void DisableUI()
    {
        UI_Game.SetActive(false);
    }

    public void EnableUI()
    {
        UI_Game.SetActive(true);
    }

    void UpdateHealthBar()
    {
        float healthPercentage = currentHealth / maxHealth;
        float circleFill = healthPercentage / circlePercent;

        circleFill *= circleFillAmount;

        circleFill = Mathf.Clamp(circleFill, 0, circleFillAmount);

        healthCircle.fillAmount = circleFill;

        //----------------------------------------------------------

        float circleAmount = circlePercent * maxHealth;

        float lineHealth = currentHealth - circleAmount;
        float lineTotalHealth = maxHealth - circleAmount;
        float lineFill = lineHealth / lineTotalHealth;

        lineFill = Mathf.Clamp(lineFill, 0, 1);

        healthLine.fillAmount = lineFill;
    }

    void KillPlayer(Vector3 vel)
    {
        isDead = true;

        Destroy(GunCamera);

        playerManager.KillPlayer(vel);
    }
}
