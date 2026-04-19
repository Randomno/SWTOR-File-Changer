using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace FileChanger
{
    public static class Helpers
    {

		// https://github.com/lgastako/jenkins/blob/master/lookup3.c (hashlittle2)
		public static ulong FileNameToHash(string f)
		{
			uint a, b, c;
			int length = f.Length;
			a = b = c = 0xDEADBEEF + ((uint)f.Length);

			int i = 0;
			while (length > 12)
			{
				a += f[i];
				a += ((uint)f[i + 1]) << 8;
				a += ((uint)f[i + 2]) << 16;
				a += ((uint)f[i + 3]) << 24;
				b += f[i + 4];
				b += ((uint)f[i + 5]) << 8;
				b += ((uint)f[i + 6]) << 16;
				b += ((uint)f[i + 7]) << 24;
				c += f[i + 8];
				c += ((uint)f[i + 9]) << 8;
				c += ((uint)f[i + 10]) << 16;
				c += ((uint)f[i + 11]) << 24;

				// mix
				a -= c; a ^= (c << 4) | (c >> 28); c += b;
				b -= a; b ^= (a << 6) | (a >> 26); a += c;
				c -= b; c ^= (b << 8) | (b >> 24); b += a;
				a -= c; a ^= (c << 16) | (c >> 16); c += b;
				b -= a; b ^= (a << 19) | (a >> 13); a += c;
				c -= b; c ^= (b << 4) | (b >> 28); b += a;

				length -= 12;
				i += 12;
			}

			switch (length)
			{
				case 12: c += ((uint)f[i + 11]) << 24; goto case 11;
				case 11: c += ((uint)f[i + 10]) << 16; goto case 10;
				case 10: c += ((uint)f[i + 9]) << 8; goto case 9;
				case 9: c += f[i + 8]; goto case 8;
				case 8: b += ((uint)f[i + 7]) << 24; goto case 7;
				case 7: b += ((uint)f[i + 6]) << 16; goto case 6;
				case 6: b += ((uint)f[i + 5]) << 8; goto case 5;
				case 5: b += f[i + 4]; goto case 4;
				case 4: a += ((uint)f[i + 3]) << 24; goto case 3;
				case 3: a += ((uint)f[i + 2]) << 16; goto case 2;
				case 2: a += ((uint)f[i + 1]) << 8; goto case 1;
				case 1: a += f[i]; break;
			}

			// final
			c ^= b; c -= (b << 14) | (b >> 18);
			a ^= c; a -= (c << 11) | (c >> 21);
			b ^= a; b -= (a << 25) | (a >> 7);
			c ^= b; c -= (b << 16) | (b >> 16);
			a ^= c; a -= (c << 4) | (c >> 28);
			b ^= a; b -= (a << 14) | (a >> 18);
			c ^= b; c -= (b << 24) | (b >> 8);

			// not c
			return (ulong)b << 32 | c;
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
