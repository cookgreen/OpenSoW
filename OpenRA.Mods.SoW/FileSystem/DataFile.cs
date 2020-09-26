using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRA.FileSystem;
using OpenRA.Primitives;

namespace OpenRA.Mods.SoW.FileSystem
{
	public class DataLoader : IPackageLoader
	{
		public class DataFile : IReadOnlyPackage
		{
			public struct DataFileEntry
			{
				public readonly int Offset;
				public readonly int Length;

				public DataFileEntry(int offset, int length)
				{
					Offset = offset;
					Length = length;
				}
			}

			private Stream dataStream;
			private Stream indexStream;
			private Dictionary<string, DataFileEntry> index;
			public string Name { get; private set; }

			public IEnumerable<string> Contents { get { return index.Keys; } }

			public DataFile(Stream s, string filename)
			{
				Name = filename;

				index = new Dictionary<string, DataFileEntry>();

				dataStream = s;
				var dir = Path.GetDirectoryName((s as FileStream).Name);
				var name = Path.GetFileNameWithoutExtension(filename);
				indexStream = new FileStream(Path.Combine(dir, name + ".info"), FileMode.Open, FileAccess.Read);

				indexStream.ReadInt32();
				int entries = indexStream.ReadInt32();
				for (int i = 0; i < entries; i++)
				{
					int temp;
					string tempName = "";

					while ((temp = indexStream.ReadByte()) != 0x00)
					{
						tempName += (char)(temp - 0xa);
					}

					int offset = indexStream.ReadInt32();
					int length = indexStream.ReadInt32();
					index.Add(tempName, new DataFileEntry(offset, length));
				}
			}

			public bool Contains(string filename)
			{
				return index.ContainsKey(filename);
			}

			public void Dispose()
			{
				dataStream.Dispose();
			}

			public Stream GetStream(string filename)
			{
				DataFileEntry e;
				if (!index.TryGetValue(filename, out e))
					return null;

				return SegmentStream.CreateWithoutOwningStream(dataStream,  e.Offset, e.Length);
			}

			public IReadOnlyPackage OpenPackage(string filename, OpenRA.FileSystem.FileSystem context)
			{
				return null;
			}
		}

		public bool TryParsePackage(Stream s, string filename, OpenRA.FileSystem.FileSystem context, out IReadOnlyPackage package)
		{
			package = new DataFile(s, filename);
			return true;
		}
	}
}
