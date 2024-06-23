using System;
using System.ComponentModel;
using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MultiDraw {
    [BackgroundColor(164, 153, 190)]
    public class ConfigServerSide : ModConfig {
        // ConfigScope.ClientSide should be used for client side, usually visual or audio tweaks.
        // ConfigScope.ServerSide should be used for basically everything else, including disabling items or changing NPC behaviours
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // [Label("$Some.Key")] // A label is the text displayed next to the option. This should usually be a short description of what it does. By default all ModConfig fields and properties have an automatic label translation key, but modders can specify a specific translation key.
        // [Tooltip("$Some.Key")] // A tooltip is a description showed when you hover your mouse over the option. It can be used as a more in-depth explanation of the option. Like with Label, a specific key can be provided.

        [DefaultValue(10)]
        [Range(0, 20)]
        public int Smoothing;

        [DefaultValue(true)]
        public bool PenUseAnimation;
    }
}