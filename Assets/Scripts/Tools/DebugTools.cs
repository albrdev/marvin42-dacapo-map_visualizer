using System;
using System.Diagnostics;
using System.Reflection;

public static class DebugTools
{
    [Conditional("DEBUG")]
    public static void Print(string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogFormat(InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void Print(object pArg)
    {
        UnityEngine.Debug.Log(pArg);
    }

    [Conditional("DEBUG")]
    public static void Print(UnityEngine.Object pObject, string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogFormat(pObject, InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void Print(UnityEngine.Object pObject, object pArg)
    {
        UnityEngine.Debug.Log(pArg, pObject);
    }

    [Conditional("DEBUG")]
    public static void PrintWarning(string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogWarningFormat(InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void PrintWarning(UnityEngine.Object pObject, string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogWarningFormat(pObject, InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void PrintError(string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogErrorFormat(InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void PrintError(UnityEngine.Object pObject, string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogErrorFormat(pObject, InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void PrintAssertion(string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogAssertionFormat(InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void PrintAssertion(UnityEngine.Object pObject, string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.LogAssertionFormat(pObject, InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void PrintException(System.Exception pException)
    {
        UnityEngine.Debug.LogException(pException);
    }

    [Conditional("DEBUG")]
    public static void PrintException(UnityEngine.Object pObject, System.Exception pException)
    {
        UnityEngine.Debug.LogException(pException, pObject);
    }

    [Conditional("DEBUG")]
    public static void Assertion(bool pCondition, string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.AssertFormat(pCondition, InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void Assertion(bool pCondition, UnityEngine.Object pObject, string pFormat, params object[] pArgs)
    {
        UnityEngine.Debug.AssertFormat(pCondition, pObject, InitialText + pFormat, pArgs);
    }

    [Conditional("DEBUG")]
    public static void Exception(string pFormat, params object[] pArgs)
    {
        throw new UnityEngine.UnityException(InitialText + string.Format(pFormat, pArgs));
    }

    [Conditional("DEBUG")]
    public static void Exception(bool pCondition, string pFormat, params object[] pArgs)
    {
        if(pCondition)
        {
            throw new UnityEngine.UnityException(InitialText + string.Format(pFormat, pArgs));
        }
    }

    [Conditional("DEBUG")]
    public static void Clear()
    {
#if UNITY_EDITOR
        System.Type type = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        MethodInfo method = type.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        method.Invoke(null, null);
#endif

        UnityEngine.Debug.ClearDeveloperConsole();
    }

    private static string InitialMessage
    {
        get {
            StackFrame stackFrame = new StackFrame(2);
            return string.Format("{0}:{1} => {2}.{3}: ", stackFrame.GetFileName(), stackFrame.GetFileLineNumber(), stackFrame.GetMethod().ReflectedType.Name, stackFrame.GetMethod().Name);
        }
    }

    private static string InitialText
    {
        get { return InitialMessage + ": "; }
    }
}
