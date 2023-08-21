using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeltonikaDualCamTcpNetBase.Codec
{
    class ReverseBinaryReader:BinaryReader
    {
        public ReverseBinaryReader(Stream input) : base(input)
        {
        }

        public override ushort ReadUInt16()
        {
            return ByteSwapper.Swap(base.ReadUInt16());
        }

        public override short ReadInt16()
        {
            return ByteSwapper.Swap(base.ReadInt16());
        }

        public override int ReadInt32()
        {
            return ByteSwapper.Swap(base.ReadInt32());
        }

        public override uint ReadUInt32()
        {
            return ByteSwapper.Swap(base.ReadUInt32());
        }

        public override long ReadInt64()
        {
            return ByteSwapper.Swap(base.ReadInt64());
        }

        public override ulong ReadUInt64()
        {
            return ByteSwapper.Swap(base.ReadUInt64());
        }
    }

}

