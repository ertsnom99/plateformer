using System;
using System.Reflection;

public static class ConsoleClearer
{
#if UNITY_EDITOR
    public static void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        Type type = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
#endif
}
