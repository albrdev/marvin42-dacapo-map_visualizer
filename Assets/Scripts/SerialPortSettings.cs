using System;
using System.IO.Ports;

[Serializable]
public struct SerialPortSettings
{
    public static readonly SerialPortSettings Default = new SerialPortSettings
    {
        BaudRate = 9600,
        Parity = Parity.None,
        DataBitSize = 8,
        StopBitCount = StopBits.One,
        HandshakeMode = Handshake.None,
        ReadTimeout = 500,
        WriteTimeout = 500,
        LoopDelay = 100
    };

    public int BaudRate { get; set; }
    public Parity Parity { get; set; }
    public int DataBitSize { get; set; }
    public StopBits StopBitCount { get; set; }
    public Handshake HandshakeMode { get; set; }
    public int ReadTimeout { get; set; }
    public int WriteTimeout { get; set; }
    public int LoopDelay { get; set; }
}
