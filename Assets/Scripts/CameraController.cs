using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    protected float m_MinZoom = 1f;
    [SerializeField]
    protected float m_MaxZoom = 10f;
    [SerializeField]
    protected float m_ZoomSensitivity = 1f;
    [SerializeField]
    protected string m_ZoomAxisName = string.Empty;
    [SerializeField]
    protected bool m_InvertZoomAxis = true;

    [SerializeField]
    protected Vector2 m_PanSensitivity = new Vector2(1f, 1f);
    [SerializeField]
    protected string m_PanButtonName = string.Empty;
    [SerializeField]
    protected string m_CursorHorizontalAxisName = string.Empty;
    [SerializeField]
    protected string m_CursorVerticalAxisName = string.Empty;
    [SerializeField]
    protected bool m_InvertHorizontalPan = true;
    [SerializeField]
    protected bool m_InvertVerticalPan = true;

    [SerializeField]
    protected string m_ResetPanButtonName = string.Empty;

    [SerializeField]
    protected string m_ResetZoomButtonName = string.Empty;

    protected Camera m_Camera;
    protected Vector3 m_InitialPosition;
    protected float m_InitialZoom;

    protected virtual void Awake()
    {
        m_Camera = this.GetComponent<Camera>();
        m_InitialPosition = m_Camera.transform.position;
        m_InitialZoom = m_Camera.orthographicSize;
    }

    protected virtual void Update()
    {
        if(!UnityTools.CursorScreenOverlap() || UnityTools.CursorUIOverlap())
            return;

        if(m_PanButtonName != string.Empty && Input.GetButton(m_PanButtonName))
        {
            Vector3 position = Vector2.zero;

            if(m_CursorHorizontalAxisName != string.Empty)
            {
                position.x = Input.GetAxis(m_CursorHorizontalAxisName) * (m_InvertHorizontalPan ? -1f : 1f) * m_PanSensitivity.x;
            }

            if(m_CursorVerticalAxisName != string.Empty)
            {
                position.y = Input.GetAxis(m_CursorVerticalAxisName) * (m_InvertVerticalPan ? -1f : 1f) * m_PanSensitivity.y;
            }

            m_Camera.transform.position += position;
        }

        if(m_ZoomAxisName != string.Empty)
        {
            float scrollValue = Input.GetAxisRaw(m_ZoomAxisName) * (m_InvertZoomAxis ? -1f : 1f);
            if(scrollValue != 0f)
            {
                scrollValue = Mathf.Clamp(m_Camera.orthographicSize + (scrollValue * m_ZoomSensitivity), m_MinZoom, m_MaxZoom);
                m_Camera.orthographicSize = scrollValue;
            }
        }

        if(m_ResetPanButtonName != string.Empty && Input.GetButtonDown(m_ResetPanButtonName))
        {
            m_Camera.transform.position = m_InitialPosition;
        }

        if(m_ResetZoomButtonName != string.Empty && Input.GetButtonDown(m_ResetZoomButtonName))
        {
            m_Camera.orthographicSize = m_InitialZoom;
        }
    }
}
