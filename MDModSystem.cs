using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MultiDraw {
    public class MDModSystem : ModSystem {

        public List<Image> images;

        public override void OnWorldLoad() {
            images = new List<Image>();
            base.OnWorldLoad();
        }

        /*public override void LoadWorldData(TagCompound tag) {
            int len;
            if (tag.ContainsKey("count")) {
                len = (int) tag.Get<int>("count");
                Console.WriteLine($"MultiDraw count is {len}");

                for (int i = 0; i < len; i++) {
                    float posx = tag.Get<float>("posx" + i.ToString());
                    float posy = tag.Get<float>("posy" + i.ToString());
                    Vector2 pos = new Vector2(posx, posy);
                    float scale = tag.Get<float>("scale" + i.ToString());
                    int image = tag.Get<int>("image" + i.ToString());
                    images.Add(new Image(pos, scale, image));
                }
            } else {
                Console.WriteLine("World does not contain MultiDraw data");
            }
            base.LoadWorldData(tag);
        }

        public override void SaveWorldData(TagCompound tag) {
            if (Main.netMode == NetmodeID.SinglePlayer) {
                tag.Add("count", images.Count);
                for (int i = 0; i < images.Count; i++) {
                    tag.Add("posx" + i.ToString(), images[i].pos.X);
                    tag.Add("posy" + i.ToString(), images[i].pos.Y);
                    tag.Add("scale" + i.ToString(), images[i].scale);
                    tag.Add("image" + i.ToString(), images[i].image);
                }
            } else {
                tag.Add("count", images.Count);
                for (int i = 0; i < images.Count; i++) {
                    tag.Add("posx" + i.ToString(), images[i].pos.X);
                    tag.Add("posy" + i.ToString(), images[i].pos.Y);
                    tag.Add("scale" + i.ToString(), images[i].scale);
                    tag.Add("image" + i.ToString(), images[i].image);
                }
            }
            base.SaveWorldData(tag);
        }*/

    }
}
