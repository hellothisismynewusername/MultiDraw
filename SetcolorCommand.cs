using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MultiDraw
{
	public class SetcolorCommand : ModCommand
	{
		// CommandType.Chat means that command can be used in Chat in SP and MP
		public override CommandType Type
			=> CommandType.Chat;

		// The desired text to trigger this command
		public override string Command
			=> "setcolor";

		// A short description of this command
		public override string Description
			=> "Changes brush color to a color or an item sprite. Input either 'black', 'white', 'red', 'green', 'blue', or a valid item ID";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (args.Length != 1) {
				Main.NewText("Incorrect amount of arguments supplied to 'setcolor'. Input only 'black', 'white', 'red', 'green', 'blue', or a valid item ID");
				return;
			}
			switch (args[0]) {
				case "black":
					ModContent.GetInstance<ConfigClientSide>().BrushImage = 0;
					break;

				case "white":
					ModContent.GetInstance<ConfigClientSide>().BrushImage = -1;
					break;

				case "red":
					ModContent.GetInstance<ConfigClientSide>().BrushImage = -2;
					break;

				case "green":
					ModContent.GetInstance<ConfigClientSide>().BrushImage = -3;
					break;

				case "blue":
					ModContent.GetInstance<ConfigClientSide>().BrushImage = -4;
					break;

				default:
					int num = -100;
					if (Int32.TryParse(args[0], out num)) {
						ModContent.GetInstance<ConfigClientSide>().BrushImage = num;
						return;
					} else {
						Main.NewText("Argument is unparsable. Input either 'black', 'white', 'red', 'green', 'blue', or a valid item ID");
						return;
					}
			}
			
		}
	}
}