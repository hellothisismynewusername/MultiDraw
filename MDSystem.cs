using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace MultiDraw {
    public class MDSystem : ModSystem {

        public static ModKeybind ReDisplay { get; private set; }
        public static ModKeybind IncreaseBrushSize { get; private set; }
        public static ModKeybind DecreaseBrushSize { get; private set; }

        public override void Load() {
            ReDisplay = KeybindLoader.RegisterKeybind(Mod, "Redisplay (if stuff disappears suddenly)", "L");
            IncreaseBrushSize = KeybindLoader.RegisterKeybind(Mod, "Increase brush size", "Up");
            DecreaseBrushSize = KeybindLoader.RegisterKeybind(Mod, "Decrease brush size", "Down");
        }

        public override void Unload() {
            ReDisplay = null;
            IncreaseBrushSize = null;
            DecreaseBrushSize = null;
        }

    }
}
