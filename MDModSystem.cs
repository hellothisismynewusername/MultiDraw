using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace MultiDraw {
    public class MDModSystem : ModSystem {

        public List<Image> images;

        public override void OnWorldLoad() {
            images = new List<Image>();
            base.OnWorldLoad();
        }

    }
}
