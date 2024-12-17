using System.ComponentModel.DataAnnotations;

namespace OpenAPI2MD.CommunityToolkit.Example
{
    /// <summary>
    /// 天气信息实体
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// 备注列表
        /// </summary>
        public List<Remark> Remarks { get; set; }
        /// <summary >
        /// 日期
        /// </summary>
        /// <example>
        /// 2020-02-02
        /// </example>
        public DateTime Date { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        /// <example>100</example>
        [Required]
        public int TemperatureC { get; set; }
       
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        /// <summary>
        /// 汇总
        /// </summary>
        /// <example>
        /// 汇总是这样写的吗
        /// </example>
        public string? Summary { get; set; }

        public Remark Mark { get; set; }

        
    }

    public class Remark
    {
        /// <summary>
        /// idc
        /// </summary>
        /// <example> 3.1415932</example>
        public float percent { get; set; }
        /// <summary>
        /// 标记
        /// </summary>
        /// <example>把大象装冰箱分几步？</example>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <example>true</example>
        public bool OK { get; set; }
    }
}