﻿using System;
using System.Runtime.InteropServices;

namespace Assets.Scripts.Networking
{
    public static class CustomPackets
    {
        public enum Type : byte
        {
            Status = Packet.Type.Max,
            MotorRun,
            MotorStop,
            MotorJSData,
            OrientationData,
            ProximityData
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct QuaternionData
        {
            public UInt32 W { get; }
            public UInt32 X { get; }
            public UInt32 Y { get; }
            public UInt32 Z { get; }

            public unsafe QuaternionData(UInt32 w, UInt32 x, UInt32 y, UInt32 z) : this()
            {
                W = w;
                X = x;
                X = y;
                X = z;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ProximityRawData
        {
            public UInt32 Distance { get; }
            public UInt32 Angle { get; }

            public unsafe ProximityRawData(UInt32 distance, UInt32 angle) : this()
            {
                Distance = distance;
                Angle = angle;

                fixed(ProximityRawData* pointer = &this)
                {
                    Packet.GenerateHeader((Packet.Header*)pointer, (UInt16)Marshal.SizeOf(this), (byte)CustomPackets.Type.ProximityData);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct OrientationDataPacket
        {
            public Packet.Header Header { get; private set; }

            public UInt32 Velocity { get; }
            public QuaternionData Rotation { get; }

            public unsafe OrientationDataPacket(UInt32 velocity, QuaternionData rotation) : this()
            {
                Velocity = velocity;
                Rotation = rotation;

                fixed(OrientationDataPacket* pointer = &this)
                {
                    Packet.GenerateHeader((Packet.Header*)pointer, (UInt16)Marshal.SizeOf(this), (byte)CustomPackets.Type.OrientationData);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ProximityDataPacket
        {
            public Packet.Header Header { get; private set; }

            public UInt32 Distance { get; }
            public UInt32 Angle { get; }

            public unsafe ProximityDataPacket(UInt32 distance, UInt32 angle) : this()
            {
                Distance = distance;
                Angle = angle;

                fixed(ProximityDataPacket* pointer = &this)
                {
                    Packet.GenerateHeader((Packet.Header*)pointer, (UInt16)Marshal.SizeOf(this), (byte)CustomPackets.Type.ProximityData);
                }
            }
        }
    }
}
