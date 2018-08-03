using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class DevExtUtils
{
    private static Dictionary<string, List<MethodInfo>> cacheDevExtMethods = new Dictionary<string, List<MethodInfo>>();
    // Optimizing garbage collection
    private static string tempKey;
    private static List<MethodInfo> tempMethods;
    private static DevExtMethodsAttribute tempAttribute;
    /// <summary>
    /// This will calls all methods from `obj` that have attributes "DevExtMethodsAttribute("`baseMethodName`")" with any number of arguments that can be set via `args`
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="baseMethodName"></param>
    /// <param name="args"></param>
    public static void InvokeClassDevExtMethods<T>(this T obj, string baseMethodName, params object[] args) where T : class
    {
        InvokeDevExtMethods(obj.GetType(), obj, baseMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, args);
    }

    /// <summary>
    /// This will calls all static methods which its type is `type` that have attributes "DevExtMethodsAttribute("`baseMethodName`")" with any number of arguments that can be set via `args`
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseMethodName"></param>
    /// <param name="args"></param>
    public static void InvokeStaticDevExtMethods(Type type, string baseMethodName, params object[] args)
    {
        InvokeDevExtMethods(type, null, baseMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, args);
    }
    
    private static void InvokeDevExtMethods(Type type, object obj, string baseMethodName, BindingFlags bindingFlags, params object[] args)
    {
        tempKey = new StringBuilder().Append(type.Name).Append('_').Append(baseMethodName).ToString();
        tempMethods = null;
        if (!cacheDevExtMethods.TryGetValue(tempKey, out tempMethods))
        {
            tempMethods = type.GetMethods(bindingFlags).Where(a =>
            {
                tempAttribute = (DevExtMethodsAttribute)a.GetCustomAttribute(typeof(DevExtMethodsAttribute), true);
                return tempAttribute != null && tempAttribute.BaseMethodName.Equals(baseMethodName);
            }).ToList();
            cacheDevExtMethods[tempKey] = tempMethods;
        }
        if (tempMethods == null || tempMethods.Count == 0) return;
        foreach (var method in tempMethods)
        {
            try
            {
                method.Invoke(obj, args);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
