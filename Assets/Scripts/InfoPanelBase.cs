using System;
using System.Collections;
using UnityEngine;

public abstract class InfoPanelBase : MonoBehaviour
{
    [SerializeField]
    private float m_UpdateInterval = 1f;

    protected virtual void Start()
    {
        StartCoroutine(UpdateValuesRoutine());
    }

    private IEnumerator UpdateValuesRoutine()
    {
        while(true)
        {
            UpdateValues();

            yield return new WaitForSeconds(m_UpdateInterval);
        }
    }

    protected abstract void UpdateValues();
}
