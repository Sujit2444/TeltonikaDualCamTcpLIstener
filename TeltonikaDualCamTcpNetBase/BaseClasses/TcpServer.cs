using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TeltonikaDualCamTcpNetBase.Codec;
using TeltonikaDualCamTcpNetBase.VideoProcessor;

namespace TeltonikaDualCamTcpNetBase.BaseClasses
{
    public class TcpServer
    {
        private string _ipAddress;
        private int _port;
        private Socket _listenerSocket;
        private Socket _client;
        private IVideoProcessor _videoProcessor;
        private FileType _fileType;

        public TcpServer(string ipAddress,int port, IVideoProcessor videoProcessor)
        {
            Utils.LogToFile(3, "[INFO]", "Calling TcpServer()");
            Utils.LogToFile(3, "[INFO]", string.Format("IP Address:{0}",ipAddress));
            Utils.LogToFile(3, "[INFO]", string.Format("Port:{0}",port));
            _ipAddress = ipAddress;
            _port = port;
            _videoProcessor = videoProcessor;
        }

        public void Run()
        {
            Utils.LogToFile(3, "[INFO]", "Calling Run()");
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
             _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.Bind(endPoint);
            _listenerSocket.Listen(2000);
            _listenerSocket.ReceiveTimeout = 300000;
            _listenerSocket.SendTimeout = 300000;
            int numberOfBytesRecieved = 0;
            while (true)
            {
                try
                {
                    Utils.LogToFile(3, "[INFO]", "Listener Running");
                    _client = _listenerSocket.Accept();
                    Utils.LogToFile(3, "[INFO]", "Accept A Connection From A Socket");
                    byte[] buffer = new byte[16];
                    numberOfBytesRecieved = _client.Receive(buffer);
                    Utils.LogToFile(3,"[INFO]",string.Format("Number Of Bytes Recieved:{0},Recieved Initialization Command: {1}",numberOfBytesRecieved, BitConverter.ToString(buffer)));
                    IntializationCommand intializeCommand = DecodeInitalizationCommand(buffer);

                    /*code added*/
                    var queryFileMetadataCommand = PrepareQueryFileMetadataCommand(intializeCommand.Settings);
                    _client.Send(queryFileMetadataCommand);
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Send:{0},Send Query File Meta Data Command:{1}", queryFileMetadataCommand.Length, BitConverter.ToString(queryFileMetadataCommand)));
                    buffer = new byte[36];
                    numberOfBytesRecieved = _client.Receive(buffer);
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Recieved:{0},Recieved File Meta Data Response Command:{1}", numberOfBytesRecieved, BitConverter.ToString(buffer)));
                    FileMetadataResponse fileMetadataResponse = DecodeFileMetadataResponse(buffer);

                    /*code end*/

                    byte[] fileReqCommand = PrepareFileRequestCommand(intializeCommand.Settings);
                    _client.Send(fileReqCommand);
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of Bytes Send:{0},Send File Request Command:{1}", fileReqCommand.Length, BitConverter.ToString(fileReqCommand)));
                   
                    buffer = new byte[10];
                    numberOfBytesRecieved = _client.Receive(buffer);
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Recieved:{0},Recieved Start Command:{1}", numberOfBytesRecieved, BitConverter.ToString(buffer)));
                    StartCommand startCommand = DecodeStartCommand(buffer);
                    byte[] resumeCommnad = PrepareResumeCommand();
                    _client.Send(resumeCommnad);
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Send:{0},Send Resume Command:{1}", resumeCommnad.Length, BitConverter.ToString(resumeCommnad)));
                  
                    buffer = new byte[8];
                    numberOfBytesRecieved = _client.Receive(buffer);
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Recieved:{0},Recieved Sync Command:{1}", numberOfBytesRecieved, BitConverter.ToString(buffer)));
                    SyncCommand syncCommand = DecodeSyncCommand(buffer);

                    int packetCount = startCommand.FilePackets;
                    byte[] totalDataBytes = new byte[0];
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of File Data Packets:{0}", packetCount));

                    int packetcounter = 1;

                    while (packetcounter <= packetCount)
                    {
                   
                        //if (totalDataBytes.Length >= (packetCount  * 1030))
                        //{
                        //    Utils.LogToFile(3, "[INFO]", string.Format("Inside Packet Counter Break. Total Databytes{0},Packet Counter{1}",totalDataBytes.Length,packetcounter));
                        //    break;
                        //}
                        if (totalDataBytes.Length > (packetCount - 1) * 1030)
                        {
                            break;
                        }
                        buffer = new byte[_client.SendBufferSize];
                        numberOfBytesRecieved = _client.Receive(buffer);
                        if (numberOfBytesRecieved == 0 && packetcounter <= packetCount)
                        {
                            Utils.LogToFile(3, "[INFO]", string.Format("Inside Packet Counter Continue. Total Databytes{0},Packet Counter{1}", totalDataBytes.Length, packetcounter));
                            continue;
                        }

                        byte[] dataBuffer = new byte[numberOfBytesRecieved];
                        Array.Copy(buffer, dataBuffer, dataBuffer.Length);
                        Utils.LogToFile(3, "[INFO]", string.Format("Reciving Data Part:{0}", packetcounter));
                        Utils.LogToFile(6, "[INFO]", string.Format("Packet:{0},Number Of Byte Recieved {1},Recieved Data Command:{2}", packetcounter, numberOfBytesRecieved, BitConverter.ToString(dataBuffer)));
                        totalDataBytes =totalDataBytes.Concat(dataBuffer).ToArray();
                        packetcounter++;
                    }

                 
                    byte[] completeCommand= new byte[] {0x00,0x0005,0x00, 0x0004, 0x00,0x00,0x00,0x00 };
                    _client.Send(completeCommand);
                    Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Send {0},Send Complete Command:{1}",completeCommand.Length,BitConverter.ToString(completeCommand)));

                    ///*code added*/
                    //var queryFileMetadataCommand = PrepareQueryFileMetadataCommand(intializeCommand.Settings);
                    //_client.Send(queryFileMetadataCommand);
                    //Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Send:{0},Send Query File Meta Data Command:{1}", queryFileMetadataCommand.Length, BitConverter.ToString(queryFileMetadataCommand)));
                    //buffer = new byte[36];
                    //numberOfBytesRecieved = _client.Receive(buffer);
                    //Utils.LogToFile(3, "[INFO]", string.Format("Number Of Byte Recieved:{0},Recieved File Meta Data Response Command:{1}", numberOfBytesRecieved, BitConverter.ToString(buffer)));
                    //FileMetadataResponse fileMetadataResponse = DecodeFileMetadataResponse(buffer);

                    ///*code end*/

                    if (intializeCommand.Settings.Contains("%photo"))
                    {
                        _fileType=FileType.png;
                    }
                    else
                    {
                        _fileType = FileType.mp4;
                    }

                    if (totalDataBytes.Length>0)
                    {
                        _videoProcessor.Add(new TcpState {DataBytes=totalDataBytes,PacketCount= startCommand.FilePackets, IMEI= intializeCommand.IMEI,FileType=_fileType,TimeStamp= fileMetadataResponse .Timestamp,VideoLength=fileMetadataResponse.VideoLength, MediaRequestType = intializeCommand .Settings});
                        Utils.LogToFile(3, "[INFO]", string.Format("Total Data Command Length:{0} Bytes", totalDataBytes.Length));
                    }
                    Array.Clear(buffer, 0, buffer.Length);
                    _client.Close();
                }
                catch (SocketException socketEx)
                {

                    if (_client != null)
                    {
                        byte[] closeCommand = new byte[] { 0x00, 0x00 };
                        //_client.Send(closeCommand);
                        _client.Close();
                    }
                    Utils.LogToFile(1, "[SOCKET EXCEPTION]", string.Format("SocketException: {0}", socketEx.Message));
                    // Handle the SocketException here, such as retrying the operation, closing the connection, etc.
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (_client != null)
                        {
                             byte[] closeCommand = new byte[] { 0x00,0x00 };
                             _client.Send(closeCommand);
                             _client.Close();
                        }
                    }
                    catch (SocketException socketEx)
                    {
                        Utils.LogToFile(1, "[SOCKET EXCEPTION]", string.Format("SocketException in Run(): {0}", socketEx.Message));
                      
                    }
                    catch (Exception ex1)
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", string.Format("Exception In Run():{0}", ex1.Message.ToString()));
                    }
                    Utils.LogToFile(1, "[EXCEPTION]", string.Format("Exception In Run():{0}", ex.Message.ToString()));
                }
            }

        }

        private IntializationCommand DecodeInitalizationCommand(byte[]rawData)
        {
            short protocolId=0;
            string imei = "";
            string typeOfRequest = "";
            try
            {
                Utils.LogToFile(3, "[INFO]", "Calling DecodeInitalizationCommand()");
                ReverseBinaryReader binaryReader = new ReverseBinaryReader(new MemoryStream(rawData));
                short header = binaryReader.ReadInt16();
                protocolId = binaryReader.ReadInt16();
                imei = binaryReader.ReadInt64().ToString();
                byte[] settings = binaryReader.ReadBytes(4);
                int a = settings[0];
                string binaryString = Convert.ToString(a, 2);
                typeOfRequest = IntializationCommand.TypeOfRequests[binaryString];
                Utils.LogToFile(3, "[INFO]", string.Format("Decoded Initialize Packet:Protocol Id={0},IMEI={1},Settings={2}", protocolId, imei, typeOfRequest));
            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", string.Format("Exception In DecodeInitalizationCommand():{0}", ex.Message.ToString()));
                throw;
            }
     
            return new IntializationCommand { ProtocolId = protocolId, IMEI = imei, Settings = typeOfRequest };
        }

        private byte[] PrepareFileRequestCommand(string requestType)
        {
            Utils.LogToFile(3, "[INFO]", "Calling PrepareFileRequestCommand()");
            byte[] requestTypeInBytes = Encoding.ASCII.GetBytes(requestType);
            byte[] fileReqCommand = new byte[] { 0x0000, 0x0008, 0x0000, Convert.ToByte(requestTypeInBytes.Length) };
            return fileReqCommand.Concat(requestTypeInBytes).ToArray();
        }

        private StartCommand DecodeStartCommand(byte[] rawdata)
        {
            short commandId=0;
            short dataLength = 0;
            int filePackets = 0;
            try
            {
                Utils.LogToFile(3, "[INFO]", "Calling DecodeStartCommand()");
                ReverseBinaryReader binaryReader = new ReverseBinaryReader(new MemoryStream(rawdata));
                commandId = binaryReader.ReadInt16();
                dataLength = binaryReader.ReadInt16();
                filePackets = binaryReader.ReadInt32();
                Utils.LogToFile(3, "[INFO]", string.Format("Decoded Start Command:Command Id={0}, Data Length={1},File Packets={2}", commandId, dataLength, filePackets));
            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", string.Format("Exception In DecodeStartCommand():{0}", ex.Message.ToString()));
                throw;
            }
           
            return new StartCommand { CommandId=commandId,DataLength=dataLength,FilePackets=filePackets};
        }



        private byte[] PrepareResumeCommand()
        {
            Utils.LogToFile(3, "[INFO]", "Calling PrepareResumeCommand()");
            return new byte[] { 0x0000, 0x0002, 0x0000, 0x0004, 0x0000, 0x0000, 0x0000, 0x00000001 };
        }

        private SyncCommand DecodeSyncCommand(byte[]RawData)
        {
            short commandId = 0;
            short dataLength = 0;
            int fileOffset = 0;
            try
            {
                Utils.LogToFile(3, "[INFO]", "Calling DecodeSyncCommand()");
                ReverseBinaryReader binaryReader = new ReverseBinaryReader(new MemoryStream(RawData));
                commandId = binaryReader.ReadInt16();
                dataLength = binaryReader.ReadInt16();
                fileOffset = binaryReader.ReadInt32();
                Utils.LogToFile(3, "[INFO]", string.Format("Decoded Sync Command:Command Id={0}, Data Length={1},File Offset={2}", commandId, dataLength, fileOffset));
            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", string.Format("Exception In DecodeSyncCommand():{0}", ex.Message.ToString()));
                throw;
            }      
            return new SyncCommand { CommandId=commandId,DataLength=dataLength,FileOffset=fileOffset };
        }

        private byte[] PrepareQueryFileMetadataCommand(string requestType) {

            Utils.LogToFile(3,"INFO", "Calling PrepareQueryFileMetadataCommand()");
            byte[] requestTypeInBytes = Encoding.ASCII.GetBytes(requestType);
            byte[] QueryFileMetadataCommand = new byte[] { 0x0000, 0x000A, 0x0000, Convert.ToByte(requestTypeInBytes.Length) };
            return QueryFileMetadataCommand.Concat(requestTypeInBytes).ToArray();
          }

        private FileMetadataResponse DecodeFileMetadataResponse(byte[] rawData)
        {
            short commandId=0;
            short dataLength = 0;
            long timestamp = 0;
            short videoLength = 0;

            try
            {
                Utils.LogToFile(3, "[INFO]", "Calling DecodeFileMetadataResponse()");
                ReverseBinaryReader reverseBinaryReader = new ReverseBinaryReader(new MemoryStream(rawData));
                commandId = reverseBinaryReader.ReadInt16();
                dataLength = reverseBinaryReader.ReadInt16();
                reverseBinaryReader.ReadBytes(2);
                timestamp = reverseBinaryReader.ReadInt64();
                timestamp = timestamp / 1000;
                reverseBinaryReader.ReadByte();
                videoLength = reverseBinaryReader.ReadInt16();
                Utils.LogToFile(3, "[INFO]", string.Format("Decoded FileMetadataResponse Command:Command Id={0}, Data Length={1},Time Stamp={2},Video Length={3}", commandId, dataLength, timestamp,videoLength));
            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", string.Format("Exception In DecodeFileMetadataResponse():{0}", ex.Message.ToString()));
                throw;
            }

            return new FileMetadataResponse {CommandId=commandId,DataLength=dataLength,Timestamp=timestamp,VideoLength=videoLength };
        }

    }
}
