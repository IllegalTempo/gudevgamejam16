using UnityEngine;

public class ammo : MonoBehaviour, IResetable
{
    public int amount;
    public virtual void onPickUP(PlayerMovement player)
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<PlayerMovement>() != null)
        {
            onPickUP(other.gameObject.GetComponent<PlayerMovement>());
        }
    }
    public void onReset()
    {
        gameObject.SetActive(true);
    }
}
