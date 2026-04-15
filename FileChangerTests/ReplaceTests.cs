using FileChanger;
using System.Collections;
using System.Security.Cryptography;

namespace FileChangerTests
{
	// TODO don't copy the test files from source to bin, because that's stupid
	// I'm not sure the backup of the .tor is necessary since the files get copied
	// but the replace files function should be changed anyway so it doesn't necessarily write to disk
	public class ReplaceTests
	{
		private FileReplacer replacer = new();

		/// <summary>
		/// Replaces a file in an archive, then extracts the same file from the updated archive and tests if they are identical
		/// </summary>
		[Fact]
		public void ReplaceAndVerify()
		{
			//var sha1 = SHA1.Create();
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

			List<string> files = new() { torFilePath };
			byte[] replacementActual = replacer.ExtractFile(testHash, files, false, true);

			Assert.Equal(replacementExpected, replacementActual);

			File.Copy(torFileBackupPath, torFilePath, overwrite: true);
			File.Delete(torFileBackupPath);
		}
	}
}
