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
            rb.MovePosition(pickuped.cam.transform.position + pickuped.cam.transform.forward * 6);
        }
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
    }

    public void onDrop()
    {
        transform.parent = originalParent;
        rb.useGravity = true;
        //rb.isKinematic = false;
        isPickedUp = false;
        pickuped = null;


    }

    public void onFreeze()
    {
        onDrop();
        foreach (MeshRenderer mr in renderers)
        {
            mr.material = GameCore.Instance.BlackWhiteMat;
        }
        IsFrozen = true;
        rb.isKinematic = true;
    }

    public void onUnfreeze()
    {
        foreach (MeshRenderer mr in renderers)
        { mr.material = originalMaterial[mr]; }
        IsFrozen = false;
        rb.isKinematic = false;
    }

    public void onReset()
    {
        onUnfreeze();
        transform.position = initpos;
        transform.rotation = initRot;
    }
}
