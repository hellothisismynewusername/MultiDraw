using Microsoft.Xna.Framework;
using MultiDraw.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MultiDraw {
    public class CameraModeCommand : ModCommand {
        // CommandType.Chat means that command can be used in Chat in SP and MP

        public override CommandType Type
            => CommandType.Chat;

        // The desired text to trigger this command
        public override string Command
            => "hideUI";

        // A short description of this command
        public override string Description
            => "Hide all the UI besides health and mana, press Esc to exit this mode";

        public override void Action(CommandCaller caller, string input, string[] args) {
            if (Main.myPlayer == caller.Player.whoAmI && Main.netMode != NetmodeID.Server) {
                Empty foo = new Empty();
                Terraria.UI.IngameFancyUI.OpenUIState(foo);
            }
        }
    }
}
