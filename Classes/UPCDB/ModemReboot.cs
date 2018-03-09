using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerIcon.Classes.UPCDB
{
    public class ModemReboot
    {

        public static void Run()
        {
            using (var client = new SshClient("192.168.0.1", "root", "zte9x15"))
            {
                client.Connect();
                //client.RunCommand("reboot");
                client.Disconnect();
            }
        }
    }
}
