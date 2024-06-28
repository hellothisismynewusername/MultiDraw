﻿using System;
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

        public override void Load() {
            ReDisplay = KeybindLoader.RegisterKeybind(Mod, "Re-sync", "L");
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
