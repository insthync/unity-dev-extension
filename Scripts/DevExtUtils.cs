using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#if !NET && !NETCOREAPP
using UnityEngine;
#endif

public static class DevExtUtils
{
    private static readonly Dictionary<Type, Dictionary<string, MethodInfo[]>> s_cacheDevExtMethods = new Dictionary<Type, Dictionary<string, MethodInfo[]>>();
    private const BindingFlags InstanceMethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags StaticMethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    /// <summary>
    /// This will calls all methods from `obj` that have attributes "DevExtMethodsAttribute("`baseMethodName`")" with any number of arguments that can be set via `args`
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="baseMethodName"></param>
    /// <param name="args"></param>
    public static void InvokeInstanceDevExtMethods<T>(this T obj, string baseMethodName, params object[] args)
    {
        if (obj == null)
            return;
        InvokeDevExtMethods(obj.GetType(), obj, baseMethodName, InstanceMethodBindingFlags, args);
    }

    public static T InvokeInstanceDevExtMethodsLoopItself<T>(this T obj, string baseMethodName, params object[] args)
    {
        if (obj == null)
            return obj;
        return (T)InvokeDevExtMethodsLoopItself(obj.GetType(), obj, baseMethodName, InstanceMethodBindingFlags, args);
    }

    /// <summary>
    /// This will calls all static methods which its type is `type` that have attributes "DevExtMethodsAttribute("`baseMethodName`")" with any number of arguments that can be set via `args`
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseMethodName"></param>
    /// <param name="args"></param>
    public static void InvokeStaticDevExtMethods(Type type, string baseMethodName, params object[] args)
    {
        InvokeDevExtMethods(type, null, baseMethodName, StaticMethodBindingFlags, args);
    }

    private static void InvokeDevExtMethods(Type type, object obj, string baseMethodName, BindingFlags bindingFlags, params object[] args)
    {
        MethodInfo[] tempMethods;
        DevExtMethodsAttribute tempAttribute;
        try
        {
            if (!s_cacheDevExtMethods.ContainsKey(type) || !s_cacheDevExtMethods[type].ContainsKey(baseMethodName))
            {
                if (!s_cacheDevExtMethods.ContainsKey(type))
                    s_cacheDevExtMethods.Add(type, new Dictionary<string, MethodInfo[]>());
                s_cacheDevExtMethods[type].Add(baseMethodName, null);
                tempMethods = type.GetMethods(bindingFlags).Where(a =>
                {
                    tempAttribute = (DevExtMethodsAttribute)a.GetCustomAttribute(typeof(DevExtMethodsAttribute), true);
                    return tempAttribute != null && tempAttribute.BaseMethodName.Equals(baseMethodName);
                }).ToArray();
                s_cacheDevExtMethods[type][baseMethodName] = tempMethods;
            }
            if (!s_cacheDevExtMethods[type].TryGetValue(baseMethodName, out tempMethods) || tempMethods == null || tempMethods.Length == 0)
                return;
            for (int tempLoopCounter = 0; tempLoopCounter < tempMethods.Length; ++tempLoopCounter)
            {
                tempMethods[tempLoopCounter].Invoke(obj, args);
            }
        }
        catch (Exception ex)
        {
#if !NET && !NETCOREAPP
            Debug.LogException(ex);
#endif
        }
    }

    private static object InvokeDevExtMethodsLoopItself(Type type, object obj, string baseMethodName, BindingFlags bindingFlags, params object[] args)
    {
        MethodInfo[] tempMethods;
        DevExtMethodsAttribute tempAttribute;
        try
        {
            if (!s_cacheDevExtMethods.ContainsKey(type) || !s_cacheDevExtMethods[type].ContainsKey(baseMethodName))
            {
                if (!s_cacheDevExtMethods.ContainsKey(type))
                    s_cacheDevExtMethods.Add(type, new Dictionary<string, MethodInfo[]>());
                s_cacheDevExtMethods[type].Add(baseMethodName, null);
                tempMethods = type.GetMethods(bindingFlags).Where(a =>
                {
                    tempAttribute = (DevExtMethodsAttribute)a.GetCustomAttribute(typeof(DevExtMethodsAttribute), true);
                    return tempAttribute != null && tempAttribute.BaseMethodName.Equals(baseMethodName);
                }).ToArray();
                s_cacheDevExtMethods[type][baseMethodName] = tempMethods;
            }
            if (!s_cacheDevExtMethods[type].TryGetValue(baseMethodName, out tempMethods) || tempMethods == null || tempMethods.Length == 0)
                return obj;
            for (int tempLoopCounter = 0; tempLoopCounter < tempMethods.Length; ++tempLoopCounter)
            {
                obj = tempMethods[tempLoopCounter].Invoke(obj, args);
            }
        }
        catch (Exception ex)
        {
#if !NET && !NETCOREAPP
            Debug.LogException(ex);
#endif
        }
        return obj;
    }
}
