using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyPaint.Utils
{
    public class ConnectedEventArgs: EventArgs
    {
        public string ConnectionToken { get; set; }

        public ConnectedEventArgs(string connectionToken)
        {
            ConnectionToken = connectionToken;
        }
    }
}
