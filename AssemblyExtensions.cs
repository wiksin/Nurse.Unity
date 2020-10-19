using System;
using System.Reflection;

namespace Nurse.Unity
{
    /// <summary>
    /// 反射处理类
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// 根据类名称获取当前进程的类型申明
        /// </summary>
        /// <param name="typename">类名</param>
        /// <returns>类型申明</returns>
        public static Type GetCurrentType(string typename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetType(typename);
        }

        /// <summary>
        /// 根据类名称获取DLL类型申明
        /// </summary>
        /// <param name="dll">DLL名称</param>
        /// <param name="typename">类名</param>
        /// <returns>类型申明</returns>
        public static Type GetDllType(string dll, string typename)
        {
            Assembly assembly = Assembly.LoadFile(dll);
            return assembly.GetType(typename);
        }

        /// <summary>
        /// 获取当前进程的类型申明数组
        /// </summary>
        /// <returns>类型申明数组</returns>
        public static Type[] GetCurrentTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes();
        }

        /// <summary>
        /// 获取DLL类型申明数组
        /// </summary>
        /// <param name="dll">DLL名称</param>
        /// <returns>类型申明数组</returns>
        public static Type[] GetDllTypes(string dll)
        {
            Assembly assembly = Assembly.LoadFile(dll);
            return assembly.GetExportedTypes();
        }

        /// <summary>
        /// 根据类型申明创建对象
        /// </summary>
        /// <param name="type">类型申明</param>
        /// <returns>对象</returns>
        public static object CreateInstance(this Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}