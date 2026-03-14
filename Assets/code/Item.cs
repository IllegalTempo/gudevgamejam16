using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Item : Selectable, IFreezable, IResetable
{
    private Transform originalParent;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private int Type;
    public bool isPickedUp = false;

    public bool IsFrozen { get; set; } = false;
    private Dictionary<MeshRenderer, Material> originalMaterial = new Dictionary<MeshRenderer, Material>();
    private PlayerMovement pickuped;

    [SerializeField]
    private MeshRenderer[] renderers;

    private Vector3 initpos;
    private Quaternion initRot;
    public AudioSource source;

    public ParticleSystem pickupeffect;
    private void Start()
    {
        initpos = transform.position;
        initRot = transform.rotation;

        originalParent = transform.parent;
        foreach (MeshRenderer mr in renderers)
        {
            originalMaterial[mr] = mr.material;
        }

    }
    protected override void Update()
    {
        base.Update();
        if (pickuped != null)
        {
            rb.MovePosition(pickuped.cam.transform.position + pickuped.cam.transform.forward * 10);
        }
        //when freezed, keep playing gamecore.instance.freeze_ambient

    }
    public void onPickUp(PlayerMovement who)
    {
        //transform.parent = who.cam.transform;
        //transform.localPosition = new Vector3(0, 0, 3);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        //rb.isKinematic = true;
        isPickedUp = true;
        pickuped = who;
        pickupeffect.gameObject.SetActive(true);
    }

    public void onDrop()
    {
        transform.parent = originalParent;
        rb.useGravity = true;
        //rb.isKinematic = false;
        isPickedUp = false;
        pickuped = null;
        pickupeffect.gameObject.SetActive(false);



    }

    public void onFreeze()
    {
        onDrop();
        foreach (MeshRenderer mr in renderers)
        {
            mr.material = GameCore.Instance.BlackWhiteMat;
        }
        IsFrozen = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        source.PlayOneShot(GameCore.Instance.freeze_ambient);
    }

    public void onUnfreeze()
    {
        foreach (MeshRenderer mr in renderers)
        { mr.material = originalMaterial[mr]; }
        IsFrozen = false;
        rb.constraints = RigidbodyConstraints.None;
    }

    public void onReset()
    {
        onUnfreeze();
        onDrop();
        transform.position = initpos;
        transform.rotation = initRot;
        
    }
}
