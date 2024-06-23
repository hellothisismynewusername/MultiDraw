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
using Humanizer;

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

						List<Byte> total = new List<Byte>();
						total.Add((byte) 3);
						int requester = reader.ReadInt32();
						for (int i = 0; i < 4; i++) {
							total.Add(BitConverter.GetBytes(requester)[i]);
						}
						for (int i = 0; i < ModContent.GetInstance<MDModSystem>().images.Count; i++) {
							for (int j = 0; j < 4; j++) {
								total.Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[i].pos.X)[j]);
							}
							for (int j = 0; j < 4; j++) {
								total.Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[i].pos.Y)[j]);
							}
							for (int j = 0; j < 4; j++) {
								total.Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[i].scale)[j]);
							}
							for (int j = 0; j < 4; j++) {
								total.Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[i].image)[j]);
							}
							for (int j = 0; j < 4; j++) {
								total.Add(BitConverter.GetBytes(-251662081)[j]);
							}
						}

						Console.WriteLine($"Sync packet size before splitting is {total.Count}");

						int count = 0;
						byte thirdAgo = 0;
						byte secondAgo = 0;
						byte firstAgo = 0;
						List<List<byte>> lists = new List<List<byte>>();
						int thing = 0;
						for (int i = 0; i < total.Count; i++) {
							if (i > 1) {
								firstAgo = total[i - 1];
							}
							if (i > 2) {
								secondAgo = total[i - 2];
							}
							if (i > 3) {
								thirdAgo = total[i - 3];
							}
							if (count > 65450 && total[i] == 240 && firstAgo == 255 && secondAgo == 240 && thirdAgo == 255) {
								//change sentinel to section end
								total[i - 2] = 255;
								total.Insert(i + 1, (byte) 4);
								for (int h = 1; h < 4 + 1; h++) {
									total.Insert(i + 1 + h, BitConverter.GetBytes(requester)[h - 1]);
								}
								count = 0;
								Console.WriteLine($"splitting FUCK {total[i - 3]} FUCK {total[i - 2]} FUCK {total[i - 1]} FUCK {total[i]}");

								lists.Add(new List<byte>());
								for (int a = thing; a < i + 1; a++) {
									lists[lists.Count - 1].Add(total[a]);
								}
								thing = i;
							}
							count++;
							if (total[i] == 240 && firstAgo == 255 && secondAgo == 240 && thirdAgo == 255) {
								for (int a = 0; a < 4; a++) {
									total.RemoveAt(i - 3);
								}
							}
						}
						Console.WriteLine($"there is {lists.Count} lists");
						for (int a = 0; a < lists.Count; a++) {
							Console.WriteLine($"list {a} has {lists[a].Count} items");
						}
						Console.WriteLine($"Sync total size after splitting is {total.Count}, but isn't this useless now?");
						int iterations = lists.Count;

						for (int i = 0; i < iterations; i++) {
							ModPacket splitSyncPack = ModContent.GetInstance<MultiDraw>().GetPacket();
							string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
							StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "aaaaPacket" + i.ToString() + ".txt"), false);
							for (int j = 0; j < lists[i].Count; j++) {
								outputFile.WriteLine(lists[i][j]);
								splitSyncPack.Write(lists[i][j]);
							}
							splitSyncPack.Send();
						}

						/*ModPacket syncPack = ModContent.GetInstance<MultiDraw>().GetPacket();
						syncPack.Write((byte) 3);
						syncPack.Write(reader.ReadInt32());
						syncPack.Write((Int32) ModContent.GetInstance<MDModSystem>().images.Count);
						for (int i = 0; i < ModContent.GetInstance<MDModSystem>().images.Count; i++) {
							syncPack.WriteVector2(ModContent.GetInstance<MDModSystem>().images[i].pos);
							syncPack.Write(ModContent.GetInstance<MDModSystem>().images[i].scale);
							syncPack.Write((Int32) ModContent.GetInstance<MDModSystem>().images[i].image);
						}

						syncPack.Send();*/

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

					case (byte) 3: { //synchronize client
						int playerToBeSynced = reader.ReadInt32();		//int (owner)
						Main.NewText($"initial syncing packet. requester is {playerToBeSynced}");

						List<Image> syncBuf = new List<Image>();
						bool didNotMatch = true;
						string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
						StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "aaaa.txt"), true);
						while (didNotMatch) {
							Vector2 pos = reader.ReadVector2();
							float scale = reader.ReadSingle();
							int image = reader.ReadInt32();
							syncBuf.Add(new Image(pos, scale, image));
							
							int read = reader.ReadInt32();
							outputFile.WriteLine($"a read session occured. the var read is {read}");
							if (read == -251658241) {
								didNotMatch = false;
								outputFile.WriteLine($"YOOOOO MATCHED");
							}
							for (int a = 0; a < BitConverter.GetBytes(read).Count(); a++) {
								outputFile.WriteLine(BitConverter.GetBytes(read)[a]);
							}
							reader.BaseStream.Seek(-4, SeekOrigin.Current);

							outputFile.WriteLine("");
						}

						if (Main.myPlayer == playerToBeSynced) {
							for (int i = 0; i < syncBuf.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Clear();
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(syncBuf[i]);
							}
						}
						}
						
						break;

					case (byte) 4: {
						int requester = reader.ReadInt32();
						Main.NewText($"secondary syncing packet(s). requester is {requester}");

						List<Image> syncBuf2 = new List<Image>();
						int read2 = reader.ReadInt32();
						string docPath2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
						StreamWriter outputFile = new StreamWriter(Path.Combine(docPath2, "aaaa.txt"), true);
						outputFile.WriteLine("BEGINNING SECONDARY:");
						bool didNotMatch = true;
						string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
						while (didNotMatch) {
							Vector2 pos = reader.ReadVector2();
							float scale = reader.ReadSingle();
							int image = reader.ReadInt32();
							syncBuf2.Add(new Image(pos, scale, image));
							
							int read = reader.ReadInt32();
							outputFile.WriteLine($"a read session occured. the var read is {read}");
							if (read == -251658241) {
								didNotMatch = false;
								outputFile.WriteLine($"YOOOOO MATCHED");
							}
							for (int a = 0; a < BitConverter.GetBytes(read).Count(); a++) {
								outputFile.WriteLine(BitConverter.GetBytes(read)[a]);
							}
							reader.BaseStream.Seek(-4, SeekOrigin.Current);

							outputFile.WriteLine("");
						}

						if (Main.myPlayer == requester) {
							for (int i = 0; i < syncBuf2.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(syncBuf2[i]);
							}
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
