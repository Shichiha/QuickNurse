using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
namespace QuickNurse
{
	public class QuickNurse : Mod
	{
		// i love magic numbers
		public static List<int> dontClearIndexes = new List<int> { 28, 34, 87, 89, 21, 86, 199 };

		internal static ModKeybind ToggleQuickNurse;
		public override void Load()
		{
			ToggleQuickNurse = KeybindLoader.RegisterKeybind(this, "Nurse Heal", "Home");
		}
	}

	public class QuickNursePlayer : ModPlayer
	{
		private bool IsNurseNearby()
		{
			Vector2 playerPos = Main.player[Main.myPlayer].position;
			foreach (NPC npc in Main.npc)
				if (npc.type == NPCID.Nurse)
					if (new Rectangle((int)((double)playerPos.X + (double)(Main.player[Main.myPlayer].width / 2) - (double)(Player.tileRangeX * 16)), (int)((double)playerPos.Y + (double)(Main.player[Main.myPlayer].height / 2) - (double)(Player.tileRangeY * 16)), Player.tileRangeX * 16 * 2, Player.tileRangeY * 16 * 2).Intersects(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height)))
						return true;
			return false;
		}

		private void HealPlayer(Player player)
		{
			player.HealEffect(player.statLifeMax2 - player.statLife, true);
			SoundEngine.PlaySound(SoundID.Item4, player.Center);
			player.statLife = player.statLifeMax2;
			for (int b = 0; b < Player.MaxBuffs; ++b)
			{
				int index = player.buffType[b];
				if (Main.debuff[index] && player.buffTime[b] > 0 && !QuickNurse.dontClearIndexes.Contains(index))
				{
					player.DelBuff(b);
					b = -1;
				}
			}
		}

		private void NurseHeal(Player player)
		{
			bool needsHealing = PlayerNeedsHealing(player);
			bool enoughMoney = player.BuyItem(NPCID.Nurse, -1);
			bool nurseNearby = IsNurseNearby();
			if (nurseNearby && enoughMoney && needsHealing)
				HealPlayer(player);
			else
			{
				if (!nurseNearby)
					Main.NewText("No nurse nearby!", 255, 0, 0);
				else if (!enoughMoney)
					Main.NewText("Not enough money!", 255, 0, 0);
				else if (!needsHealing)
					Main.NewText("You are already at full health!", 255, 255, 255);
			}
		}
		private bool PlayerNeedsHealing(Player player) => player.statLife < player.statLifeMax2;

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (QuickNurse.ToggleQuickNurse.JustPressed)
				NurseHeal(Main.player[Main.myPlayer]);
		}
	}

	internal static class Hotkeys
	{
		public static string ToggleQuickNurse { get; set; }
	}
}