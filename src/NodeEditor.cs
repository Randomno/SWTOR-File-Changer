using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace FileChanger
{
    class NodeEditor
    {
		private readonly ILogger logger;
		public NodeEditor(ILogger logger = null)
        {
            this.logger = logger ?? new ConsoleLogger();
        }
    }
}
