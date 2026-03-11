using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PlayerMovement : Selectable, IFreezable, IResetable
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
    // Track colliders that are considered ground (only collisions with an upward normal)
    private HashSet<Collider> groundColliders = new HashSet<Collider>();
    private bool isSprinting;
    private float xRotation = 0f;
    private float currentAnimatorSpeed = 0f;
    private float speedLerpDuration = 0.5f;

    public Selectable seenObject;
    public Item HoldingItem;

    public int FreezeAmmo = 0;
    public int ContinueAmmo = 0;
    public Renderer[] displays;
    private float frozegunTimer = 0f;
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

    public bool IsFrozen { get; set; }
    private Vector3 originPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        originPosition = transform.position;
        //setoriginal materials



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
        foreach (Renderer r in displays)
        {
            originalMaterials[r] = r.material;
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
        unregister();
    }


    protected override void Update()
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
        if (!IsFrozen)
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
        frozegunTimer = 0.45f / 0.6f;
        animator.SetTrigger("trigger_freeze");
        yield return new WaitForSeconds(0.45f);
        if (seenObject is IFreezable f)
        {
            if (seenObject is Item i && i.isPickedUp) yield break;
            if (f.IsFrozen)
            {
                if (ContinueAmmo < 1)
                {
                    GameCore.Instance.displayStatusText("You run out of Recover Ammo!");

                }
                else
                {
                    f.onUnfreeze();
                    ContinueAmmo--;
                    GameCore.Instance.continueAmmoText.text = ContinueAmmo.ToString();

                }

            }
            else
            {
                if (FreezeAmmo < 1)
                {
                    GameCore.Instance.displayStatusText("You run out of Time Freeze Ammo!");

                }
                else
                {
                    f.onFreeze();
                    FreezeAmmo--;
                    GameCore.Instance.freezeAmmoText.text = FreezeAmmo.ToString();
                }

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
        GameCore.Instance.freezeAmmoText.text = FreezeAmmo.ToString();
        GameCore.Instance.continueAmmoText.text = ContinueAmmo.ToString();
        togglePlayerElements(true);
        //yield return WaitForTransitionAnimation("switchtimeline", -1, 1);


    }
    void FixedUpdate()
    {
        if (!IsFrozen)
        {
            HandleMovement();
            checkLookat();
        }

    }

    private void checkLookat()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;
        Selectable before = seenObject;
        // Cast all hits and pick the first one that is not part of this player (or its children)
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, GameCore.Instance.SelectableItems);
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            RaycastHit? valid = null;
            foreach (var h in hits)
            {
                if (h.collider == null) continue;
                if (h.collider.transform.IsChildOf(transform)) continue; // ignore self
                valid = h;
                break;
            }
            if (valid.HasValue)
            {
                hit = valid.Value;
                seenObject = hit.collider.GetComponent<Selectable>();
                if (seenObject == null) return;
            }
            else
            {
                seenObject = null;
            }
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
                if (before is Item)
                {
                    GameCore.Instance.interaction_pickup.SetActive(true);

                }

                if (before is IFreezable f)
                {
                    if (f is Item i && i.isPickedUp)
                    {

                    }
                    else
                    {
                        GameCore.Instance.interaction_freeze.SetActive(true);

                    }
                    if (f.IsFrozen)
                    {
                        GameCore.Instance.freezeText.text = "unfreeze";
                        GameCore.Instance.interaction_pickup.SetActive(false);

                    }
                    else
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
        // Use unscaled delta so look sensitivity is not affected by Time.timeScale changes
        float mouseX = lookInput.x * mouseSensitivity * Time.unscaledDeltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.unscaledDeltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        playerHead.transform.localRotation = Quaternion.Euler(-xRotation + 90, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }



    private void OnCollisionEnter(Collision collision)
    {
        // Consider this collider ground only if any contact has a sufficiently upward normal.
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.65f)
            {
                groundColliders.Add(collision.collider);
                break;
            }
        }
        isGrounded = groundColliders.Count > 0;

    }

    private void OnCollisionExit(Collision collision)
    {

        // Remove the collider from ground set when collision ends
        groundColliders.Remove(collision.collider);
        isGrounded = groundColliders.Count > 0;

    }

    void HandleMovement()
    {
        Vector3 moveDirection = transform.right * -moveInput.x + transform.forward * -moveInput.y;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        //if (isGrounded)
        //{

        //}
        //else
        //{
        //    Vector3 airVelocity = moveDirection * currentSpeed * airControl;
        //    rb.AddForce(airVelocity, ForceMode.Force);
        //}
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
        float targetSpeed = Mathf.Min(rb.linearVelocity.magnitude / walkSpeed, 1f);
        currentAnimatorSpeed = Mathf.Lerp(currentAnimatorSpeed, targetSpeed, Time.fixedDeltaTime / speedLerpDuration);
        //animator.SetFloat("speed", currentAnimatorSpeed);
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void onFreeze()
    {
        IsFrozen = true;
        foreach (Renderer renderer in displays)
        {
            renderer.material = GameCore.Instance.BlackWhiteMat;
        }
        rb.isKinematic = true;
    }

    public void onUnfreeze()
    {
        IsFrozen = false;
        foreach (Renderer renderer in displays)
        {
            renderer.material = originalMaterials[renderer];
        }
        rb.isKinematic = false;
    }

    public void onReset()
    {
        IsFrozen = false;
        HoldingItem = null;
        foreach (Renderer renderer in displays)
        {
            renderer.material = originalMaterials[renderer];
        }
        Debug.Log($"[onReset] PlayerMovement.onReset called for {name}. originPosition={originPosition}\n" + new System.Diagnostics.StackTrace(true).ToString());
        transform.position = originPosition;
        FreezeAmmo = 0;
        ContinueAmmo = 0;
        GameCore.Instance.freezeAmmoText.text = "0";
        GameCore.Instance.continueAmmoText.text = "0";
        resetUI();
        if(gameObject.name == "Player")
        {
            EnablePlayer();
        } else
        {
            DisablePlayer();
        }
    }
}
