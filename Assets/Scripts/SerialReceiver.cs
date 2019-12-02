using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using Assets.Scripts.Networking;
using Assets.Scripts.Cryptography.CRC;

public static class SerialReceiver
{
    public unsafe delegate void OnPacketReceivedEventHandler(Packet.Header* header);

    public static OnPacketReceivedEventHandler OnPacketReceivedEvent { get; set; } = null;

    private static SerialPort s_SerialPort;
    private static Thread s_ReceivingThread;
    private static bool s_IsActive = false;

    private static byte[] s_ReadBuffer = new byte[512];
    private static int s_PacketSuccessCount = 0;
    private static int s_PacketFailCount = 0;

    private static SerialPortSettings s_Settings;
    private static int s_UpdateInterval;

    private static readonly object s_SerialPortLock = new object();
    private static readonly object s_SettingsLock = new object();
    private static readonly object s_UpdateIntervalLock = new object();
    private static readonly object s_IsActiveLock = new object();
    private static readonly object s_PacketSuccessCountLock = new object();
    private static readonly object s_PacketFailCountLock = new object();

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

    public static SerialPortSettings Settings
    {
        get
        {
            lock(s_SettingsLock)
            {
                return s_Settings;
            }
        }
        private set
        {
            lock(s_SettingsLock)
            {
                s_Settings = value;
            }
        }
    }

    public static int UpdateInterval
    {
        get
        {
            lock(s_UpdateIntervalLock)
            {
                return s_UpdateInterval;
            }
        }

        set
        {
            lock(s_UpdateIntervalLock)
            {
                s_UpdateInterval = value;
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
                if(value == s_IsActive)
                    return;

                s_IsActive = value;
                if(s_IsActive)
                {
                    s_ReceivingThread.Start();//*
                }
                else
                {
                    if(s_ReceivingThread.IsAlive)
                    {
                        s_ReceivingThread.Join();//*
                    }
                }
            }
        }
    }

    public static int PacketSuccessCount
    {
        get
        {
            lock(s_PacketSuccessCountLock)
            {
                return s_PacketSuccessCount;
            }
        }

        set
        {
            lock(s_PacketSuccessCountLock)
            {
                s_PacketSuccessCount = value;
            }
        }
    }

    public static int PacketFailCount
    {
        get
        {
            lock(s_PacketFailCountLock)
            {
                return s_PacketFailCount;
            }
        }

        set
        {
            lock(s_PacketFailCountLock)
            {
                s_PacketFailCount = value;
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

    public static void Begin(string portName, int updateInterval = 0)
    {
        Begin(portName, SerialPortSettings.Default, updateInterval);
    }

    public static void Begin(string portName, SerialPortSettings settings, int updateInterval = 0)
    {
        if(SerialPort.IsOpen)
            return;

        s_ReceivingThread = new Thread(Poll);

        SerialPort = new SerialPort();

        Settings = settings;

        SerialPort.PortName = portName;
        SerialPort.BaudRate = Settings.BaudRate;
        SerialPort.Parity = Settings.Parity;
        SerialPort.DataBits = Settings.DataBits;
        SerialPort.StopBits = Settings.StopBits;
        SerialPort.Handshake = Settings.Handshake;
        SerialPort.RtsEnable = Settings.RTSEnable;
        SerialPort.DtrEnable = Settings.DTREnable;
        //SerialPort.ReceivedBytesThreshold = Settings.ReceiveThreshold;//*
        SerialPort.ReadTimeout = Settings.ReadTimeout;
        SerialPort.WriteTimeout = Settings.WriteTimeout;

        //SerialPort.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);//*

        s_UpdateInterval = updateInterval;

        SerialPort.Open();
        IsActive = true;
    }

    public static void End()
    {
        IsActive = false;

        if(SerialPort.IsOpen)
        {
            SerialPort.Close();
        }
    }

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
        increment = -1;

        UInt16 checksum = CRC16.Generate(offset + Marshal.SizeOf(header->HeaderChecksum), Marshal.SizeOf(*header) - Marshal.SizeOf(header->HeaderChecksum));
        DebugTools.Print($"Header: HeaderChecksum={header->HeaderChecksum.ToString("X4")}, DataChecksum={header->DataChecksum.ToString("X4")}, Type={header->Type}, Size={header->Size} (Checksum={checksum.ToString("X4")}, Hex={ToHexString(offset, Marshal.SizeOf(*header))})");

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

    /*private static void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
    }*/

    private static int s_ReadOffset = 0;
    private static void Poll()
    {
        while(IsActive)
        {
            try
            {
                if(SerialPort.BytesToRead <= 0 || SerialPort.BytesToRead < Settings.ReceivedBytesThreshold/*SerialPort.ReceivedBytesThreshold*/)
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

            Thread.Sleep(s_UpdateInterval);
        }
    }
}
