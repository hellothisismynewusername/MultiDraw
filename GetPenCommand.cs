using Microsoft.Xna.Framework;
using MultiDraw.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MultiDraw {
    public class GetPenCommand : ModCommand {
        // CommandType.Chat means that command can be used in Chat in SP and MP

        public override CommandType Type
            => CommandType.Chat;

        // The desired text to trigger this command
        public override string Command
            => "pen";

        // A short description of this command
        public override string Description
            => "Use to acquire pen";

        public override void Action(CommandCaller caller, string input, string[] args) {
            if (Main.myPlayer == caller.Player.whoAmI && Main.netMode != NetmodeID.Server) {
                caller.Player.QuickSpawnItem(caller.Player.GetSource_FromThis(), ModContent.ItemType<Pen>());
            }
        }
    }
}
