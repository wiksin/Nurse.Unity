using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Nurse.Unity
{
    /// <summary>
    /// 属性帮助类
    /// </summary>
    public static class PropHelper
    {
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
                return  (TableAttribute)attr[0];
            }
            return null;
        }

       




        /// <summary>
        /// 获取模型属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <param name="isRequired">指定是否搜索该成员的继承链以查找这些特性</param>
        /// <returns></returns>
        private static T GetAttribute<T>(this MemberInfo member, bool isRequired=false)
        where T : Attribute
        {
            var attribute = member.GetCustomAttributes(typeof(T), false).SingleOrDefault();

            if (attribute == null && isRequired)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "必须在成员{1}上定义{0}属性",
                        typeof(T).Name,
                        member.Name));
            }

            return (T)attribute;
        }
        /// <summary>
        /// 获取DisplayName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "找不到属性引用表达式.",
                    "属性表达式");
            }

            var attr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        /// <summary>
        /// 获取Description
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetPropertyDescription<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "找不到属性引用表达式.",
                    "属性表达式");
            }

            var attr = memberInfo.GetAttribute<DescriptionAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.Description;
        }

        /// <summary>
        /// 获取属性信息
        /// </summary>
        /// <param name="propertyExpression">属性表达式</param>
        /// <returns></returns>
        public  static MemberInfo GetPropertyInformation(Expression propertyExpression)
        {
            MemberExpression memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null)
            {
                UnaryExpression unaryExpr = propertyExpression as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                {
                    memberExpr = unaryExpr.Operand as MemberExpression;
                }
            }
            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
            {
                return memberExpr.Member;
            }

            return null;
        }

       


    }
}
