using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 10f;
    public float airControl = 0.3f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 90f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private PlayerInput playerInput;
    private Rigidbody rb;
    private Animator animator;
    [SerializeField]
    private GameObject playerHead;
    [SerializeField]
    public Camera cam;
    [SerializeField]
    private GameObject FPonly;
    public Vector2 moveInput;
    public Vector2 lookInput;
    private bool isGrounded;
    private bool isSprinting;
    private float xRotation = 0f;
    private float currentAnimatorSpeed = 0f;
    private float speedLerpDuration = 0.5f;

    public Selectable seenObject;
    public Item HoldingItem;

    public Renderer[] displays;
    private float frozegunTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        resetUI();
    }
    public void Init(bool clone)
    {
        gameObject.name = clone ? "Clone" : "Player";
        playerInput = GameCore.Instance.playerInput;
        if (clone)
        {
            GameCore.Instance.clone = this;
            togglePlayerElements(false);
            perm_changematerial(GameCore.Instance.cloneMat);
        }
        else
        {
            GameCore.Instance.player = this;
            togglePlayerElements(true);

        }


    }
    private void resetUI()
    {
        GameCore.Instance.interaction_pickup.SetActive(false);
        GameCore.Instance.interaction_freeze.SetActive(false);

    }
    private void register()
    {
        GameCore.Instance.currentPlayer = this;
        print("REGISTER INPUT IN " + gameObject.name);
        playerInput.player.jump.performed += OnJump;
        playerInput.player.pickup.performed += pickUpSelected;
        playerInput.player.freeze.performed += StartUseFreezeGun;
    }
    private void unregister()
    {
        print("UNREGISTER INPUT IN " + gameObject.name);

        playerInput.player.jump.performed -= OnJump;
        playerInput.player.pickup.performed -= pickUpSelected;
        playerInput.player.freeze.performed -= StartUseFreezeGun;

    }

    void OnDisable()
    {
        animator.SetFloat("speed", 0f);
    }


    void Update()
    {
        moveInput = playerInput.player.movement.ReadValue<Vector2>();
        lookInput = playerInput.player.look.ReadValue<Vector2>();
        isSprinting = playerInput.player.sprint.IsPressed();
            if (frozegunTimer > 0)
            {
                frozegunTimer -= Time.deltaTime;
        }

    }
    public void perm_changematerial(Material nm)
    {
        foreach (Renderer mr in displays)
        {
            mr.material = nm;
        }
    }
    private void LateUpdate()
    {
        HandleMouseLook();

    }
    private void pickUpSelected(InputAction.CallbackContext context)
    {
        if (HoldingItem == null)
        {
            if (seenObject is IFreezable f && f.IsFrozen) return;

            if (seenObject is Item hold && !hold.isPickedUp)
            {
                HoldingItem = hold;
                HoldingItem.onPickUp(this);
                animator.SetBool("picking", true);
                GameCore.Instance.interaction_pickup.SetActive(true);
                GameCore.Instance.interaction_freeze.SetActive(false);
                GameCore.Instance.pickupText.text = "drop";
            }

        }
        else
        {
            HoldingItem.onDrop();
            HoldingItem = null;
            animator.SetBool("picking", false);
            GameCore.Instance.pickupText.text = "pick up";

        }
    }
    private void StartUseFreezeGun(InputAction.CallbackContext con)
    {
        StartCoroutine(useFreezeGun(con));
    }
    private IEnumerator useFreezeGun(InputAction.CallbackContext context)
    {
        if (frozegunTimer > 0) yield break;
        frozegunTimer = 0.45f/0.6f;
        animator.SetTrigger("trigger_freeze");
        yield return new WaitForSeconds(0.45f);
        if (seenObject is IFreezable f)
        {
            if (seenObject is Item i && i.isPickedUp) yield break;
            if (f.IsFrozen)
            {
                f.onUnfreeze();
            }
            else
            {
                f.onFreeze();
            }
        }
    }

    public IEnumerator DisablePlayer()
    {
        resetUI();
        yield return WaitForTransitionAnimation("switchtimeline", 1, 0);
        togglePlayerElements(false);
    }
    private void togglePlayerElements(bool b)
    {
        //foreach child in cam, set active to b
        cam.enabled = b;
        cam.GetComponent<AudioListener>().enabled = b;
        FPonly.SetActive(b);
        this.enabled = b;
        //toggle all skinned mesh renderer in children
        foreach (Renderer g in displays)
        {
            g.enabled = !b;
        }
        if (b)
        {
            register();
        }
        else
        {
            unregister();

        }


    }
    private IEnumerator WaitForTransitionAnimation(string animationname, float speed, float start)
    {
        if (animator != null)
        {
            animator.Play(animationname, 0, start);
            yield return new WaitForSeconds(0.1f);
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            animator.speed = speed;

            yield return new WaitForSeconds(stateInfo.length);
        }
        animator.speed = 1f;
    }
    public void EnablePlayer()
    {
        togglePlayerElements(true);
        //yield return WaitForTransitionAnimation("switchtimeline", -1, 1);


    }
    void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        checkLookat();
    }
    
    private void checkLookat()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        Selectable before = seenObject;
        if (Physics.Raycast(ray, out hit, 100f, GameCore.Instance.SelectableItems))
        {

            seenObject = hit.collider.GetComponent<Selectable>();
            if (seenObject == null) return;

        }
        else
        {
            seenObject = null;
        }
        UpdateSeenObject(before, before == seenObject);

    }
    private void UpdateSeenObject(Selectable before, bool lookedat)
    {
        if (before != null)
        {
            if (lookedat)
            {

                before.onLookedAt();
                GameCore.Instance.interaction_pickup.SetActive(true);

                if (before is IFreezable f)
                {
                    if(f is Item i && i.isPickedUp)
                    {

                    } else
                    {
                        GameCore.Instance.interaction_freeze.SetActive(true);

                    }
                    if (f.IsFrozen)
                    {
                        GameCore.Instance.freezeText.text = "unfreeze";
                        GameCore.Instance.interaction_pickup.SetActive(false);

                    } else
                    {
                        GameCore.Instance.freezeText.text = "freeze";
                    }

                } 
            }
            else
            {
                before.onLookedAway();
                GameCore.Instance.interaction_pickup.SetActive(false);
                GameCore.Instance.interaction_freeze.SetActive(false);

            }
        }

    }


    void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        playerHead.transform.localRotation = Quaternion.Euler(-xRotation + 90, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void HandleMovement()
    {
        Vector3 moveDirection = transform.right * -moveInput.x + transform.forward * -moveInput.y;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if (isGrounded)
        {
            Vector3 targetVelocity = moveDirection * currentSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            Vector3 airVelocity = moveDirection * currentSpeed * airControl;
            rb.AddForce(airVelocity, ForceMode.Force);
        }

        float targetSpeed = Mathf.Min(rb.linearVelocity.magnitude / walkSpeed, 1f);
        currentAnimatorSpeed = Mathf.Lerp(currentAnimatorSpeed, targetSpeed, Time.fixedDeltaTime / speedLerpDuration);
        animator.SetFloat("speed", currentAnimatorSpeed);
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
