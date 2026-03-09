using UnityEngine;

public class door : Selectable, IFreezable
{
    public bool IsFrozen { get; set; }

    public void onFreeze()
    {
        IsFrozen = true;

    }

    public void onUnfreeze()
    {
        IsFrozen = false;

    }
}
