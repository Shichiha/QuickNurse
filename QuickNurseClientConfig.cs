using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace QuickNurse
{
	[Label("Config")]
	class QuickNurseClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static QuickNurseClientConfig Instance;
		[Label("Show Info Messages")]
		[Tooltip("Whether or not to show info messages (i.e. \"Nurse is not nearby\")")]
		[DefaultValue(true)]
		public bool ShowInfoMessages;

	}

}