using System;
using System.Runtime.InteropServices;
using Assets.Scripts.Cryptography.CRC;

namespace Assets.Scripts.Networking
{
    public static class Packet
    {
        public enum Type : byte
        {
            False = 0,
            True = 1,
            Max = 2
        }

        public static unsafe bool VerifyHeader(Header* pkt)
        {
            return CRC16.Generate((byte*)pkt + Marshal.SizeOf(pkt->HeaderChecksum), Marshal.SizeOf(*pkt) - Marshal.SizeOf(pkt->HeaderChecksum)) == pkt->HeaderChecksum;
        }

        public static unsafe bool VerifyData(void* pkt)
        {
            Header* hdr = (Header*)pkt;
            return CRC16.Generate((byte*)pkt + Marshal.SizeOf(*hdr), hdr->Size) == hdr->DataChecksum;
        }

        public static unsafe void GenerateHeader(Header* pkt, UInt16 size, byte type)
        {
            pkt->Type = type;
            pkt->Size = (UInt16)(size - Marshal.SizeOf(*pkt));
            pkt->DataChecksum = CRC16.Generate((byte*)pkt + Marshal.SizeOf(*pkt), pkt->Size);
            pkt->HeaderChecksum = CRC16.Generate((byte*)pkt + Marshal.SizeOf(pkt->HeaderChecksum), Marshal.SizeOf(*pkt) - Marshal.SizeOf(pkt->HeaderChecksum));
        }

        public static unsafe void GenerateBasic(void* pkt, byte type)
        {
            Packet.GenerateHeader((Packet.Header*)pkt, (UInt16)Marshal.SizeOf<Packet.Header>(), type);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public UInt16 HeaderChecksum { get; internal set; }
            public UInt16 DataChecksum { get; internal set; }
            public byte Type { get; internal set; }
            public UInt16 Size { get; internal set; }

            public Header(UInt16 headerChecksum, UInt16 dataChecksum, byte type, UInt16 size) : this()
            {
                HeaderChecksum = headerChecksum;
                DataChecksum = dataChecksum;
                Type = type;
                Size = size;
            }
        }
    }
}
