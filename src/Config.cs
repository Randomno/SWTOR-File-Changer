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
		public List<string> torFiles { get; set; }
		public Dictionary<string, string> changeList { get; set; }
		public Dictionary<string, string> nodeChangeList { get; set; }
		// todo don't pass this?
		public HashSet<ulong> bucketList { get; set; } = new HashSet<ulong>();
		public bool createBackup { get; set; }
	}
}
