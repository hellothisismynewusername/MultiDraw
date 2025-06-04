using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace MultiDraw {
    public class KeybindSystem : ModSystem {

        public static ModKeybind ReDisplay { get; private set; }
        public static ModKeybind IncreaseBrushSize { get; private set; }
        public static ModKeybind DecreaseBrushSize { get; private set; }
        public static ModKeybind EraseNearby { get; private set; }
        public static ModKeybind ToggleVisibility { get; private set; }

        public override void Load() {
            ReDisplay = KeybindLoader.RegisterKeybind(Mod, "Re-sync", "L");
            IncreaseBrushSize = KeybindLoader.RegisterKeybind(Mod, "Increase brush size", "Up");
            DecreaseBrushSize = KeybindLoader.RegisterKeybind(Mod, "Decrease brush size", "Down");
            EraseNearby = KeybindLoader.RegisterKeybind(Mod, "Erase nearby drawings", "Right");
            ToggleVisibility = KeybindLoader.RegisterKeybind(Mod, "Show/hide drawings", "Left");
        }

        public override void Unload() {
            ReDisplay = null;
            IncreaseBrushSize = null;
            DecreaseBrushSize = null;
            EraseNearby = null;
            ToggleVisibility = null;
        }

    }
}
