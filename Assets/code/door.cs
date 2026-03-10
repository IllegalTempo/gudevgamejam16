using System;
using System.Collections;
using UnityEngine;

public class door : Selectable, IFreezable
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    public Vector3 target;

    private Vector3 movingtarget;
    private Vector3 orgPos;
    public bool IsFrozen { get; set; }

    public MeshRenderer meshRenderer;
    private Material material;
    public void onFreeze()
    {
        IsFrozen = true;
        meshRenderer.material = GameCore.Instance.BlackWhiteMat;


    }

    public void onUnfreeze()
    {
        IsFrozen = false;
        meshRenderer.material = material;

    }
    private void Awake()
    {
        orgPos = transform.position;
        movingtarget = transform.position;
        material = meshRenderer.material;
    }
    private void OnDrawGizmosSelected()
    {
        if (target == Vector3.zero) return;
        // Draw arrow showing movement direction
        Vector3 start = transform.position;
        Vector3 end = start + target;

        // Draw main line
        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);

        // Draw arrowhead
        float arrowHeadLength = 0.3f;
        float arrowHeadAngle = 20f;
        Vector3 right = Quaternion.LookRotation(target) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(target) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        Gizmos.DrawRay(end, right * arrowHeadLength);
        Gizmos.DrawRay(end, left * arrowHeadLength);
    }

    public void onOpen()
    {
        movingtarget = orgPos + target;
    }
    public void onClose()
    {

        movingtarget = orgPos - target;

    }
    protected override void Update()
    {
        base.Update();
        if(IsFrozen)
        {

        } else
        {
            rb.MovePosition(Vector3.Lerp(transform.position, movingtarget, 0.02f));

        }

    }


}
