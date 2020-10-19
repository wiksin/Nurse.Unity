
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Nurse.Unity
{
    /// <summary>
    /// Sql语句生成器
    /// </summary>
    public static class SqlBuilder
    {
        /// <summary>
        /// 获取类型中的属性名称
        /// </summary>
        /// <returns></returns>
        public static List<string> SelectFieldsNames<T>()
        {
            var type = typeof(T);
            List<string> names = new List<string>();
            var propertys = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in propertys)
            {
                var name = property.Name;
                names.Add(name);
            }
            return names;
        }
        /// <summary>
        /// 通过属性获取模型映射表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static string InitScheamAndTable<T>()
        {
            var attr = AttributeExtensions.GetTableAttribute<T>();
            if (attr==null) return "  [dbo]." + $"[{typeof(T).Name}]  ";
            else
            {
                var scheam = "  [dbo].";
                var name = $"[{typeof(T).Name}]   ";
                if (!attr.Schema.IsNullOrWhiteSpace())
                {
                    scheam = $"  [{attr.Schema}].";
                }
                if (!attr.Name.IsNullOrWhiteSpace())
                {
                    name = $"[{attr.Name}]   ";
                }
                
                return $" {scheam + name} ";
            }
        }



        /// <summary>
        /// 变更字段筛选
        /// </summary>
        /// <typeparam name="T">模型类</typeparam>
        /// <param name="model">传入模型用于判定值是否为空</param>
        /// <param name="keyName">主键字段</param>
        /// <param name="exceptNull">没有值得字段自动排除</param>
        /// <returns></returns>
        public static List<string> UpdateFidel<T>(T model,string keyName,bool exceptNull=true)
        {
            var type = model.GetType();
            List<string> names = new List<string>();
            var propertys = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            names.Add(keyName);
            foreach (var property in propertys)
            {
                if (exceptNull)
                {
                    if (property.GetValue(model, null) != null || !property.GetValue(model, null).ToString().IsNullOrWhiteSpace())
                    {
                        var name = property.Name;
                        names.Add(name);
                    }
                }
                else
                {
                    var name = property.Name;
                    names.Add(name);
                }
               
            }
            return names;
        }
        #region 变更
        /// <summary>
        /// 生成添加语句
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="filtersNames">需要排除的属性，不区分大小写</param>
        /// <returns></returns>
        public static string InsertSql<T>(List<string> filtersNames = null)
        {


            var modelName = InitScheamAndTable<T>();
            List<string> conList = new List<string>();
            if (filtersNames != null && filtersNames.Any())
            {
                foreach (var item in filtersNames)
                {
                    conList.Add(item.ToUpper());
                }
            }
            string insertSql = $"insert  into {modelName} ";
            string names = "";
            string values = "";
            foreach (var item in SelectFieldsNames<T>())
            {
                if (conList != null && conList.Any() && conList.Contains(item.ToUpper())) continue;
                names += $"[{item}],";
                values += $"@{item},";
            }
            return insertSql + $"({names.TrimEnd(',') }) VALUES ({values.TrimEnd(',')})";
        }
        /// <summary>
        /// 修改语句生成
        /// </summary>
        /// <param name="model">模型</param>
        /// <param name="whereStr">筛选条件值</param>
        /// <param name="filtersNames">排除属性</param>
        /// <returns></returns>
        public static string UpdateSql<T>(string whereStr, List<string> filtersNames = null)
        {

            var modelName = InitScheamAndTable<T>();
            List<string> conList = new List<string>();
            if (filtersNames != null && filtersNames.Any())
            {
                foreach (var item in filtersNames)
                {
                    conList.Add(item.ToUpper());
                }
            }
            string updateSql = $"UPDATE    {modelName} SET   ";
            string udpDate = "";
            //更新主体
            foreach (var item in SelectFieldsNames<T>())
            {
                if (conList != null && conList.Any() && conList.Contains(item.ToUpper())) continue;
                udpDate += $"[{item}]=@{item},";
            }
            return updateSql + $"{udpDate.TrimEnd(',') } where  1=1  {whereStr}";
        }
      /// <summary>
      /// 修改语句部分字段修改
      /// </summary>
      /// <typeparam name="T">模型名称</typeparam>
      /// <param name="updateFilde">需要修改数据</param>
      /// <param name="whereStr">条件</param>
      /// <returns></returns>
        public static string UpdateSql<T>(List<string> updateFilde,string whereStr)
        {
            var modelName = InitScheamAndTable<T>();
            var fildeNames = "";
            //更新字段
            foreach (var item in updateFilde)
            {
                fildeNames += $" {item}=@{item} ,";
            }
            string updateSql = $"UPDATE    {modelName} SET    ";
            return updateSql + $"{fildeNames.TrimEnd(',') } where  1=1  {whereStr}";
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="whereStr">删除条件</param>
        /// <param name="isPhysical">是否物理删除 默认逻辑删除</param>
        /// <param name="scheam"></param>
        /// <returns></returns>
        public static string DeleteSql<T>(string whereStr, bool isPhysical = false)
        {
            var modelName = InitScheamAndTable<T>();
            if (isPhysical)
            {
                return $" DELETE FROM {modelName} WHERE  {whereStr}";
            }
            else
            {
                return $" Update   {modelName} SET isDeleted=true WHERE  {whereStr} ";
            }
        }


        #endregion

        #region 查询
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="whereStr">筛选条件名称</param>
        /// <param name="sortName">排序名称</param>
        /// <param name="sortType">排序类型:默认倒叙DESC</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="filtersNames">排除字段，默认不排除</param>
        /// <param name="scheam">数据分区</param>
        /// <returns></returns>
        public static string QueryPageSql<T>(string whereStr, List<string> filtersNames = null, string sortName = "CreatTime", string sortType = "DESC", int pageIndex = 1, int pageSize = 20)
        {

            int startIndex = pageSize * (pageIndex - 1) + 1;//该分页的起始值
            if (startIndex < 0) { startIndex = 1; }
            var modelName = InitScheamAndTable<T>();
            List<string> conList = new List<string>();
            string fields = "*";
            if (filtersNames != null && filtersNames.Any())
            {
                fields = "";
                foreach (var item in filtersNames)
                {
                    conList.Add(item.ToUpper());
                }
                foreach (var item in SelectFieldsNames<T>())
                {
                    if (conList != null && conList.Any() && conList.Contains(item.ToUpper())) continue;
                    fields += $",{item}  ";
                }
            }
            
            
            if (!whereStr.IsNullOrEmpty())
            {
                whereStr = $" where 1=1 AND {whereStr} ";
            }
            int startrecord = 1;
            int endRecord = pageSize;
            if (pageIndex == 1) startrecord = pageIndex;
            else
            {
                startrecord = pageIndex * pageSize + 1;
                endRecord=(pageIndex+1)*pageSize;
            }
              
            string querStr = $@"SELECT * FROM(
                            SELECT ROW_NUMBER()OVER(order by a.{sortName} {sortType}) AS RN ,{fields.TrimStart(',')} FROM {modelName} a
                             {whereStr}  )t WHERE t.RN BETWEEN {startrecord}  AND  {endRecord}";

            return querStr;

        }

        /// <summary>
        /// 不分页查询
        /// </summary>
        /// <param name="whereStr"></param>
        /// <param name="sortName"></param>
        /// <param name="sortType"></param>
        /// <param name="filtersNames"></param>
        /// <param name="scheam"></param>
        /// <returns></returns>
        public static string QuerySql<T>(string whereStr, string sortName = "CreatTime", string sortType = "DESC", List<string> filtersNames = null, string scheam = "[dbo].")
        {
            var modelName = InitScheamAndTable<T>();
            List<string> conList = new List<string>();
            if (filtersNames != null && filtersNames.Any())
            {
                foreach (var item in filtersNames)
                {
                    conList.Add(item.ToUpper());
                }
            }
            string fields = "";
            foreach (var item in SelectFieldsNames<T>())
            {
                if (conList != null && conList.Any() && conList.Contains(item.ToUpper())) continue;
                fields += $",{item}  ";
            }
            if (!whereStr.IsNullOrEmpty())
            {
                whereStr = $" where 1=1  {whereStr} ";
            }
            string querStr = $@"SELECT *  FROM(
                            SELECT ROW_NUMBER()OVER(order by a.{sortName} {sortType}) AS RN ,{fields.TrimStart(',')} FROM {modelName} a
                             {whereStr}  )t ";

            return querStr;

        }

        /// <summary>
        /// 获取单条信息
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="filtersNames"></param>
        /// <param name="scheam"></param>
        /// <returns></returns>
        public static string QueryOne<T>(string keyName, List<string> filtersNames = null, string scheam = "dbo.")
        {

            var modelName = InitScheamAndTable<T>();
            List<string> conList = new List<string>();
            if (filtersNames != null && filtersNames.Any())
            {
                foreach (var item in filtersNames)
                {
                    conList.Add(item.ToUpper());
                }
            }
            string names = "";
            string values = "";
            foreach (var item in SelectFieldsNames<T>())
            {
                if (conList != null && conList.Any() && conList.Contains(item.ToUpper())) continue;
                names += $"[{item}],";
                values += $"@{item},";
            }
            return $"Select  {names.TrimEnd(',') } From {modelName}  Where  {keyName}=@{keyName}";
        }

        /// <summary>
        /// 获取条件内的所有数据行
        /// </summary>
        /// <param name="whereStr">参数条件</param>
        /// <param name="scheam"></param>
        /// <returns></returns>
        public static string QueryCount<T>(string whereStr, string scheam = "dbo.")
        {
            var modelName = InitScheamAndTable<T>();

            if (!whereStr.IsNullOrEmpty())
            {
                return $"Select  Count(*) From {modelName}  Where 1=1  {whereStr}  ";
            }
            else
            {
                return $"Select  Count(*) From {modelName}   ";
            }
        }
        #endregion

    }
   
}
