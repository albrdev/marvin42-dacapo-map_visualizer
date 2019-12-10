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
    private float m_DistanceThreshold = 0f;
    [SerializeField]
    private Vector3 m_DrawOrigin = Vector3.zero;
    [SerializeField]
    private float m_Scale = 1f;
    [SerializeField]
    private int m_ContentCapacity = 360 * 10;
    [SerializeField]
    private int m_UpdateInterval = 0;

    [SerializeField, ReadOnlyProperty]
    private OrientationData m_Orientation;
    [SerializeField, ReadOnlyProperty]
    private List<ProximityData>[] m_ProximityContent;

    private IList<ProximityData>[] ProximityContent
    {
        get
        {
            lock(m_ProximityContentLock)
            {
                return m_ProximityContent;
            }
        }
    }

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

                if(data.ID < 0 || data.ID >= ProximityContent.Length)
                {
                    DebugTools.Print($"Invalid sensor index ID: {data.ID}");
                    return;
                }

                int count = ProximityContent[data.ID].Count;
                int cap = m_ContentCapacity - 1;
                for(; count-- > cap;)
                {
                    ProximityContent[data.ID].RemoveAt(0);
                }

                ProximityContent[data.ID].Add(data);

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

        m_ProximityContent = new List<ProximityData>[2]
        {
            new List<ProximityData>(m_ContentCapacity),
            new List<ProximityData>(m_ContentCapacity)
        };

        unsafe
        {
            SerialReceiver.OnPacketReceivedEvent = PacketReceived;
        }
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

    private void DrawSensorData(IList<ProximityData> list, Vector3 origin)
    {
        Gizmos.color = Color.red;
        for(int j = 1; j < list.Count; j++)
        {
            if(list[j].Distance < 0f)
                continue;

            Gizmos.DrawLine((origin + list[j - 1].Position) * m_Scale, (origin + list[j].Position) * m_Scale);
        }
    }

    private void OnDrawGizmos()
    {
        if(ProximityContent == null)
            return;

        for(int i = 0; i < ProximityContent.Length; i++)
        {
            if(ProximityContent[i].Count <= 1)
                continue;

            Vector3 h = m_DrawOrigin / 2;
            ProximityData lastInput;
            DrawSensorData(ProximityContent[i], h);

            lastInput = ProximityContent[i][ProximityContent[i].Count - 1];

            Gizmos.color = Color.yellow;
            //Gizmos.DrawLine(Vector3.zero, m_Data[m_Data.Count - 1].Position.normalized * 5f);
            Gizmos.DrawLine(m_DrawOrigin, (h + lastInput.Position) * m_Scale);
            //Gizmos.color = Color.yellow.SetA(0.5f);
            //Gizmos.DrawLine((m_DrawOrigin + lastInput.Position) * m_Scale, (m_DrawOrigin + lastInput.Position.normalized) * (5f * m_Scale));
        }
    }
}
