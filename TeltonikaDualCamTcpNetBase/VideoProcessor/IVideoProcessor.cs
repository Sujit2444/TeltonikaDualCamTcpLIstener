using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeltonikaDualCamTcpNetBase.BaseClasses;

namespace TeltonikaDualCamTcpNetBase.VideoProcessor
{
    public interface IVideoProcessor
    {
       void Add(TcpState tcpState);
       void RunVideoProcessing();
       bool Stopped { get; set; }

       void SetDbSaver(DatabaseSaver databaseSaver);
    }
}
