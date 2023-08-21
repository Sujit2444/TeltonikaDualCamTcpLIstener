using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeltonikaDualCamTcpNetBase.BaseClasses;
using TeltonikaDualCamTcpNetBase.VideoProcessor;

namespace TeltonikaDualCamTcpListenerForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Utils.LogToFile(3, "[INFO]", "Calling Form1()");
            InitializeComponent();
            StartListenerRunner();
        }
        private void StartListenerRunner()
        {
            Utils.LogToFile(3, "[INFO]", "Calling StartListenerRunner()");
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
            string ipAddress = ConfigurationManager.AppSettings["TeltonikaIpAddress"];
            IVideoProcessor teltonikaVideoProcessor = new TeltonikaVideoProcessor();
            ListenerRunner _listenerRunner = new ListenerRunner(port, ipAddress, teltonikaVideoProcessor);
            _listenerRunner.Start();
        }
    }
}
