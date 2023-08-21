using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeltonikaDualCamTcpNetBase.Codec
{
    public class IntializationCommand
    {

        private static Dictionary<string, string> _typeOfRequests = new Dictionary<string, string> { { "1000100", "%photof" }, { "1001000", "%photor" }, { "1010000", "%videof" }, { "1100000", "%videor" } };
        public static Dictionary<string, string> TypeOfRequests { get{return _typeOfRequests;} }
        
        public short ProtocolId { get; set; }
        public string IMEI { get; set; }
        public string Settings { get; set; }
    }

    public class StartCommand
    {
        public short CommandId { get; set; }
        public short DataLength { get; set; }
        public int FilePackets { get; set; }
    }

    public class SyncCommand
    {
        public short CommandId { get; set; }
        public short DataLength { get; set; }
        public int FileOffset { get; set; }

    }

    public class FileDataTransferCommand
    {
        public short CommandId { get; set; }
        public short DataLength { get; set; }
        public byte[] FileData { get; set; }
        public short DataCrc { get; set; }
    }

    public class FileMetadataResponse
    {
        public short CommandId { get; set; }
        public short DataLength { get; set; }
        public long Timestamp { get; set; }
        public short VideoLength { get; set; }
    }
}
