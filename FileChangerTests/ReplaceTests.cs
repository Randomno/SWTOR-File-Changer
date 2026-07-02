using FileChanger;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using Xunit.Abstractions;

namespace FileChangerTests
{
	// TODO don't copy the test files from source to bin, because that's stupid
	// I'm not sure the backup of the .tor is necessary since the files get copied
	// but the replace files function should be changed anyway so it doesn't necessarily write to disk
	public class ReplaceTests
	{
		private readonly ITestOutputHelper output;
		private FileReplacer replacer = new();
		public ReplaceTests(ITestOutputHelper output)
		{
			//logging
			this.output = output;
		}

		/// <summary>
		/// TBA
		/// </summary>
		[Fact]
		public void ReplaceAndVerify()
		{
		}
	}
}
