using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if !NET && !NETCOREAPP
using UnityEngine;
#endif

namespace Insthync.DevExtension
{
    public static class DevExtUtils
    {
        private static readonly Dictionary<string, Dictionary<string, MethodInfo[]>> s_cacheDevExtMethods = new Dictionary<string, Dictionary<string, MethodInfo[]>>();
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

        private static bool TryGetDevExtMethods(Type type, string baseMethodName, BindingFlags bindingFlags, out MethodInfo[] methods)
        {
            methods = null;
            DevExtMethodsAttribute tempAttribute;
            string typeName = type.FullName;
            if (!s_cacheDevExtMethods.ContainsKey(typeName) || !s_cacheDevExtMethods[typeName].ContainsKey(baseMethodName))
            {
                if (!s_cacheDevExtMethods.ContainsKey(typeName))
                    s_cacheDevExtMethods.Add(typeName, new Dictionary<string, MethodInfo[]>());
                s_cacheDevExtMethods[typeName].Add(baseMethodName, null);
                methods = type.GetMethods(bindingFlags).Where(a =>
                {
                    tempAttribute = (DevExtMethodsAttribute)a.GetCustomAttribute(typeof(DevExtMethodsAttribute), true);
                    return tempAttribute != null && tempAttribute.BaseMethodName.Equals(baseMethodName);
                }).ToArray();
                s_cacheDevExtMethods[typeName][baseMethodName] = methods;
            }
            methods = s_cacheDevExtMethods[typeName][baseMethodName];
            return methods != null && methods.Length > 0;
        }

        private static void InvokeDevExtMethods(Type type, object obj, string baseMethodName, BindingFlags bindingFlags, params object[] args)
        {
            try
            {
                if (!TryGetDevExtMethods(type, baseMethodName, bindingFlags, out MethodInfo[] methods))
                    return;

                for (int tempLoopCounter = 0; tempLoopCounter < methods.Length; ++tempLoopCounter)
                {
                    methods[tempLoopCounter].Invoke(obj, args);
                }
            }
            catch (Exception ex)
            {
#if NET || NETCOREAPP
                Console.WriteLine(ex.ToString());
#else
                Debug.LogException(ex);
#endif
            }
        }

        private static object InvokeDevExtMethodsLoopItself(Type type, object obj, string baseMethodName, BindingFlags bindingFlags, params object[] args)
        {
            try
            {
                if (!TryGetDevExtMethods(type, baseMethodName, bindingFlags, out MethodInfo[] methods))
                    return obj;

                for (int tempLoopCounter = 0; tempLoopCounter < methods.Length; ++tempLoopCounter)
                {
                    obj = methods[tempLoopCounter].Invoke(obj, args);
                }
            }
            catch (Exception ex)
            {
#if NET || NETCOREAPP
                Console.WriteLine(ex.ToString());
#else
                Debug.LogException(ex);
#endif
            }
            return obj;
        }
    }
}
