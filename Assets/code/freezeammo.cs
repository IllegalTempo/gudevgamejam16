using UnityEngine;
using System.Collections;

namespace Assets.code
{
	public class freezeammo: ammo
	{
		public override void onPickUP(PlayerMovement player)
		{
			base.onPickUP(player);
			player.FreezeAmmo += amount;
			GameCore.Instance.freezeAmmoText.text = player.FreezeAmmo.ToString();
        }
    }
}