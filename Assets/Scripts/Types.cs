using System;
using UnityEngine;
using Assets.Scripts.Networking;

namespace Assets.Scripts.Types
{
    [Serializable]
    public struct OrientationData
    {
        [SerializeField, ReadOnlyProperty]
        private float m_Velocity;
        [SerializeField, ReadOnlyProperty]
        private Quaternion m_Rotation;

        public float Velocity
        {
            get { return m_Velocity; }
            private set { m_Velocity = value; }
        }

        public Quaternion Rotation
        {
            get { return m_Rotation; }
            private set { m_Rotation = value; }
        }

        public Vector3 Direction
        {
            get { return (Rotation * Vector3.up); }
        }

        public static unsafe implicit operator OrientationData(CustomPackets.OrientationDataPacket packet)
        {
            return new OrientationData(packet);
        }

        public override string ToString()
        {
            return string.Format("{0}({1}, {2})", nameof(OrientationData), Velocity, Rotation);
        }

        public OrientationData(CustomPackets.OrientationDataPacket value) : this()
        {
            Velocity = System.BitConverter.ToSingle(BitConverter.GetBytes(value.Velocity), 0);

            Rotation = new Quaternion
            (
                w: System.BitConverter.ToSingle(BitConverter.GetBytes(value.Rotation.W), 0),
                x: System.BitConverter.ToSingle(BitConverter.GetBytes(value.Rotation.X), 0),
                y: System.BitConverter.ToSingle(BitConverter.GetBytes(value.Rotation.Y), 0),
                z: System.BitConverter.ToSingle(BitConverter.GetBytes(value.Rotation.Z), 0)
            );
        }

        public OrientationData(float velocity, Quaternion rotation) : this()
        {
            Velocity = velocity;
            Rotation = rotation;
        }
    }

    [Serializable]
    public struct ProximityData
    {
        [SerializeField, ReadOnlyProperty]
        private byte m_ID;
        [SerializeField, ReadOnlyProperty]
        private float m_Distance;
        [SerializeField, ReadOnlyProperty]
        private float m_Angle;

        public byte ID
        {
            get { return m_ID; }
            private set { m_ID = value; }
        }

        public float Distance
        {
            get { return m_Distance; }
            private set { m_Distance = value; }
        }

        public float Angle
        {
            get { return m_Angle; }
            private set { m_Angle = value; }
        }

        public Quaternion Rotation
        {
            get { return Quaternion.AngleAxis(Angle, Vector3.back); }
        }

        public Vector3 Direction
        {
            get { return (Rotation * Vector3.up); }
        }

        public Vector3 Position
        {
            get { return Direction * Distance; }
        }

        public static unsafe implicit operator ProximityData(CustomPackets.ProximityDataPacket packet)
        {
            return new ProximityData(packet);
        }

        public override string ToString()
        {
            return string.Format("{0}({1}, {2}, {3})", nameof(ProximityData), ID, Distance, Angle);
        }

        public ProximityData(CustomPackets.ProximityDataPacket value) : this()
        {
            ID = value.ID;
            Distance = System.BitConverter.ToSingle(BitConverter.GetBytes(value.Distance), 0);
            Angle = System.BitConverter.ToSingle(BitConverter.GetBytes(value.Angle), 0);
        }

        public ProximityData(byte id, float distance, float angle) : this()
        {
            ID = id;
            Distance = distance;
            Angle = angle;
        }
    }
}
