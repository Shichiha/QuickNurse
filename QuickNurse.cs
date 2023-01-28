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
		public static double num9 = 100;


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
			int nurse = NPC.FindFirstNPC(NPCID.Nurse);
			NPC nurseNPC = Main.npc[nurse];
			return nurse > 0 && nurseNPC.Distance(playerPos) < QuickNurse.num9;
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
			List<string> messages = new List<string> { "No nurse nearby!", "Not enough money!", "You are already at full health!" };
			bool needsHealing = PlayerNeedsHealing(player);
			bool enoughMoney = player.BuyItem(NPCID.Nurse, -1);
			bool nurseNearby = IsNurseNearby();
			if (nurseNearby && enoughMoney && needsHealing)
				HealPlayer(player);
			else
			{
				if (!nurseNearby)
					Main.NewText(messages[0], 255, 0, 0);
				else if (!enoughMoney)
					Main.NewText(messages[1], 255, 0, 0);
				else if (!needsHealing)
					Main.NewText(messages[2], 255, 255, 255);
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