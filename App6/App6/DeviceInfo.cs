using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App6
{
    public class DeviceInfo
    {
        public string DisplayName { get; set; }

        /// <summary>
        /// Raw host address
        /// </summary>
       // private void Send(string message);
        public string HostName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
