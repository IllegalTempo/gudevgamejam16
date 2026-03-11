using System.Xml.Linq;
using UnityEngine;

public class TransformWatcher : MonoBehaviour
{
    public Vector3 lastPos;
    public bool lastKinematic;

    void Start()
    {
        lastPos = transform.position;
        var rb = GetComponent<Rigidbody>();
        lastKinematic = rb != null && rb.isKinematic;
    }

    void Update()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        if (rb.isKinematic != lastKinematic)
        {
            Debug.Log($"[TransformWatcher] isKinematic changed to {rb.isKinematic} on {name}\n{System.Environment.StackTrace}");
            lastKinematic = rb.isKinematic;
        }

        if (transform.position != lastPos)
        {
            var delta = transform.position - lastPos;
            if (delta.sqrMagnitude > 0.0001f)
            {
                Debug.Log($"[TransformWatcher] Position changed on {name} from {lastPos} to {transform.position}\n{System.Environment.StackTrace}");
                lastPos = transform.position;
            }
        }
    }
}