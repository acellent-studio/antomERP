using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace antom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string host = Dns.GetHostName();
            IPAddress[] ipList = Dns.GetHostAddresses(host);
            foreach(var ip in ipList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    webBrowser1.Navigate("http://" + ip.ToString());
                }
            }

        }
    }
}
