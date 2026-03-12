using UnityEngine;
using UnityEngine.Events;

public class pressureplate : MonoBehaviour
{
    public UnityEvent onPressed;
    public UnityEvent onReleased;
    private bool pressed = false;
    public AudioSource audioSource;
    private void OnCollisionEnter(Collision collision)
    {
        if (pressed) return;
        pressed = true;
        onPressed.Invoke();
        audioSource.PlayOneShot(GameCore.Instance.clicksound);

    }
    private void OnCollisionExit(Collision collision)
    {
        if (!pressed) return;
        onReleased.Invoke();
        pressed = false;

    }
}
