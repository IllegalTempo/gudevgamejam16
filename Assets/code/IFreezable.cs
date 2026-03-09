using UnityEngine;

public interface IFreezable
{
    public bool IsFrozen { get; set; }
    public void onFreeze();
    public void onUnfreeze();

}
