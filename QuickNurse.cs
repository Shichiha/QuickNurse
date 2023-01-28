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
		internal static ModKeybind QuickNurseHotkey;
		internal static ModKeybind GetPriceHotkey;
		public override void Load()
		{
			QuickNurseHotkey = KeybindLoader.RegisterKeybind(this, "Nurse Heal", "Home");
			GetPriceHotkey = KeybindLoader.RegisterKeybind(this, "Get Price", "End");
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
				int buffIndex = player.buffType[b];
				if (Main.debuff[buffIndex] && player.buffTime[b] > 0 && !BuffID.Sets.NurseCannotRemoveDebuff[buffIndex])
				{
					player.DelBuff(b);
					b = -1;
				}
			}
		}

		private int GetPrice()
		{
			Player myPlayer = Main.player[Main.myPlayer];
			int price = myPlayer.statLifeMax2 - myPlayer.statLife;
			for (int i = 0; i < Player.MaxBuffs; ++i)
			{
				int buffIndex = myPlayer.buffType[i];
				if (Main.debuff[buffIndex] && myPlayer.buffTime[i] > 60 && !BuffID.Sets.NurseCannotRemoveDebuff[buffIndex])
					price += 100;
			}
			if (NPC.downedGolemBoss) price *= 200;
			else if (NPC.downedPlantBoss) price *= 150;
			else if (NPC.downedMechBossAny) price *= 100;
			else if (Main.hardMode) price *= 60;
			else if (NPC.downedBoss3 || NPC.downedQueenBee) price *= 25;
			else if (NPC.downedBoss2) price *= 10;
			else if (NPC.downedBoss1) price *= 3;
			if (Main.expertMode) price *= 2;
			int priceScaled = (int)((double)price * myPlayer.currentShoppingSettings.PriceAdjustment);
			return priceScaled;
		}

		private void NurseHeal(Player player)
		{
			bool needsHealing = PlayerNeedsHealing(player);
			bool enoughMoney = player.BuyItem(GetPrice());
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
			if (QuickNurse.QuickNurseHotkey.JustPressed)
				NurseHeal(Main.player[Main.myPlayer]);
			if (QuickNurse.GetPriceHotkey.JustPressed)
			{
				Main.NewText("Price: " + GetPrice(), 255, 255, 255);
			}
		}
	}

	internal static class Hotkeys
	{
		public static string QuickNurseHotkey { get; set; }
		public static string GetPriceHotkey { get; set; }
	}
}