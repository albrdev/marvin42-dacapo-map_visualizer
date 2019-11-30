using System;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using Assets.Scripts.Networking;
using Assets.Scripts.Types;
using Assets.Scripts.Cryptography.CRC;
using System.Text;

public static class SerialReceiver
{
    public unsafe delegate void OnPacketReceivedEventHandler(Packet.Header* header);

    public static OnPacketReceivedEventHandler OnPacketReceivedEvent { get; set; } = null;

    private static SerialPort s_SerialPort;
    private static Thread s_ReceivingThread;
    private static bool s_IsActive = false;

    public static SerialPort SerialPort
    {
        get
        {
            lock(s_SerialPortLock)
            {
                return s_SerialPort;
            }
        }
        private set
        {
            lock(s_SerialPortLock)
            {
                s_SerialPort = value;
            }
        }
    }

    public static bool IsActive
    {
        get
        {
            lock(s_IsActiveLock)
            {
                return s_IsActive;
            }
        }
        private set
        {
            lock(s_IsActiveLock)
            {
                s_IsActive = value;
            }
        }
    }

    private static SerialPortSettings s_Settings;
    private static int m_UpdateInterval;

    private static readonly object s_SerialPortLock = new object();
    private static readonly object s_IsActiveLock = new object();
    private static readonly object s_DataLock = new object();
    private static readonly object s_StatsLock = new object();
    private static ProximityData s_Data;

    private static byte[] s_ReadBuffer = new byte[512];

    private static int m_PacketSuccessCount = 0;
    private static int m_PacketFailCount = 0;

    public static int PacketSuccessCount
    {
        get
        {
            lock(s_StatsLock)
            {
                return m_PacketSuccessCount;
            }
        }

        set
        {
            lock(s_StatsLock)
            {
                m_PacketSuccessCount = value;
            }
        }
    }

    public static int PacketFailCount
    {
        get
        {
            lock(s_StatsLock)
            {
                return m_PacketFailCount;
            }
        }

        set
        {
            lock(s_StatsLock)
            {
                m_PacketFailCount = value;
            }
        }
    }

    public static int PacketTotalCount
    {
        get { return PacketSuccessCount + PacketFailCount; }
    }

    public static float PacketSuccessRatio
    {
        get { return (float)PacketSuccessCount / (float)PacketTotalCount; }
    }

    public static float PacketFailRatio
    {
        get { return (float)PacketFailCount / (float)PacketTotalCount; }
    }

    public static ProximityData Data
    {
        get
        {
            lock(s_DataLock)
            {
                return s_Data;
            }
        }

        private set
        {
            lock(s_DataLock)
            {
                s_Data = value;
            }
        }
    }

    public static void Begin(string portName, int updateInterval = 0)
    {
        Begin(portName, SerialPortSettings.Default, updateInterval);
    }

    public static void Begin(string portName, SerialPortSettings settings, int updateInterval = 0)
    {
        s_ReceivingThread = new Thread(Poll);

        SerialPort = new SerialPort();

        s_Settings = settings;

        SerialPort.PortName = portName;
        SerialPort.BaudRate = s_Settings.BaudRate;
        SerialPort.Parity = s_Settings.Parity;
        SerialPort.DataBits = s_Settings.DataBits;
        SerialPort.StopBits = s_Settings.StopBits;
        SerialPort.Handshake = s_Settings.Handshake;
        SerialPort.RtsEnable = s_Settings.RTSEnable;
        SerialPort.DtrEnable = s_Settings.DTREnable;
        //SerialPort.ReceivedBytesThreshold = s_Settings.ReceiveThreshold;//*
        SerialPort.ReadTimeout = s_Settings.ReadTimeout;
        SerialPort.WriteTimeout = s_Settings.WriteTimeout;

        //SerialPort.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);//*

        m_UpdateInterval = updateInterval;

        IsActive = true;
        SerialPort.Open();
        s_ReceivingThread.Start();//*
    }

    public static void End()
    {
        IsActive = false;
        if(s_ReceivingThread.IsAlive)
        {
            s_ReceivingThread.Join();//*
        }

        if(SerialPort.IsOpen)
        {
            SerialPort.Close();
        }
    }

    /*private static void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
    }*/

    private static unsafe string ToHexString(byte* offset, int count)
    {
        StringBuilder sb = new StringBuilder();
        for(int i = 0; i < count; i++)
        {
            sb.AppendFormat("{0:x2}", offset[i]);
        }

        return sb.ToString();
    }

    private static unsafe bool HandlePacket(byte* offset, byte* end, out int increment)
    {
        Packet.Header* header = (Packet.Header*)offset;
        UInt16 checksum = CRC16.Generate(offset + Marshal.SizeOf(header->HeaderChecksum), Marshal.SizeOf(*header) - Marshal.SizeOf(header->HeaderChecksum));
        DebugTools.Print($"Header: HeaderChecksum={header->HeaderChecksum.ToString("X4")}, DataChecksum={header->DataChecksum.ToString("X4")}, Type={header->Type}, Size={header->Size} (Checksum={checksum.ToString("X4")}, Hex={ToHexString(offset, Marshal.SizeOf(*header))})");

        increment = -1;
        if(!Packet.VerifyHeader(header))
        {
            DebugTools.Print($"Header checksum failed: {header->HeaderChecksum.ToString("X4")} / {checksum.ToString("X4")}");
            return false;
        }

        if(offset + (Marshal.SizeOf(*header) + header->Size) > end)
        {
            increment = 0;
            return false;
        }

        checksum = CRC16.Generate(offset + Marshal.SizeOf(*header), header->Size);
        DebugTools.Print($"Data: DataChecksum={header->DataChecksum.ToString("X4")}, Size={header->Size} (Checksum={checksum.ToString("X4")}, Hex={ToHexString(offset + Marshal.SizeOf(*header), header->Size)})");

        if(!Packet.VerifyData(header))
        {
            DebugTools.Print($"Content checksum failed: {header->DataChecksum.ToString("X4")} / {checksum.ToString("X4")}");
            return false;
        }

        increment = Marshal.SizeOf(*header) + header->Size;
        OnPacketReceivedEvent?.Invoke(header);
        return true;
    }

    private static int s_ReadOffset = 0;
    private static void Poll()
    {
        while(IsActive)
        {
            try
            {
                if(SerialPort.BytesToRead < 128)
                    continue;

                int indexOffset = 0;
                int readSize = SerialPort.Read(s_ReadBuffer, s_ReadOffset, s_ReadBuffer.Length - s_ReadOffset);
                readSize += s_ReadOffset;
                s_ReadOffset = 0;

                DebugTools.Print("BUFFER BEGIN");
                int increment = 0;
                while(indexOffset + Marshal.SizeOf<Packet.Header>() <= readSize)
                {
                    unsafe
                    {
                        fixed(byte* offset = &s_ReadBuffer[indexOffset], end = &s_ReadBuffer[readSize])
                        {
                            if(!HandlePacket(offset, end, out increment))
                            {
                                break;
                            }
                        }
                    }

                    indexOffset += increment;
                }

                if(increment < 0)
                {
                    DebugTools.Print("BUFFER ERROR\n");
                    PacketFailCount++;
                    continue;
                }
                else
                {
                    PacketSuccessCount++;
                }

                s_ReadOffset = readSize - indexOffset;
                for(int i = 0; i < s_ReadOffset; i++)
                {
                    s_ReadBuffer[i] = s_ReadBuffer[indexOffset + i];
                }

                DebugTools.Print("BUFFER END");
            }
            catch(TimeoutException) { }

            Thread.Sleep(m_UpdateInterval);
        }
    }
}
