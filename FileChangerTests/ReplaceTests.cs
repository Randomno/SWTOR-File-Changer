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
		/// Replaces a file in an archive, then extracts the same file from the updated archive and tests if they are identical
		/// </summary>
		[Fact]
		public void ReplaceAndVerify()
		{
			var sha1 = SHA1.Create();
			//string orig = "bwaui_nameplates.gfx"; // the original file, stored in this test directory
			string path = "/resources/gfx/gfx_production/bwaui_nameplates.gfx"; // the internal path of the file
			ulong testHash = Helpers.FileNameToHash(path); // the hash of the file path as used internally
			string replacement = "bwaui_nameplates_edit.gfx"; // the replacement file, stored in this test directory
			string torFilePath = "main_gfx_1.tor"; // the tor archive, stored in this test directory
			string torFileBackupPath = torFilePath + ".bak";
			File.Copy(torFilePath, torFileBackupPath, overwrite: true);

			byte[] replacementExpected = File.ReadAllBytes(replacement);

			Hashtable changeList = new();
			changeList.Add(testHash, replacement);
			Hashtable origNamesList = new();
			origNamesList.Add(testHash, path);
			replacer.LoadArchiveReplaceFiles(torFilePath, false, false, changeList, origNamesList, replaceDir: "");

			List<string> torFiles = new() { torFilePath };
			Env env = Env.Live;
			byte[] replacementActual = replacer.ExtractFile(path, torFiles, env);

			string newTorSha1 = Convert.ToHexString(sha1.ComputeHash
				(File.ReadAllBytes(torFilePath))
				);

			// temporary check that none of the file changing logic has changed while refactoring
			Assert.Equal("6226DE0923698578151318CC33E11015C8F46345", newTorSha1);

			// known failure
			Assert.Equal(replacementExpected, replacementActual);

			File.Copy(torFileBackupPath, torFilePath, overwrite: true);
			File.Delete(torFileBackupPath);
		}
	}
}
