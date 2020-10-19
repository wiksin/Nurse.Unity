using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nurse.Unity
{
    /// <summary>
    /// 模型映射属性
    /// </summary>
    public class TableAttribute : Attribute
    {

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">表名称</param>
        public TableAttribute(string name) {
            this.Name = name;
        }

       /// <summary>
       /// 表名称
       /// </summary>
        public string Name { get; }
        /// <summary>
        /// Schema 名称
        /// </summary>
        public string Schema { get; set; }
    }
}
