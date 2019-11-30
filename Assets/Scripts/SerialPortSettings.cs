using System;
using System.IO.Ports;
using UnityEngine;

[Serializable]
public struct SerialPortSettings
{
    public static readonly SerialPortSettings Default = new SerialPortSettings
    {
        BaudRate = 9600,
        Parity = Parity.None,
        DataBits = 8,
        StopBits = StopBits.One,
        Handshake = Handshake.None,
        RTSEnable = false,
        DTREnable = false,
        ReceivedBytesThreshold = 1,
        ReadTimeout = SerialPort.InfiniteTimeout,
        WriteTimeout = SerialPort.InfiniteTimeout
    };

    [SerializeField]
    private int m_BaudRate;
    [SerializeField]
    private Parity m_Parity;
    [SerializeField]
    private int m_DataBits;
    [SerializeField]
    private StopBits m_StopBits;
    [SerializeField]
    private Handshake m_Handshake;
    [SerializeField]
    private bool m_RTSEnable;
    [SerializeField]
    private bool m_DTREnable;
    [SerializeField]
    private int m_ReceivedBytesThreshold;
    [SerializeField]
    private int m_ReadTimeout;
    [SerializeField]
    private int m_WriteTimeout;

    public int BaudRate
    {
        get { return m_BaudRate; }
        set { m_BaudRate = value; }
    }

    public Parity Parity
    {
        get { return m_Parity; }
        set { m_Parity = value; }
    }

    public int DataBits
    {
        get { return m_DataBits; }
        set { m_DataBits = value; }
    }

    public StopBits StopBits
    {
        get { return m_StopBits; }
        set { m_StopBits = value; }
    }

    public Handshake Handshake
    {
        get { return m_Handshake; }
        set { m_Handshake = value; }
    }

    public bool RTSEnable
    {
        get { return m_RTSEnable; }
        set { m_RTSEnable = value; }
    }

    public bool DTREnable
    {
        get { return m_DTREnable; }
        set { m_DTREnable = value; }
    }

    public int ReceivedBytesThreshold
    {
        get { return m_ReceivedBytesThreshold; }
        set { m_ReceivedBytesThreshold = value; }
    }

    public int ReadTimeout
    {
        get { return m_ReadTimeout; }
        set { m_ReadTimeout = value; }
    }

    public int WriteTimeout
    {
        get { return m_WriteTimeout; }
        set { m_WriteTimeout = value; }
    }
}
