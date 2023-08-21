using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TeltonikaDualCamTcpNetBase.BaseClasses;
using TeltonikaDualCamTcpNetBase.VideoProcessor;

namespace TeltonikaDualCamTcpLIstener
{
    public partial class TeltonikaDualCamTcpListener : ServiceBase
    {
        public TeltonikaDualCamTcpListener()
        {
            InitializeComponent();
        }

        private ListenerRunner _listenerRunner;
        protected override void OnStart(string[] args)
        {
            int port =Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
            string ipAddress = ConfigurationManager.AppSettings["TeltonikaIpAddress"];
            IVideoProcessor teltonikaVideoProcessor = new TeltonikaVideoProcessor();
            _listenerRunner = new ListenerRunner(port, ipAddress, teltonikaVideoProcessor);
            _listenerRunner.Start();
        }

        protected override void OnStop()
        {
            if (_listenerRunner!= null)
            {
                _listenerRunner.Stop();
            }
        }
    }
}
