using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MultiDraw.Content.Items
{ 
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class Pen : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.MultiDraw.hjson' file.
		public override void SetDefaults()
		{
			Item.damage = 0;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 0;
			Item.value = Item.buyPrice(0);
			Item.rare = ItemRarityID.Gray;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.scale = 0.75f;
		}

        public override void UpdateInventory(Player player) {
			if (ModContent.GetInstance<ConfigServerSide>().PenUseAnimation) {
				Item.useStyle = ItemUseStyleID.Swing;
			} else {
				Item.useStyle = 0;
			}
            base.UpdateInventory(player);
        }
	}
}
