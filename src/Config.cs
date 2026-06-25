using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileChanger
{
    public class Config
	{
		public List<string> torFiles { get; set; } = new();
		public Dictionary<string, string> changeList { get; set; } = new(); // game path, replacement file
		public Dictionary<ulong, string> hashChangeList { get; set; } = new(); // hash, replacement file
		public Dictionary<string, string> nodeChangeList { get; set; } = new(); // node, replacement file

		//public HashSet<ulong> bucketList { get; set; } = new(); // todo don't pass this?
		public bool createBackup { get; set; }
	}
}
