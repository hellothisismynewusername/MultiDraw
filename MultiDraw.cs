using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using rail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using MultiDraw.Content.Projectiles;

namespace MultiDraw
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class MultiDraw : Mod
	{

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			if (Main.netMode == NetmodeID.Server) {
				switch (reader.ReadByte()) {
					case (byte) 0:
						ModPacket pack = ModContent.GetInstance<MultiDraw>().GetPacket();

						pack.Write((byte) 0);				//byte

						int owner = reader.ReadInt32();		//int (owner)
						pack.Write((Int32) owner);

						int len = reader.ReadInt32();		//int (len)
						pack.Write((Int32) len);

						List<Image> buf = new List<Image>();
						for (int i = 0; i < len; i++) {
							Vector2 pos = reader.ReadVector2();
							float scale = reader.ReadSingle();
							int image = reader.ReadInt32();
							buf.Add(new Image(pos, scale, image));
						}
						for (int i = 0; i < buf.Count; i++) {
							ModContent.GetInstance<MDModSystem>().images.Add(buf[i]);

							pack.WriteVector2(buf[i].pos);
							pack.Write(buf[i].scale);
							pack.Write((Int32) buf[i].image);
						}

						pack.Send();
						break;

					case (byte) 1:
						ModPacket erasePack = ModContent.GetInstance<MultiDraw>().GetPacket();

						erasePack.Write((byte) 1);

						erasePack.Write(reader.ReadInt32());	//owner

						int eraseLen = reader.ReadInt32();
						erasePack.Write((Int32) eraseLen);

						for (int i = 0; i < eraseLen; i++) {
							int eraseInd = reader.ReadInt32();
							ModContent.GetInstance<MDModSystem>().images.RemoveAt(eraseInd);
							erasePack.Write((Int32) eraseInd);
						}

						erasePack.Send();
						break;

					case (byte) 2: //recieving request to sync images
						ModPacket syncPack = ModContent.GetInstance<MultiDraw>().GetPacket();
						syncPack.Write((byte) 3);
						syncPack.Write(reader.ReadInt32());
						syncPack.Write((Int32) ModContent.GetInstance<MDModSystem>().images.Count);
						for (int i = 0; i < ModContent.GetInstance<MDModSystem>().images.Count; i++) {
							syncPack.WriteVector2(ModContent.GetInstance<MDModSystem>().images[i].pos);
							syncPack.Write(ModContent.GetInstance<MDModSystem>().images[i].scale);
							syncPack.Write((Int32) ModContent.GetInstance<MDModSystem>().images[i].image);
						}

						syncPack.Send();

						break;

					default:
						Main.NewText("AHHHHHHHHHHHHHHHH");
						Console.WriteLine("OHHHH NOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
						break;
				}
			}
			if (Main.netMode != NetmodeID.Server) {
				switch (reader.ReadByte()) {
					case (byte) 0:
						int owner = reader.ReadInt32();		//int (owner)
						int len = reader.ReadInt32();		//int (len)

						List<Image> buf = new List<Image>();
						for (int i = 0; i < len; i++) {
							Vector2 pos = reader.ReadVector2();
							float scale = reader.ReadSingle();
							int image = reader.ReadInt32();
							buf.Add(new Image(pos, scale, image));
						}

						if (Main.myPlayer != owner) {
							for (int i = 0; i < buf.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(buf[i]);
							}
						}

						break;

					case (byte) 1:
						int eraseOwner = reader.ReadInt32();
						int eraseLen = reader.ReadInt32();

						List<int> indexesToBeErased = new List<int>();
						for (int i = 0; i < eraseLen; i++) {
							indexesToBeErased.Add(reader.ReadInt32());
						}
						if (Main.myPlayer != eraseOwner) {
							for (int i = 0; i < indexesToBeErased.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.RemoveAt(indexesToBeErased[i]);
							}
						}

						break;

					case (byte) 2:
						Main.NewText("Error occured, receieved packet with identifier 2");
						Console.WriteLine("Error occured, client receieved packet with identifier 2");
						break;

					case (byte) 3: //synchronize client
						int playerToBeSynced = reader.ReadInt32();		//int (owner)
						int syncLen = reader.ReadInt32();		//int (len)

						List<Image> syncBuf = new List<Image>();
						for (int i = 0; i < syncLen; i++) {
							Vector2 pos = reader.ReadVector2();
							float scale = reader.ReadSingle();
							int image = reader.ReadInt32();
							syncBuf.Add(new Image(pos, scale, image));
						}

						if (Main.myPlayer == playerToBeSynced) {
							for (int i = 0; i < syncBuf.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(syncBuf[i]);
							}
						}
						break;

					default:
						Console.WriteLine("BRUUUUUUUUUH");
						break;
				}
			}
		}

	}

	public class Image {
		public Vector2 pos;
		public float scale;
		public int image;

		public Image(Vector2 inPos, float inScale, int inImage) {
			pos = inPos;
			scale = inScale;
			image = inImage;
		}
	}
}
