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
			Mod Calamity = ModLoader.GetMod("CalamityMod");
		}
	}

	public class QuickNursePlayer : ModPlayer
	{
		Mod Calamity = ModLoader.GetMod("CalamityMod");

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
			price = (int)((double)price * myPlayer.currentShoppingSettings.PriceAdjustment);
			if (Calamity != null)
			{
				if (price > 0)
				{

					if ((bool)Calamity.Call("Downed", "yharon"))
						price += 90000;
					else if ((bool)Calamity.Call("Downed", "devourer of gods"))
						price += 60000;
					else if ((bool)Calamity.Call("Downed", "providence"))
						price += 32000;
					else if (NPC.downedMoonlord)
						price += 20000;
					else if (NPC.downedFishron || (bool)Calamity.Call("Downed", "plaguebringer") || (bool)Calamity.Call("Downed", "ravager"))
						price += 12000;
					else if (NPC.downedGolemBoss)
						price += 900;
					else if (NPC.downedPlantBoss || (bool)Calamity.Call("Downed", "calamitas clone"))
						price += 6000;
					else if (NPC.downedMechBossAny)
						price += 4000;
					else if (Main.hardMode)
						price += 2400;
					else if (NPC.downedBoss3)
						price += 1200;
					else if (NPC.downedBoss1)
						price += 600;
					else
						price += 300;

					for (int i = 0; i < Main.maxNPCs; i++)
					{
						if (Main.npc[i].active && Main.npc[i].boss)
						{
							price *= 5;
							break;
						}
					}
					price += 5000; // measure
				}
				}
			return price;
			
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
				{
					int price = GetPrice();
					int platinum = price / 1000000;
					int gold = (price % 1000000) / 10000;
					int silver = (price % 10000) / 100;
					int copper = price % 100;
					Main.NewText("Price: " + price, 255, 255, 255);
				}

		}
	}

	internal static class Hotkeys
	{
		public static string QuickNurseHotkey { get; set; }
		public static string GetPriceHotkey { get; set; }
	}
}