using HashCreatorNS;
using HashDictionaryNS;
using HasherFunctionsNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace FileChanger
{
    public static class Helpers
    {

		static HashDictionary hashDict = new();
		static HashCreator hashCreated = new(hashDict, Hasher.HasherType.TOR);


		public static ulong FileNameToHash(string fileName)
		{
			uint ph;
			uint sh;
			hashCreated.hashFilename(fileName.ToLower(), out ph, out sh);
			return (ulong)ph << 32 | sh;
		}


		/// <summary>
		/// Compresses raw bytes using Zstandard (TOR v6 expects ZSTD).
		/// </summary>
		public static byte[] Compress(byte[] bytes)
		{

			return new Compressor()
					   .Wrap(bytes)    // returns Span<byte>
					   .ToArray();     // copy into byte[]
		}

		/// <summary>
		/// read a null-terminated ASCII string at `offset` inside `buffer`
		/// </summary>
		public static string ReadStringAt(byte[] buffer, int offset)
		{
			var sb = new StringBuilder();
			for (int i = offset; i < buffer.Length && buffer[i] != 0; i++)
				sb.Append((char)buffer[i]);
			return sb.ToString();
		}

	}
}
