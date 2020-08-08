﻿using System.IO;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.DarkColony.SpriteLoaders
{
	public class BTSLoader : ISpriteLoader
	{
		class BTSFrame : ISpriteFrame
		{
			public Size Size { get; set; }

			public Size FrameSize { get; set; }

			public float2 Offset { get; set; }

			public byte[] Data { get; set; }

			public bool DisableExportPadding { get; set; }

			public SpriteFrameType Type => SpriteFrameType.Indexed;
		}

		static ISpriteFrame[] ParseFrames(BinaryReader reader, int numFrames)
		{
			var frames = new BTSFrame[numFrames];

			for (var i = 0; i < numFrames; i++)
			{
				var id = reader.ReadInt32();
				var pixels = reader.ReadBytes(32 * 32);

				frames[i] = new BTSFrame
				{
					Size = new Size(32, 32),
					FrameSize = new Size(32, 32),
					Offset = new float2(0, 0),
					Data = pixels
				};
			}

			return frames;
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			metadata = new TypeDictionary();

			if (Path.GetExtension(((FileStream)s).Name) != ".BTS")
			{
				frames = null;
				return false;
			}

			using (var reader = new BinaryReader(s))
			{
				var unkSize = reader.ReadUInt32(); // TODO could be filesize related
				var numFrames = reader.ReadInt32();

				reader.BaseStream.Position += 256 * 3; // We skip the palette here...

				frames = ParseFrames(reader, numFrames);
            }

			return true;
		}
	}
}
