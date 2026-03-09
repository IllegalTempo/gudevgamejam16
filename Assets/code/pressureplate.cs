using UnityEngine;
using UnityEngine.Events;

public class pressureplate : MonoBehaviour
{
    public UnityEvent onPressed;
    public UnityEvent onReleased;
    private bool pressed = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (pressed) return;
        pressed = true;
        onPressed.Invoke();

    }
    private void OnCollisionExit(Collision collision)
    {
        if (!pressed) return;
        onReleased.Invoke();
        pressed = false;

    }
}
