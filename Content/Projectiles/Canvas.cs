using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.Metrics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using Terraria.DataStructures;
using System.Collections.Generic;
using MultiDraw.Content.Items;
using rail;

namespace MultiDraw.Content.Projectiles
{
    public class Canvas : ModProjectile
    {

        

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.width = 1; // The width of the projectile's hitbox.
			Projectile.height = 1; // The height of the projectile's hitbox.
            Projectile.ignoreWater = true;
			Projectile.noEnchantments = true;
            //Projectile.position = new Vector2(0f, 0f);
            //Main.NewText("i spawned");
            Projectile.active = true;
            Projectile.friendly = false;
            Projectile.hostile = false;
            //Projectile.alpha = 255;
        }

        public override void PostDraw(Color lightColor) {
            Projectile.position = Main.player[Projectile.owner].position;
            if (Main.myPlayer == Projectile.owner) {
/*
                Vector2 pos = Main.player[Projectile.owner].position;
                Texture2D itemTexture;
                Rectangle itemRect;
                Main.GetItemDrawFrame(ItemID.WoodenArrow, out itemTexture, out itemRect);
                Main.EntitySpriteDraw(itemTexture, pos - Main.screenPosition, itemRect, Color.White, 0f, new Vector2(itemTexture.Width / 2, itemTexture.Height / 2), 1f, SpriteEffects.None);
                */
                
                //Main.NewText($"{Main.player[Projectile.owner].GetModPlayer<MDPlayer>().images.Count} is what actual?");

                Projectile.timeLeft = 100000;

                for (int i = 0; i < Main.player[Projectile.owner].GetModPlayer<MDPlayer>().images.Count; i++) {
                    Texture2D texture;
                    Rectangle rect;
                    switch (Main.player[Projectile.owner].GetModPlayer<MDPlayer>().images[i].image) {
                        case 0:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/black");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -1:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/white");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -2:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/red");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -3:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/green");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -4:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/blue");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -5:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/yellow");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -6:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/orange");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -7:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/purple");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        case -8:
                            texture = (Texture2D) Mod.Assets.Request<Texture2D>("Assets/pink");
                            rect = new Rectangle(0, 0, 8, 8);
                            break;
                        default:
                            Main.GetItemDrawFrame(Main.player[Projectile.owner].GetModPlayer<MDPlayer>().images[i].image, out texture, out rect);
                            break;
                    }

                    Main.EntitySpriteDraw(texture, Main.player[Projectile.owner].GetModPlayer<MDPlayer>().images[i].pos - Main.screenPosition, rect, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), Main.player[Projectile.owner].GetModPlayer<MDPlayer>().images[i].scale, SpriteEffects.None);
                }

                
            }
        }

    }
}
