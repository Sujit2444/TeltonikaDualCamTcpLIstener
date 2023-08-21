using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeltonikaDualCamTcpNetBase.VideoProcessor;

namespace TeltonikaDualCamTcpNetBase.BaseClasses
{
    public class ListenerRunner
    {
        private int _port;
        private string _ipAddress;
        private Thread _listenerThread;
        private TcpServer _tcpServer;
        private IVideoProcessor _videoProcessor;
        private Thread _videoProcessorThread;
        private DatabaseSaver _databaseSaver;
        public ListenerRunner(int port,string ipAddress,IVideoProcessor videoProcessor)
        {
            Utils.LogToFile(3, "[INFO]", "Calling ListenerRunner()");
            _port = port;
            _ipAddress = ipAddress;
            _videoProcessor = videoProcessor;
            CreateAll();
        }

        private void CreateAll()
        {
            Utils.LogToFile(3, "[INFO]", "Calling CreateAll()");
            _tcpServer = new TcpServer(_ipAddress, _port,_videoProcessor);
            _listenerThread = new Thread(_tcpServer.Run);
            _listenerThread.IsBackground = true;
            _databaseSaver = new DatabaseSaver();
            if (_videoProcessor != null)
            {
                _videoProcessor.SetDbSaver(_databaseSaver);
                _videoProcessorThread = new Thread(_videoProcessor.RunVideoProcessing);
                _videoProcessorThread.IsBackground = true;
            }          
        }

        public void Start()
        {
            Utils.LogToFile(3, "[INFO]", "Calling Start()");
            _listenerThread.Start();
            _videoProcessorThread.Start();
        }

        public void Stop()
        {
            Utils.LogToFile(3, "[INFO]", "Calling Stop()");
            _listenerThread.Abort();
            _videoProcessor.Stopped = true;
            _videoProcessorThread.Abort();
            Thread.Sleep(100);
        }
    }
}
