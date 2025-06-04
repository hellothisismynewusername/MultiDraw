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
using System.Threading;

namespace MultiDraw
{
    public class MDPlayer : ModPlayer {

        public List<Image> images;
        public List<Image> buf;
        //public List<int> eraseBuf;
        public List<Vector2> erasePositionsBuf;
        bool oldMouseState;
        bool rightOldMouseState;
        float scale;
        Vector2 prevMouse;
        int smoothing;
        bool visible;

        public override void OnEnterWorld() {
            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server) {
                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.position, new Vector2(0f, 0f), ModContent.ProjectileType<Canvas>(), 0, 0f);
                images = new List<Image>();
                buf = new List<Image>();
                //eraseBuf = new List<int>();
                erasePositionsBuf = new();
                scale = 1f;
                prevMouse = Main.MouseWorld;
                smoothing = ModContent.GetInstance<ConfigServerSide>().Smoothing;
                visible = true;

                if (Main.netMode != NetmodeID.SinglePlayer) {
                    //request to sync images
                    ModPacket pack = ModContent.GetInstance<MultiDraw>().GetPacket();
                    pack.Write((byte) 2);
                    pack.Write((Int32) Player.whoAmI);
                    pack.Send();
                } else {
                    images = ModContent.GetInstance<MDModSystem>().images;
                }
            }
        }

        public override void PreUpdate() {
            re(false, true);
        }

        public override void PostUpdate() {
            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server) {

                smoothing = ModContent.GetInstance<ConfigServerSide>().Smoothing;

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

                    if (Main.netMode != NetmodeID.SinglePlayer && buf.Count * 16 > 65480) {
                        Main.mouseLeft = false;
                    }
                }
                if (Player.HeldItem.type == ModContent.ItemType<Pen>() && Main.mouseRight) {
                    for (int i = 0; i < images.Count; i++) {
                        if (images[i].pos.X >= Main.MouseWorld.X - scale * 5f && images[i].pos.X <= Main.MouseWorld.X + scale * 5f &&
                            images[i].pos.Y >= Main.MouseWorld.Y - scale * 5f && images[i].pos.Y <= Main.MouseWorld.Y + scale * 5f) {
                            images.RemoveAt(i);
                            if (Main.netMode != NetmodeID.SinglePlayer) {
                                //eraseBuf.Add(i);
                                erasePositionsBuf.Add(Main.MouseWorld);
                            }
                        }

                        if (Main.netMode != NetmodeID.SinglePlayer && erasePositionsBuf.Count * 4 > 65480) {
                            break;
                        }
                    }
                }

                if (Main.netMode != NetmodeID.SinglePlayer &&
                    Player.HeldItem.type == ModContent.ItemType<Pen>() &&
                    !Main.mouseLeft && oldMouseState) { //Just Released

                    ModPacket pack = ModContent.GetInstance<MultiDraw>().GetPacket();
                    pack.Write((byte) 0);                       //byte
                    pack.Write((Int32) Player.whoAmI);          //int (owner)
                    pack.Write((Int32) buf.Count);              //int (len)
                    bool earlyEnd = false;
                    for (int i = 0; i < buf.Count; i++) {
                        pack.WriteVector2(buf[i].pos);
                        pack.Write(buf[i].scale);
                        pack.Write((Int32) buf[i].image);

                        //overflow protection
                        if (pack.BaseStream.Length > 65500) {
                            if (ModContent.GetInstance<ConfigClientSide>().PacketSizeWarningMessage) {
                                Main.NewText("Cutting off early due to approaching maximum size limit. Please release the mouse button earlier, or decrease smoothing in server-side config.");
                            }
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

                            if (Main.netMode != NetmodeID.SinglePlayer) {
                                //request to sync images
                                ModPacket p = ModContent.GetInstance<MultiDraw>().GetPacket();
                                p.Write((byte) 2);
                                p.Write((Int32) Player.whoAmI);
                                p.Send();
                            }

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
                    pack.Write((byte)1);                       //byte
                    pack.Write((Int32)Player.whoAmI);          //int (owner)
                    pack.Write((float)scale);
                    pack.Write((Int32)erasePositionsBuf.Count);         //int (len)
                    bool earlyEnd = false;
                    for (int i = 0; i < erasePositionsBuf.Count; i++)
                    {
                        pack.WriteVector2(erasePositionsBuf[i]);

                        //overflow protection
                        if (pack.BaseStream.Length > 65500)
                        {
                            if (ModContent.GetInstance<ConfigClientSide>().PacketSizeWarningMessage)
                            {
                                Main.NewText("Cutting off early due to approaching maximum size limit. Please release the mouse button earlier.");
                            }
                            ModPacket earlyPack = ModContent.GetInstance<MultiDraw>().GetPacket();
                            earlyPack.Write((byte)1);
                            earlyPack.Write((Int32)Player.whoAmI);
                            earlyPack.Write((float)scale);
                            earlyPack.Write((Int32)i);
                            for (int j = 0; j < i; j++)
                            {
                                earlyPack.WriteVector2(erasePositionsBuf[j]);
                            }
                            earlyPack.Send();
                            erasePositionsBuf.Clear();
                            earlyEnd = true;

                            if (Main.netMode != NetmodeID.SinglePlayer)
                            {
                                //request to sync images
                                ModPacket p = ModContent.GetInstance<MultiDraw>().GetPacket();
                                p.Write((byte)2);
                                p.Write((Int32)Player.whoAmI);
                                p.Send();
                            }

                            break;
                        }
                    }
                    if (!earlyEnd) pack.Send();

                    erasePositionsBuf.Clear();

                    if (Main.netMode != NetmodeID.SinglePlayer) {
                        //request to sync erverybody
                        ModPacket p = ModContent.GetInstance<MultiDraw>().GetPacket();
                        p.Write((byte) 3);
                        p.Write((Int32) Player.whoAmI);
                        p.Send();
                    }

                }

                //Main.NewText($"{images.Count}  |  {buf.Count}");


                oldMouseState = Main.mouseLeft;
                rightOldMouseState = Main.mouseRight;
                prevMouse = Main.MouseWorld;
            }
        }

        void re(bool sync, bool disp) {
            if (sync) {
                if (Main.netMode != NetmodeID.SinglePlayer) {
                    ModPacket pack = ModContent.GetInstance<MultiDraw>().GetPacket();
                    pack.Write((byte) 2);
                    pack.Write((Int32) Player.whoAmI);
                    pack.Send();
                }
            }
            if (disp) {
                if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server) {
                    for (int i = 0; i < Main.maxProjectiles; i++) {
                        if (Main.projectile[i].active && Main.projectile[i].ModProjectile != null && Main.projectile[i].ModProjectile.Type == ModContent.ProjectileType<Canvas>() && Main.projectile[i].owner == Player.whoAmI) {
                            Main.projectile[i].Kill();
                        }
                    }
                    if (visible) {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.position, new Vector2(0f, 0f), ModContent.ProjectileType<Canvas>(), 0, 0f);
                    }
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet) {
            if (Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server) {
                if (KeybindSystem.ReDisplay.JustPressed) {
                    re(true, true);
                }
                if (KeybindSystem.IncreaseBrushSize.JustPressed && !Main.mouseRight) {
                    if (scale < 10f) {
                        scale += 0.4f;
                    }
                }
                if (KeybindSystem.DecreaseBrushSize.JustPressed && !Main.mouseRight) {
                    if (scale > 0.8f) {
                        scale -= 0.4f;
                    }
                }
                if (KeybindSystem.EraseNearby.JustPressed && Main.netMode == NetmodeID.SinglePlayer) {
                    for (int i = images.Count - 1; i >= 0; i--) {

                        if (images[i].pos.X >= Player.position.X - 950 &&
                            Player.GetModPlayer<MDPlayer>().images[i].pos.X <= Player.position.X + 950 &&
                            Player.GetModPlayer<MDPlayer>().images[i].pos.Y >= Player.position.Y - 530 &&
                            Player.GetModPlayer<MDPlayer>().images[i].pos.Y <= Player.position.Y + 530) {

                            images.RemoveAt(i);
                        }

                    }
                }
                if (KeybindSystem.ToggleVisibility.JustPressed) {
                    visible = !visible;
                }
            }
        }
    }
}
