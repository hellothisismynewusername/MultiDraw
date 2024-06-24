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

        public override void ClearWorld() {
            images = new List<Image>();
        }

        public override void SaveWorldData(TagCompound tag) {
            List<Vector2> positions = new List<Vector2>();
            List<float> scales = new List<float>();
            List<int> imageIdens = new List<int>();
            if (Main.netMode == NetmodeID.SinglePlayer) {
                for (int i = 0; i < Main.player[0].GetModPlayer<MDPlayer>().images.Count; i++) {    //the player in singeplayer is always 0, right?
                    positions.Add(Main.player[0].GetModPlayer<MDPlayer>().images[i].pos);
                    scales.Add(Main.player[0].GetModPlayer<MDPlayer>().images[i].scale);
                    imageIdens.Add(Main.player[0].GetModPlayer<MDPlayer>().images[i].image);
                }
            } else {
                for (int i = 0; i < images.Count; i++) {    //the player in singeplayer is always 0, right?
                    positions.Add(images[i].pos);
                    scales.Add(images[i].scale);
                    imageIdens.Add(images[i].image);
                }
            }
            tag.Add("positions", positions);
            tag.Add("scales", scales);
            tag.Add("imageIdens", imageIdens);
        }

        public override void LoadWorldData(TagCompound tag) {
            List<Vector2> positions = new List<Vector2>();
            if (tag.ContainsKey("positions")) {
                positions = (List<Vector2>) tag.GetList<Vector2>("positions");
            } else {
                Console.WriteLine("MultiDraw: save does not contain key for positions");
            }
            List<float> scales = new List<float>();
            if (tag.ContainsKey("scales")) {
                scales = (List<float>) tag.GetList<float>("scales");
            } else {
                Console.WriteLine("MultiDraw: save does not contain key for scales");
            }
            List<int> imageIdens = new List<int>();
            if (tag.ContainsKey("imageIdens")) {
                imageIdens = (List<int>) tag.GetList<int>("imageIdens");
            } else {
                Console.WriteLine("MultiDraw: save does not contain key for imageIdens");
            }

            if (positions.Count != scales.Count || scales.Count != imageIdens.Count || positions.Count != imageIdens.Count) {
                Console.WriteLine("MultiDraw: Error in loading, list lengths do not align");
                Console.WriteLine($"MultiDraw: positions length {positions.Count}\nscales length {scales.Count}\nimageIdens length {imageIdens.Count}");
            }

            for (int i = 0; i < positions.Count; i++) {
                images.Add(new Image(positions[i], scales[i], imageIdens[i]));
            }
        }

    }
}
