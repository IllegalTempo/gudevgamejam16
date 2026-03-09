using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Item : Selectable, IFreezable
{
    private Transform originalParent;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private int Type;
    public bool isPickedUp = false;

    public bool IsFrozen { get; set; } = false;
    private Dictionary<MeshRenderer,Material> originalMaterial = new Dictionary<MeshRenderer, Material>();

    [SerializeField]
    private MeshRenderer[] renderers;
    private void Start()
    {
        originalParent = transform.parent;
        foreach (MeshRenderer mr in renderers)
        {
                originalMaterial[mr] = mr.material;
        }

    }
    public void onPickUp(PlayerMovement who)
    {
        transform.parent = who.cam.transform;
        transform.localPosition = new Vector3(0, 0, 3);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        isPickedUp = true;
    }

    public void onDrop()
    {
        transform.parent = originalParent;
        rb.isKinematic = false;
        isPickedUp = false;


    }

    public void onFreeze()
    {
        onDrop();
        foreach(MeshRenderer mr in renderers)
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
}
