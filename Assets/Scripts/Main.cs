using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Types;
using Assets.Scripts.Networking;
using Assets.Scripts.ExtensionClasses;

public class Main : MonoBehaviourSingleton
{
    public static new Main Instance
    {
        get { return GetInstance<Main>(); }
    }

    [SerializeField]
    private string m_COMPortName = string.Empty;
    [SerializeField]
    private SerialPortSettings m_Settings = SerialPortSettings.Default; // Baud rates: 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200
    [SerializeField]
    private int m_UpdateInterval = 0;

    [SerializeField]
    private float m_Scale = 1f;

    private const int ContentCapacity = 360 * 10;

    [SerializeField, ReadOnlyProperty]
    private OrientationData m_Orientation;
    [SerializeField, ReadOnlyProperty]
    private List<ProximityData> m_ProximityContent = new List<ProximityData>(ContentCapacity);

    public OrientationData Orientation
    {
        get
        {
            lock(m_OrientationLock)
            {
                return m_Orientation;
            }
        }

        set
        {
            lock(m_OrientationLock)
            {
                m_Orientation = value;
            }
        }
    }

    private readonly object m_OrientationLock = new object();
    private readonly object m_ProximityContentLock = new object();
    private unsafe void PacketReceived(Packet.Header* header)
    {
        switch(header->Type)
        {
            case (byte)CustomPackets.Type.OrientationData:
            {
                Orientation = *((CustomPackets.OrientationDataPacket*)header);

                DebugTools.Print($"DATA: {Orientation}");
                break;
            }

            case (byte)CustomPackets.Type.ProximityData:
            {
                ProximityData data = *((CustomPackets.ProximityDataPacket*)header);
                DebugTools.Print($"DATA: {data}");

                int count;
                lock(m_ProximityContentLock)
                {
                    count = m_ProximityContent.Count;
                }

                int cap = ContentCapacity - 1;
                for(; count-- > cap;)
                {
                    lock(m_ProximityContentLock)
                    {
                        m_ProximityContent.RemoveAt(0);
                    }
                }

                lock(m_ProximityContentLock)
                {
                    m_ProximityContent.Add(data);
                }

                break;
            }

            default:
            {
                DebugTools.Print($"Unknown packet type: {header->Type}");
                break;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        unsafe
        {
            SerialReceiver.OnPacketReceivedEvent = PacketReceived;
        }

        //SerialReceiver.OnUpdate += RevolutionUpdate;
        //SerialReceiver.OnUpdate += OscillationUpdate;
    }

    private void Start()
    {
        SerialReceiver.Begin(m_COMPortName, m_Settings, m_UpdateInterval);
    }

    protected override void OnDestroy()
    {
        SerialReceiver.End();
        base.OnDestroy();
    }

    ProximityData m_Origin;
    Vector3 m_LastCrossProduct;
    bool m_InitialCall = true;
    private void RevolutionUpdate(ProximityData data)
    {
        if(m_InitialCall)
        {
            m_Origin = data;
            m_InitialCall = false;
        }

        data = new ProximityData(data.Distance, data.Angle - 90f);
        Vector3 crossProduct = MathTools.CrossProduct(data.Direction, m_Origin.Direction);
        lock(m_ProximityContent)
        {
            if((m_LastCrossProduct.z < 0f && crossProduct.z >= 0f) || (m_LastCrossProduct.z > 0f && crossProduct.z <= 0f))
            {
                m_ProximityContent.Clear();
            }

            m_ProximityContent.Add(data);
        }

        m_LastCrossProduct = crossProduct;
    }

    float m_LastAngle = 0f;
    int m_LastDirection = 0;
    private void OscillationUpdate(ProximityData data)
    {
        data = new ProximityData(data.Distance, data.Angle - 90f);
        int direction = MathTools.Sign(data.Angle - m_LastAngle);

        lock(m_ProximityContent)
        {
            if(direction != m_LastDirection)
            {
                m_ProximityContent.Clear();
                m_LastDirection = direction;
            }

            m_ProximityContent.Add(data);
        }

        m_LastAngle = data.Angle;
    }

    private void OnDrawGizmos()
    {
        if(m_ProximityContent.Count <= 1)
            return;

        ProximityData lastInput;
        lock(m_ProximityContent)
        {
            Gizmos.color = Color.red;
            for(int i = 1; i < m_ProximityContent.Count; i++)
            {
                Gizmos.DrawLine(m_ProximityContent[i - 1].Position * m_Scale, m_ProximityContent[i].Position * m_Scale);
            }

            lastInput = m_ProximityContent[m_ProximityContent.Count - 1];
        }

        Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(Vector3.zero, m_Data[m_Data.Count - 1].Position.normalized * 5f);
        Gizmos.DrawLine(Vector3.zero, lastInput.Position * m_Scale);
        Gizmos.color = Color.yellow.SetA(0.5f);
        Gizmos.DrawLine(lastInput.Position * m_Scale, lastInput.Position.normalized * (5f * m_Scale));
    }
}
