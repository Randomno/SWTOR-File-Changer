using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWTOR_File_Changer
{
    class Settings
	{
		public string InstallationFolder { get; set; }
		public bool CreateBackup { get; set; }
		public bool EditNode { get; set; }
		public Hashtable ChangeList { get; set; }
		public Hashtable OrigNamesList { get; set; }
		public Hashtable NodeChangeList { get; set; }
		public Hashtable BucketList { get; set; }
	}
}
