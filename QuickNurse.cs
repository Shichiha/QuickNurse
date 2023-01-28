using System.ComponentModel;
using System.Collections.Generic;
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
		internal static ModKeybind ToggleQuickNurse;
		public static QuickNurse Instance;
		public override void Load()
		{
			Instance = this;
			ToggleQuickNurse = KeybindLoader.RegisterKeybind(this, "Auto Heal", "Home");

		}

		public override void Unload()
		{
			Instance = null;
		}


	}

	public class QuickNursePlayer : ModPlayer
	{
		private bool IsNurseNearby()
		{
			Vector2 playerPos = Main.player[Main.myPlayer].position;
			int nurse = NPC.FindFirstNPC(NPCID.Nurse);
			NPC nurseNPC = Main.npc[nurse];
			double num9 = 100;
			return nurse > 0 && nurseNPC.Distance(playerPos) < num9;
		}

		private void HealPlayer(Player player)
		{
			player.HealEffect(player.statLifeMax2 - player.statLife, true);
			SoundEngine.PlaySound(SoundID.Item4, player.Center);
			player.statLife = player.statLifeMax2;
			for (int b = 0; b < Player.MaxBuffs; ++b)
			{
				int index = player.buffType[b];
				List<int> debuffIndexes = new List<int> { 28, 34, 87, 89, 21, 86, 199 };
				if (Main.debuff[index] && player.buffTime[b] > 0 && !debuffIndexes.Contains(index))
				{
					player.DelBuff(b);
					b = -1;
				}
			}
		}

		private bool PlayerNeedsHealing(Player player) => player.statLife < player.statLifeMax2;

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (QuickNurse.ToggleQuickNurse.JustPressed)
			{
				Player myPlayer = Main.player[Main.myPlayer];
				List<string> messages = new List<string> { "No nurse nearby!", "Not enough money!", "You are already at full health!" };
				bool needsHealing = PlayerNeedsHealing(myPlayer);
				bool enoughMoney = myPlayer.BuyItem(NPCID.Nurse, -1);
				bool nurseNearby = IsNurseNearby();
				if (nurseNearby && enoughMoney && needsHealing)
					HealPlayer(myPlayer);
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
		}
	}

	internal static class Hotkeys
	{
		[DefaultValue("LeftControl")]
		public static string ToggleQuickNurse { get; set; }
	}
}