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
		private NPC getNurse()
		{
			List<NPC> nurses = new List<NPC>();
			Player player = Main.player[Main.myPlayer];
			foreach (NPC npc in Main.npc)
				if (npc.type == NPCID.Nurse && npc.active)
					nurses.Add(npc);
			if (nurses.Count == 0) return null;
			nurses.Sort((a, b) => Vector2.Distance(a.Center, player.Center).CompareTo(Vector2.Distance(b.Center, player.Center)));
			return nurses[0];
			
		}
		private bool IsNurseNearby()
		{
			Player myPlayer = Main.player[Main.myPlayer];
			Vector2 playerPos = myPlayer.position;
			NPC nurse = getNurse();
			if (nurse == null || myPlayer.dead) return false;
			return new Rectangle((int)((double)playerPos.X + (double)(Main.player[Main.myPlayer].width / 2) - (double)(Player.tileRangeX * 16)), (int)((double)playerPos.Y + (double)(Main.player[Main.myPlayer].height / 2) - (double)(Player.tileRangeY * 16)), Player.tileRangeX * 16 * 2, Player.tileRangeY * 16 * 2).Intersects(new Rectangle((int)nurse.position.X, (int)nurse.position.Y, nurse.width, nurse.height));
		}

		private void HealPlayer(Player player)
		{
			player.HealEffect(player.statLifeMax2 - player.statLife, true);
			SoundEngine.PlaySound(SoundID.Item4, player.Center);
			player.statLife = player.statLifeMax2;
			for (int buffSlot = 0; buffSlot < Player.MaxBuffs; ++buffSlot)
			{
				int buffId = player.buffType[buffSlot];
				if (Main.debuff[buffId] && player.buffTime[buffSlot] > 0 && !BuffID.Sets.NurseCannotRemoveDebuff[buffId])
				{
					player.DelBuff(buffSlot);
					buffSlot = -1;
				}
			}
		}

		private int GetPrice()
		{
			Player myPlayer = Main.player[Main.myPlayer];
			int price = myPlayer.statLifeMax2 - myPlayer.statLife;
			for (int buffSlot = 0; buffSlot < Player.MaxBuffs; ++buffSlot)
			{
				int buffId = myPlayer.buffType[buffSlot];
				if (Main.debuff[buffId] && myPlayer.buffTime[buffSlot] > 60 && !BuffID.Sets.NurseCannotRemoveDebuff[buffId])
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
			int playerHPtoMax = player.statLifeMax2 - player.statLife;
			if (nurseNearby && enoughMoney && needsHealing)
			{
				HealPlayer(player);
				PostNurseHeal(getNurse(), playerHPtoMax, true, GetPrice());
			}
			else
			{
				if (!nurseNearby && QuickNurseClientConfig.Instance.ShowInfoMessages)
					Main.NewText("No nurse nearby!", 255, 0, 0);
				else if (!enoughMoney && QuickNurseClientConfig.Instance.ShowInfoMessages)
					Main.NewText("Not enough money!", 255, 0, 0);
				else if (!needsHealing && QuickNurseClientConfig.Instance.ShowInfoMessages)
					Main.NewText("You are already at full health!", 255, 255, 255);
			}
		}
		private bool PlayerNeedsHealing(Player player) => player.statLife < player.statLifeMax2;

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (QuickNurse.QuickNurseHotkey.JustPressed)
				NurseHeal(Main.player[Main.myPlayer]);
			if (QuickNurse.GetPriceHotkey.JustPressed)
				Main.NewText("Price: " + GetPrice(), 255, 255, 255);

		}
	}

	internal static class Hotkeys
	{
		public static string QuickNurseHotkey { get; set; }
		public static string GetPriceHotkey { get; set; }
	}
}