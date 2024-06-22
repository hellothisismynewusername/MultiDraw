using Microsoft.Xna.Framework;
using MultiDraw.Content.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MultiDraw.Content.Projectiles;
using MultiDraw.Content.Items;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Steamworks;
using MultiDraw.Content.Items;
using Terraria.GameInput;
using Terraria.ModLoader.IO;

namespace MultiDraw
{
    public class MDPlayer : ModPlayer {

        int myCanvas;
        public List<Image> images;
        public List<Image> buf;
        public List<int> eraseBuf;
        bool oldMouseState;
        bool rightOldMouseState;
        public bool reset;
        float scale;
        Vector2 prevMouse;
        int smoothing;

        public override void OnEnterWorld() {
            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server) {
                myCanvas = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.position, new Vector2(0f, 0f), ModContent.ProjectileType<Canvas>(), 1, 1f);
                images = new List<Image>();
                buf = new List<Image>();
                eraseBuf = new List<int>();
                scale = 1f;
                prevMouse = Main.MouseWorld;
                smoothing = ModContent.GetInstance<ConfigServerSide>().Smoothing;

                if (Main.netMode != NetmodeID.SinglePlayer) {
                    //request to sync images
                    ModPacket pack = ModContent.GetInstance<MultiDraw>().GetPacket();
                    pack.Write((byte) 2);
                    pack.Write((Int32) Player.whoAmI);
                    pack.Send();
                }
            }
        }

        public override void OnRespawn() {
            reset = true;
        }

        public override void PostUpdate() {
/*
            if (Main.netMode == NetmodeID.Server) {
                Console.WriteLine($"{ModContent.GetInstance<MDModSystem>().images.Count} is server's images");
            }
*/
            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server) {

                smoothing = ModContent.GetInstance<ConfigServerSide>().Smoothing;

                if (!Main.projectile[myCanvas].active) {
                    myCanvas = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.position, new Vector2(0f, 0f), ModContent.ProjectileType<Canvas>(), 1, 1f);
                }

                if (Player.HeldItem.type == ModContent.ItemType<Pen>() && Main.mouseLeft) {
                    //images.Add(new Image(new Vector2((prevMouse.X + Main.MouseWorld.X) / 2f, (prevMouse.Y + Main.MouseWorld.Y) / 2f), scale, -2));

                    float dist = Vector2.Distance(prevMouse, Main.MouseWorld);
                    float step = dist / (float) smoothing;
                    float angle = (float) Math.Atan2(Main.MouseWorld.Y - prevMouse.Y, Main.MouseWorld.X - prevMouse.X);
                    for (int i = 1; i < smoothing; i++) {
                        float stepX = (float) Math.Cos(angle) * step * (float) i;
                        float stepY = (float) Math.Sin(angle) * step * (float) i;
                        Vector2 p = new Vector2(prevMouse.X + stepX, prevMouse.Y + stepY);
                        images.Add(new Image(p, scale, ModContent.GetInstance<ConfigClientSide>().BrushImage));
                        if (Main.netMode != NetmodeID.SinglePlayer) {
                            buf.Add(new Image(p, scale, ModContent.GetInstance<ConfigClientSide>().BrushImage));
                        }
                    }

                    images.Add(new Image(Main.MouseWorld, scale, ModContent.GetInstance<ConfigClientSide>().BrushImage));
                    if (Main.netMode != NetmodeID.SinglePlayer) {
                        buf.Add(new Image(Main.MouseWorld, scale, ModContent.GetInstance<ConfigClientSide>().BrushImage));
                    }
                }
                if (Player.HeldItem.type == ModContent.ItemType<Pen>() && Main.mouseRight) {
                    for (int i = 0; i < images.Count; i++) {
                        if (images[i].pos.X >= Main.MouseWorld.X - scale * 5f && images[i].pos.X <= Main.MouseWorld.X + scale * 5f &&
                            images[i].pos.Y >= Main.MouseWorld.Y - scale * 5f && images[i].pos.Y <= Main.MouseWorld.Y + scale * 5f) {
                            images.RemoveAt(i);
                            if (Main.netMode != NetmodeID.SinglePlayer) {
                                eraseBuf.Add(i);
                            }
                        }
                    }
                }

                if (Main.netMode != NetmodeID.SinglePlayer &&
                    Player.HeldItem.type == ModContent.ItemType<Pen>() &&
                    !Main.mouseLeft && oldMouseState) { //Just Released

                    ModPacket pack = ModContent.GetInstance<MultiDraw>().GetPacket();
                    pack.Write((byte) 0);                       //byte
                    pack.Write((Int32) Player.whoAmI);       //int (owner)
                    pack.Write((Int32) buf.Count);              //int (len)
                    bool earlyEnd = false;
                    for (int i = 0; i < buf.Count; i++) {
                        pack.WriteVector2(buf[i].pos);
                        pack.Write(buf[i].scale);
                        pack.Write((Int32) buf[i].image);

                        if (pack.BaseStream.Length > 65500) {
                            Main.NewText("Cutting off early due to approaching maximum size limit. Please let go of the mouse button earlier");
                            ModPacket earlyPack = ModContent.GetInstance<MultiDraw>().GetPacket();
                            earlyPack.Write((byte) 0);
                            earlyPack.Write((Int32) Player.whoAmI);
                            earlyPack.Write((Int32) i);
                            for (int j = 0; j < i; j++) {
                                earlyPack.WriteVector2(buf[j].pos);
                                earlyPack.Write(buf[j].scale);
                                earlyPack.Write((Int32) buf[j].image);
                            }
                            earlyPack.Send();

                            for (int j = i; j < buf.Count; j++) {
                                for (int k = 0; k < images.Count; k++) {
                                    if (images[k].pos.Equals(buf[j].pos)) {
                                        images.RemoveAt(k);
                                    }
                                }
                            }
                            earlyEnd = true;
                            break;
                        }
                    }
                    if (!earlyEnd) pack.Send();

                    buf.Clear();
                }
                if (Main.netMode != NetmodeID.SinglePlayer &&
                    Player.HeldItem.type == ModContent.ItemType<Pen>() &&
                    !Main.mouseRight && rightOldMouseState) { //Just Released ERASER

                    ModPacket pack = ModContent.GetInstance<MultiDraw>().GetPacket();
                    pack.Write((byte) 1);                       //byte
                    pack.Write((Int32) Player.whoAmI);       //int (owner)
                    pack.Write((Int32) eraseBuf.Count);              //int (len)
                    for (int i = 0; i < eraseBuf.Count; i++) {
                        pack.Write((Int32) eraseBuf[i]);
                    }
                    pack.Send();

                    eraseBuf.Clear();
                }

                //Main.NewText($"{images.Count}  |  {buf.Count}");

                oldMouseState = Main.mouseLeft;
                rightOldMouseState = Main.mouseRight;
                prevMouse = Main.MouseWorld;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server) {
                if (MDSystem.ReDisplay.JustPressed) {
                    myCanvas = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.position, new Vector2(0f, 0f), ModContent.ProjectileType<Canvas>(), 1, 1f);
                    reset = true;
                }
                if (MDSystem.IncreaseBrushSize.JustPressed) {
                    if (scale < 10f) {
                        scale += 0.4f;
                    }
                }
                if (MDSystem.DecreaseBrushSize.JustPressed) {
                    if (scale > 0.8f) {
                        scale -= 0.4f;
                    }
                }
            }
        }
    }
}
