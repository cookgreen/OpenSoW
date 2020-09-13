using System;
using System.Collections.Generic;
using System.IO;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.SoW.SpriteLoaders
{
	public class Ps6Loader : ISpriteLoader
	{
		class Ps6Frame : ISpriteFrame
		{
			public Size Size { get; set; }

			public Size FrameSize { get; set; }

			public float2 Offset { get; set; }

			public byte[] Data { get; set; }

			public bool DisableExportPadding { get; set; }

			public SpriteFrameType Type => SpriteFrameType.Indexed;
		}

		static ISpriteFrame[] ParseFrames(BinaryReader reader)
		{
			List<ISpriteFrame> frames = new List<ISpriteFrame>();

			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				int size = reader.ReadInt32();

				if (size == 0)
				{
					break;
				}

				long outerPosition = reader.BaseStream.Position;

				Ps6Frame frame = new Ps6Frame();
				frames.Add(frame);

				long startPosition = reader.BaseStream.Position;

				int width = reader.ReadInt16();
				int height = reader.ReadInt16();
				frame.Size = new Size(width, height);

				int originX = reader.ReadInt16();
				int originY = reader.ReadInt16();
				frame.FrameSize = new Size(originX, originY);

				short unk1 = reader.ReadInt16(); // TODO
				short unk2 = reader.ReadInt16(); // TODO

				short unk3 = reader.ReadInt16(); // TODO
				short unk4 = reader.ReadInt16(); // TODO

				if (frame.Size.Width > 0 && frame.Size.Height > 0)
				{
					frame.Data = new byte[frame.Size.Width * frame.Size.Height];

					for (int y = 0; y < frame.Size.Height; y++)
					{
						int lineOffset = reader.ReadInt32();
						long position = reader.BaseStream.Position;
						reader.BaseStream.Position = startPosition + lineOffset * 2;

						int numCommand = reader.ReadInt16();
						int x = 0;
						bool skipMode = reader.ReadInt16() == 0x00;

						for (int i = 0; i < numCommand; i++)
						{
							if (skipMode)
							{
								x += reader.ReadInt16();
							}
							else
							{
								short readPixels = reader.ReadInt16();

								for (int j = 0; j < readPixels; j++)
								{
									int color16 = reader.ReadInt16();
									int color32 = (((color16 >> 8) & 0xf8) << 24) | (((color16 >> 3) & 0xfc) << 16) | ((color16 & 0x1f) << 11) | 0xff;
									frame.Data[x + y * frame.Size.Width] = Convert.ToByte(color32);
									x++;
								}
							}

							skipMode = !skipMode;
						}

						reader.BaseStream.Position = (position);
					}
				}

				reader.BaseStream.Position = (outerPosition + size);
			}

			return frames.ToArray();
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			metadata = new TypeDictionary();

			if (Path.GetExtension(((FileStream)s).Name) != ".ps6")
			{
				frames = null;
				return false;
			}

			using (var reader = new BinaryReader(s))
			{
				frames = ParseFrames(reader);
            }

			return true;
		}
	}
}
