using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

namespace UnityEngine
{
    public static class UnityTools
    {
        public static bool CursorScreenOverlap()
        {
#if UNITY_EDITOR
            Vector2 dimensions = Handles.GetMainGameViewSize();
            return (UnityEngine.Input.mousePosition.x >= 0 && UnityEngine.Input.mousePosition.y >= 0) && (UnityEngine.Input.mousePosition.x < dimensions.x && UnityEngine.Input.mousePosition.y < dimensions.y);
#else
            return (UnityEngine.Input.mousePosition.x < 0 && UnityEngine.Input.mousePosition.y < 0) && (UnityEngine.Input.mousePosition.x < Screen.width && UnityEngine.Input.mousePosition.y < Screen.height);
#endif
        }

        public static bool CursorUIOverlap()
        {
            return GUIUtility.hotControl != 0 || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
        }
    }
}
