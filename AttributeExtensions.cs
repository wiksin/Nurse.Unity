﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace Nurse.Unity
{
    /// <summary>
    /// 属性扩展类
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// 类型是否能够转为指定基类
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="baseType">基类</param>
        /// <returns>是否能够转为指定基类</returns>
        public static bool As(this Type type, Type baseType)
        {
            if (type == null)
            {
                return false;
            }

            // 如果基类是泛型定义
            if (baseType.IsGenericTypeDefinition && type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                type = type.GetGenericTypeDefinition();
            }

            if (type == baseType)
            {
                return true;
            }

            if (baseType.IsAssignableFrom(type))
            {
                return true;
            }

            var rs = false;

            // 接口
            if (baseType.IsInterface)
            {
                if (type.GetInterface(baseType.FullName) != null)
                {
                    rs = true;
                }
                else if (type.GetInterfaces().Any(e => e.IsGenericType && baseType.IsGenericTypeDefinition ? e.GetGenericTypeDefinition() == baseType : e == baseType))
                {
                    rs = true;
                }
            }

            // 判断是否子类时，支持只反射加载的程序集
            if (!rs && type.Assembly.ReflectionOnly)
            {
                // 反射加载时，需要特殊处理接口
                while (!rs && type != typeof(object))
                {
                    if (type == null)
                    {
                        continue;
                    }

                    if (type.FullName == baseType.FullName && type.AssemblyQualifiedName == baseType.AssemblyQualifiedName)
                    {
                        rs = true;
                    }

                    type = type.BaseType;
                }
            }

            return rs;
        }

        /// <summary>是否泛型列表</summary>
        /// <param name="type">类型</param>
        /// <returns>是否泛型列表</returns>
        public static bool IsList(this Type type)
        {
            return type != null && type.IsGenericType && type.As(typeof(IList<>));
            //return Array.IndexOf(type.GetInterfaces(), typeof(IEnumerable)) > -1;
        }

        /// <summary>是否泛型字典</summary>
        /// <param name="type">类型</param>
        /// <returns>是否泛型字典</returns>
        public static bool IsDictionary(this Type type) => type != null && type.IsGenericType && type.As(typeof(IDictionary<,>));

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="info">info</param>
        /// <typeparam name="T">T</typeparam>
        /// <returns>属性</returns>
        public static T GetCustomAttribute<T>(this PropertyInfo info)
        {
            return (T)info.GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="info">info</param>
        /// <typeparam name="T">T</typeparam>
        /// <returns>属性</returns>
        public static T GetCustomAttribute<T>(this FieldInfo info)
        {
            return (T)info.GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="type">type</param>
        /// <typeparam name="T">T</typeparam>
        /// <returns>属性</returns>
        public static T GetCustomAttribute<T>(this Type type)
        {
            return (T)type.GetCustomAttributes(typeof(T), false).FirstOrDefault();
        }

        /// <summary>获取序列化名称</summary>
        /// <param name="pi">pi</param>
        /// <returns>名称</returns>
        public static string GetName(this PropertyInfo pi)
        {
            string name = pi.Name;
            var att = pi.GetCustomAttribute<XmlElementAttribute>();
            if (att != null && !att.ElementName.IsNullOrEmpty())
            {
                name = att.ElementName;
            }

            return name;
        }

        /// <summary>获取成员绑定的显示名，优先DisplayName，然后Description</summary>
        /// <param name="member">member</param>
        /// <returns>显示名</returns>
        public static string DisplayName(this PropertyInfo member)
        {
            var att = member.GetCustomAttribute<DisplayNameAttribute>();
            if (att != null && !att.DisplayName.IsNullOrWhiteSpace())
            {
                return att.DisplayName;
            }

            return "";
        }
        /// <summary>
        /// 获取TableAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TableAttribute GetTableAttribute<T>()
        {
            MemberInfo typeinfo = typeof(T);
            var attr = typeinfo.GetCustomAttributes(typeof(TableAttribute), false);
            if (attr.Length > 0)
            {
                return (TableAttribute)attr[0];
            }
            return null;
        }

        /// <summary>
        /// 获取类属性的显示名
        /// </summary>
        /// <param name="value">类</param>
        /// <param name="PropertyName">属性名称</param>
        /// <typeparam name="T">类类型</typeparam>
        /// <returns>字符串</returns>
        public static string DisplayName<T>(this T value, string PropertyName) where T : class
        {
            var list = value.GetType().GetProperties();
            foreach (PropertyInfo info in list)
            {
                if (info.Name.Equals(PropertyName))
                {
                    var att = info.GetCustomAttribute<DisplayNameAttribute>();
                    return att != null ? att.DisplayName : "";
                }
            }

            return "";
        }

        /// <summary>
        /// 获取类属性的显示描述
        /// </summary>
        /// <param name="value">类</param>
        /// <param name="PropertyName">属性名称</param>
        /// <typeparam name="T">类类型</typeparam>
        /// <returns>字符串</returns>
        public static string Description<T>(this T value, string PropertyName) where T : class
        {
            var list = value.GetType().GetProperties();
            foreach (PropertyInfo info in list)
            {
                if (info.Name.Equals(PropertyName))
                {
                    var att = info.GetCustomAttribute<DescriptionAttribute>();
                    return att != null ? att.Description : "";
                }
            }

            return "";
        }
        /// <summary>
        /// 获取对象中属性名
        /// </summary>
        /// <typeparam name="T">对象模型</typeparam>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string GetPropName<T>(Expression<Func<T, object>> expr)
        {
            var rtn = "";
            if (expr.Body is UnaryExpression)
            {
                rtn = ((MemberExpression)((UnaryExpression)expr.Body).Operand).Member.Name;
            }
            else if (expr.Body is MemberExpression)
            {
                rtn = ((MemberExpression)expr.Body).Member.Name;
            }
            else if (expr.Body is ParameterExpression)
            {
                rtn = ((ParameterExpression)expr.Body).Type.Name;
            }
            return rtn;
        }
        /// <summary>获取成员绑定的显示名，优先DisplayName，然后Description</summary>
        /// <param name="member">member</param>
        /// <returns>显示名</returns>
        public static string Description(this PropertyInfo member)
        {
            var att2 = member.GetCustomAttribute<DescriptionAttribute>();
            if (att2 != null && !att2.Description.IsNullOrWhiteSpace())
            {
                return att2.Description;
            }

            return "";
        }

        
    }
}