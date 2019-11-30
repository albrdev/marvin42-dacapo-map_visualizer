using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ExtensionClasses;

public class PacketStatsInfoPanel : InfoPanelBase
{
    [SerializeField]
    private Text m_TotalCountValue;
    [SerializeField]
    private Text m_SuccessCountValue;
    [SerializeField]
    private Text m_FailCountValue;
    [SerializeField]
    private Text m_SuccessRatioValue;
    [SerializeField]
    private Text m_FailRatioValue;

    private void Awake()
    {
        m_TotalCountValue.text = "0";
        m_SuccessCountValue.text = "0";
        m_FailCountValue.text = "0";
        m_SuccessRatioValue.text = "0.00%";
        m_FailRatioValue.text = "0.00%";
    }

    protected override void UpdateValues()
    {
        m_TotalCountValue.text = SerialReceiver.PacketTotalCount.ToString();
        m_SuccessCountValue.text = SerialReceiver.PacketSuccessCount.ToString();
        m_FailCountValue.text = SerialReceiver.PacketFailCount.ToString();
        m_SuccessRatioValue.text = SerialReceiver.PacketSuccessRatio.ToPercentage().ToString("n2") + "%";
        m_FailRatioValue.text = SerialReceiver.PacketSuccessRatio.ToPercentage().ToString("n2") + "%";
    }
}
