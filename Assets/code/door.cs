using System.Collections;
using UnityEngine;

public class door : Selectable, IFreezable
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float movement;
    public Vector3 target;
    public bool IsFrozen { get; set; }

    public void onFreeze()
    {
        IsFrozen = true;


    }

    public void onUnfreeze()
    {
        IsFrozen = false;

    }
    private void Awake()
    {
    }
    private void OnDrawGizmosSelected()
    {
        if (movement == 0)
        {
            return;
        }
        // Draw arrow showing movement direction
        Vector3 direction = transform.right * movement;
        Vector3 start = transform.position;
        Vector3 end = start + direction;

        // Draw main line
        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);

        // Draw arrowhead
        float arrowHeadLength = 0.3f;
        float arrowHeadAngle = 20f;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        Gizmos.DrawRay(end, right * arrowHeadLength);
        Gizmos.DrawRay(end, left * arrowHeadLength);
    }

    public void onOpen()
    {

    }
    public void onClose()
    {

    }

    
}
