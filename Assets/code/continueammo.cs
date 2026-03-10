using UnityEngine;
using System.Collections;

namespace Assets.code
{
	public class continueammo: ammo
	{
		public override void onPickUP(PlayerMovement player)
		{
			base.onPickUP(player);
			player.ContinueAmmo += amount;
			GameCore.Instance.continueAmmoText.text = player.ContinueAmmo.ToString();
        }
    }
}