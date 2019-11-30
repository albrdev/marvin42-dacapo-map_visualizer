using System;
using UnityEngine;
using UnityEngine.UI;

public class OrientationInfoPanel : InfoPanelBase
{
    [SerializeField]
    private Text m_VelocityValue;
    [SerializeField]
    private Text m_DirectionValue;
    [SerializeField]
    private Text m_RotationValue;

    private void Awake()
    {
        m_VelocityValue.text = "0.00";
        m_DirectionValue.text = Vector3.zero.ToString();
        m_RotationValue.text = Quaternion.identity.ToString();
    }

    protected override void UpdateValues()
    {
        m_VelocityValue.text = Main.Instance.Orientation.Velocity.ToString("n2");
        m_DirectionValue.text = Main.Instance.Orientation.Direction.ToString();
        m_RotationValue.text = Main.Instance.Orientation.Rotation.ToString();
    }
}
