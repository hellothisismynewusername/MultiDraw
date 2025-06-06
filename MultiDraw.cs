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
using Terraria.GameContent;

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
							float scale0 = reader.ReadSingle();
							int image = reader.ReadInt32();
							buf.Add(new Image(pos, scale0, image));
						}
						for (int i = 0; i < buf.Count; i++) {
							ModContent.GetInstance<MDModSystem>().images.Add(buf[i]);

							pack.WriteVector2(buf[i].pos);
							pack.Write(buf[i].scale);
							pack.Write((Int32) buf[i].image);
						}

						pack.Send();
						break;


					//erasure
					case (byte) 1:
						ModPacket erasePack = ModContent.GetInstance<MultiDraw>().GetPacket();

						erasePack.Write((byte) 1);

						erasePack.Write(reader.ReadInt32());    //owner

						float scale = reader.ReadSingle();
						erasePack.Write((float) scale);

						int eraseLen = reader.ReadInt32();
						erasePack.Write((Int32) eraseLen);

						Console.WriteLine($"eraseLen is {eraseLen}");

						var images = ModContent.GetInstance<MDModSystem>().images;

						for (int i = 0; i < eraseLen; i++) {
							Vector2 erasePosition = reader.ReadVector2();
							for (int j = images.Count - 1; j > 0; j--) {
								 if (images[j].pos.X >= erasePosition.X - scale * 5f && images[j].pos.X <= erasePosition.X + scale * 5f &&
									images[j].pos.Y >= erasePosition.Y - scale * 5f && images[j].pos.Y <= erasePosition.Y + scale * 5f) {
									images.RemoveAt(j);
								}
							}
							erasePack.WriteVector2(erasePosition);
						}

						//erasePack.Send();
						break;

					case (byte) 2: //recieving request to sync images 
					{

						int requester = reader.ReadInt32();
						Console.WriteLine($"MultiDraw: recieved request to sync. requester is {requester}");

						int totalBytesPayload = 16 * ModContent.GetInstance<MDModSystem>().images.Count;
						int numPackets = (int) Math.Ceiling((float) totalBytesPayload / 65535f) + 1; //just in case there's enough space for the payloads but not enough for the headers (rare chance)
						Console.WriteLine($"MultiDraw: totalBytesPayload {totalBytesPayload} numPackets {numPackets} image count {ModContent.GetInstance<MDModSystem>().images.Count}");
						List<List<byte>> packets = new List<List<byte>>();
						int offset = 0;
						int lenPayloadPerPacket = ModContent.GetInstance<MDModSystem>().images.Count / numPackets;
						for (int i = 0; i < numPackets; i++) {
							packets.Add(new List<byte>());

							//header
							if (i == 0) {
								packets[i].Add((byte) 3);
							} else {
								packets[i].Add((byte) 4);
							}
							for (int a = 0; a < BitConverter.GetBytes(requester).Count(); a++) {
								packets[i].Add(BitConverter.GetBytes(requester)[a]);
							}
							
							//Console.WriteLine($"MultiDraw: the lenPayloadPerPacket is {lenPayloadPerPacket}");
							if (i == numPackets - 1) {
								Console.WriteLine($"MultiDraw: last packet, extra {ModContent.GetInstance<MDModSystem>().images.Count - (lenPayloadPerPacket * numPackets)}");
								for (int a = 0; a < BitConverter.GetBytes(lenPayloadPerPacket + (ModContent.GetInstance<MDModSystem>().images.Count - (lenPayloadPerPacket * numPackets))).Count(); a++) {
									packets[i].Add(BitConverter.GetBytes(lenPayloadPerPacket + (ModContent.GetInstance<MDModSystem>().images.Count - (lenPayloadPerPacket * numPackets)))[a]);
								}
							} else {
								for (int a = 0; a < BitConverter.GetBytes(lenPayloadPerPacket).Count(); a++) {
									packets[i].Add(BitConverter.GetBytes(lenPayloadPerPacket)[a]);
								}
							}

							//Console.WriteLine($"MultiDraw: from {offset} to {lenPayloadPerPacket * (i + 1)}");
							//payload
							for (int a = offset; a < lenPayloadPerPacket * (i + 1); a++) {
								//if (lenPayloadPerPacket * (i + 1) < ModContent.GetInstance<MDModSystem>().images.Count) { //the dividing can cause some off-by-one errors
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.X)[b]);
									}
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.Y)[b]);
									}
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].scale)[b]);
									}
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].image)[b]);
									}
								//}
							}
							offset = lenPayloadPerPacket * (i + 1);
						}
						//missed a little bit but there should always be a bit of extra space in every packet, so it'll add the missing little amount in the last packet.
						if (lenPayloadPerPacket * numPackets < totalBytesPayload) {
							Console.WriteLine($"MultiDraw: leftover ({ModContent.GetInstance<MDModSystem>().images.Count - lenPayloadPerPacket * numPackets}). len {ModContent.GetInstance<MDModSystem>().images.Count} done {lenPayloadPerPacket * numPackets}");
							for (int a = lenPayloadPerPacket * numPackets; a < ModContent.GetInstance<MDModSystem>().images.Count; a++) {
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.X)[b]);
								}
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.Y)[b]);
								}
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].scale)[b]);
								}
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].image)[b]);
								}
							}
						}
						//Console.WriteLine("saving time");
						//saving packets to a file to view them and making the real packets and sending them
						//string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
						for (int i = 0; i < numPackets; i++) {
							//StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "aaaaSyncPacket" + i.ToString() + ".txt"), false);

							ModPacket actualPacket = ModContent.GetInstance<MultiDraw>().GetPacket();

							for (int j = 0; j < packets[i].Count; j++) {
								/*if (j % 4 == 1) { //there's one byte at the beginning offsetting everything
									outputFile.WriteLine("");
								}
								outputFile.WriteLine(packets[i][j]);*/

								actualPacket.Write((byte) packets[i][j]);
							}
							//outputFile.Close();

							actualPacket.Send();
						}

						Console.WriteLine("MultiDraw: finished syncing");
						break;

					}


					case 3:
					{

						int requester = reader.ReadInt32();
						Console.WriteLine($"MultiDraw: recieved request to sync everybody. requester is {requester}");

						int totalBytesPayload = 16 * ModContent.GetInstance<MDModSystem>().images.Count;
						int numPackets = (int) Math.Ceiling((float) totalBytesPayload / 65535f) + 1; //just in case there's enough space for the payloads but not enough for the headers (rare chance)
						Console.WriteLine($"MultiDraw: totalBytesPayload {totalBytesPayload} numPackets {numPackets} image count {ModContent.GetInstance<MDModSystem>().images.Count}");
						List<List<byte>> packets = new List<List<byte>>();
						int offset = 0;
						int lenPayloadPerPacket = ModContent.GetInstance<MDModSystem>().images.Count / numPackets;
						for (int i = 0; i < numPackets; i++) {
							packets.Add(new List<byte>());

							//header
							if (i == 0) {
								packets[i].Add((byte) 5);
							} else {
								packets[i].Add((byte) 6);
							}
							for (int a = 0; a < BitConverter.GetBytes(requester).Count(); a++) {
								packets[i].Add(BitConverter.GetBytes(requester)[a]);
							}
							
							//Console.WriteLine($"MultiDraw: the lenPayloadPerPacket is {lenPayloadPerPacket}");
							if (i == numPackets - 1) {
								Console.WriteLine($"MultiDraw: last packet, extra {ModContent.GetInstance<MDModSystem>().images.Count - (lenPayloadPerPacket * numPackets)}");
								for (int a = 0; a < BitConverter.GetBytes(lenPayloadPerPacket + (ModContent.GetInstance<MDModSystem>().images.Count - (lenPayloadPerPacket * numPackets))).Count(); a++) {
									packets[i].Add(BitConverter.GetBytes(lenPayloadPerPacket + (ModContent.GetInstance<MDModSystem>().images.Count - (lenPayloadPerPacket * numPackets)))[a]);
								}
							} else {
								for (int a = 0; a < BitConverter.GetBytes(lenPayloadPerPacket).Count(); a++) {
									packets[i].Add(BitConverter.GetBytes(lenPayloadPerPacket)[a]);
								}
							}

							//Console.WriteLine($"MultiDraw: from {offset} to {lenPayloadPerPacket * (i + 1)}");
							//payload
							for (int a = offset; a < lenPayloadPerPacket * (i + 1); a++) {
								//if (lenPayloadPerPacket * (i + 1) < ModContent.GetInstance<MDModSystem>().images.Count) { //the dividing can cause some off-by-one errors
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.X)[b]);
									}
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.Y)[b]);
									}
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].scale)[b]);
									}
									for (int b = 0; b < 4; b++) {
										packets[i].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].image)[b]);
									}
								//}
							}
							offset = lenPayloadPerPacket * (i + 1);
						}
						//missed a little bit but there should always be a bit of extra space in every packet, so it'll add the missing little amount in the last packet.
						if (lenPayloadPerPacket * numPackets < totalBytesPayload) {
							Console.WriteLine($"MultiDraw: leftover ({ModContent.GetInstance<MDModSystem>().images.Count - lenPayloadPerPacket * numPackets}). len {ModContent.GetInstance<MDModSystem>().images.Count} done {lenPayloadPerPacket * numPackets}");
							for (int a = lenPayloadPerPacket * numPackets; a < ModContent.GetInstance<MDModSystem>().images.Count; a++) {
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.X)[b]);
								}
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].pos.Y)[b]);
								}
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].scale)[b]);
								}
								for (int b = 0; b < 4; b++) {
									packets[packets.Count - 1].Add(BitConverter.GetBytes(ModContent.GetInstance<MDModSystem>().images[a].image)[b]);
								}
							}
						}
						//Console.WriteLine("saving time");
						//saving packets to a file to view them and making the real packets and sending them
						//string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
						for (int i = 0; i < numPackets; i++) {
							//StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "aaaaSyncPacket" + i.ToString() + ".txt"), false);

							ModPacket actualPacket = ModContent.GetInstance<MultiDraw>().GetPacket();

							for (int j = 0; j < packets[i].Count; j++) {
								/*if (j % 4 == 1) { //there's one byte at the beginning offsetting everything
									outputFile.WriteLine("");
								}
								outputFile.WriteLine(packets[i][j]);*/

								actualPacket.Write((byte) packets[i][j]);
							}
							//outputFile.Close();

							actualPacket.Send();
						}

						Console.WriteLine("MultiDraw: finished syncing");
						break;

					}


					default:
						Console.WriteLine("MultiDraw: Error occured");
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
							float scale1 = reader.ReadSingle();
							int image = reader.ReadInt32();
							buf.Add(new Image(pos, scale1, image));
						}

						if (Main.myPlayer != owner) {
							for (int i = 0; i < buf.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(buf[i]);
							}
						}

						break;

					//erasure
					case (byte) 1:

						var images = ModContent.GetInstance<MDModSystem>().images;

						int eraseOwner = reader.ReadInt32();
						float scale = reader.ReadSingle();
						int eraseLen = reader.ReadInt32();

						Main.NewText($"eraseLen is {eraseLen}");

						List<Vector2> erasePositions = new();
						for (int i = 0; i < eraseLen; i++)
						{
							erasePositions.Add(reader.ReadVector2());
						}

						if (Main.myPlayer != eraseOwner) {

							for (int i = 0; i < erasePositions.Count; i++)
							{
								Vector2 erasePosition = reader.ReadVector2();
								for (int j = images.Count - 1; j > 0; j--) {
									 if (images[j].pos.X >= erasePosition.X - scale * 5f && images[j].pos.X <= erasePosition.X + scale * 5f &&
										images[j].pos.Y >= erasePosition.Y - scale * 5f && images[j].pos.Y <= erasePosition.Y + scale * 5f) {
										images.RemoveAt(j);
									}
								}
							}
						}

						break;

					case (byte) 2:
						Main.NewText("Error occured, receieved packet with identifier 2");
						Console.WriteLine("Error occured, client receieved packet with identifier 2");
						break;

					case (byte) 3: { //synchronize client
						int playerToBeSynced = reader.ReadInt32();
						//Main.NewText($"initial syncing packet. requester is {playerToBeSynced}");
						
						int length = reader.ReadInt32();
						List<Image> buffer = new List<Image>();
						for (int i = 0; i < length; i++) {
							Vector2 pos = reader.ReadVector2();
							float scale2 = reader.ReadSingle();
							int image = reader.ReadInt32();
							buffer.Add(new Image(pos, scale2, image));
						}

						if (Main.myPlayer == playerToBeSynced) {
							Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Clear();
							for (int i = 0; i < buffer.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(buffer[i]);
							}
						}

						break;
					}

					case (byte) 4: {
						int playerToBeSynced = reader.ReadInt32();
						//Main.NewText($"secondary syncing packet(s). requester is {playerToBeSynced}");

						int length = reader.ReadInt32();
						List<Image> buffer = new List<Image>();
						for (int i = 0; i < length; i++) {
							Vector2 pos = reader.ReadVector2();
							float scale3 = reader.ReadSingle();
							int image = reader.ReadInt32();
							buffer.Add(new Image(pos, scale3, image));
						}

						if (Main.myPlayer == playerToBeSynced) {
							for (int i = 0; i < buffer.Count; i++) {
								Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(buffer[i]);
							}
						}

						break;
					}


					case (byte) 5: { //synchronize everybody
						int playerToBeSynced = reader.ReadInt32();
						//Main.NewText($"initial syncing packet. requester is {playerToBeSynced}");
						
						int length = reader.ReadInt32();
						List<Image> buffer = new List<Image>();
						for (int i = 0; i < length; i++) {
							Vector2 pos = reader.ReadVector2();
							float scale2 = reader.ReadSingle();
							int image = reader.ReadInt32();
							buffer.Add(new Image(pos, scale2, image));
						}

						Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Clear();
						for (int i = 0; i < buffer.Count; i++) {
							Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(buffer[i]);
						}
						

						break;
					}

					case (byte) 6: {
						int playerToBeSynced = reader.ReadInt32();
						//Main.NewText($"secondary syncing packet(s). requester is {playerToBeSynced}");

						int length = reader.ReadInt32();
						List<Image> buffer = new List<Image>();
						for (int i = 0; i < length; i++) {
							Vector2 pos = reader.ReadVector2();
							float scale3 = reader.ReadSingle();
							int image = reader.ReadInt32();
							buffer.Add(new Image(pos, scale3, image));
						}

						for (int i = 0; i < buffer.Count; i++) {
							Main.player[Main.myPlayer].GetModPlayer<MDPlayer>().images.Add(buffer[i]);
						}
						

						break;
					}

					default:
						Main.NewText("MultiDraw: Error occured");
						Console.WriteLine("MultiDraw: Error occured");
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
