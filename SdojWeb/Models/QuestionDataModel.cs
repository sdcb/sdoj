using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using AutoMapper;
using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class QuestionData
    {
        public int Id { get; set; }

        public Question Question { get; set; }

        public int QuestionId { get; set; }

        public string Input { get; set; }

        [Required]
        public string Output { get; set; }

        public float MemoryLimitMb { get; set; }

        public int TimeLimit { get; set; }

        public DateTime UpdateTime { get; set; }
    }

    public class QuestionDataFullModel : IMapFrom<QuestionData>
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public int QuestionId { get; set; }

        [JsonProperty("c")]
        public string Input { get; set; }

        [JsonProperty("d")]
        public string Output { get; set; }

        [JsonProperty("e")]
        public float MemoryLimitMb { get; set; }

        [JsonProperty("f")]
        public int TimeLimit { get; set; }
    }

    public class QuestionDataHashModel : IMapFrom<QuestionData>
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public DateTime UpdateTime { get; set; }
    }

    public class QuestionDataSummaryModel : IHaveCustomMapping
    {
        public int Id { get; set; }

        [Display(Name = "题目")]
        public string QuestionName { get; set; }

        public int QuestionId { get; set; }

        [Display(Name = "输入字符数")]
        public int InputLength { get; set; }

        [Display(Name = "输出字符数")]
        public int OutputLength { get; set; }

        [Display(Name = "编辑日期")]
        public DateTime UpdateTime { get; set; }

        [Display(Name = "内存限制"), DisplayFormat(DataFormatString = "{0} MB")]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制"), DisplayFormat(DataFormatString = "{0} ms")]
        public int TimeLimit { get; set; }

        public bool IsSampleData { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionData, QuestionDataSummaryModel>()
                .ForMember(d => d.IsSampleData, s => s.MapFrom(x => x.Question.SampleDataId == x.Id))
                .ForMember(d => d.MemoryLimitMb, s => s.MapFrom(x => x.MemoryLimitMb))
                .ForMember(d => d.TimeLimit, s => s.MapFrom(x => x.TimeLimit));
        }
    }

    public class QuestionDataEditModel : IHaveCustomMapping
    {
        [HiddenInput]
        public int Id { get; set; }

        [HiddenInput, Required]
        public int QuestionId { get; set; }

        [Display(Name = "题目"), Editable(false)]
        public string QuestionName { get; set; }

        [Display(Name = "输入数据"), DataType(DataType.MultilineText)]
        public string Input { get; set; }

        [Display(Name = "输出数据"), DataType(DataType.MultilineText), Required]
        public string Output { get; set; }

        [Display(Name = "内存限制(MB)"), Required]
        public float MemoryLimitMb { get; set; }

        [Display(Name = "时间限制(ms)"), Required]
        public int TimeLimit { get; set; }

        [HiddenInput]
        public int CreateUserId { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionDataEditModel, QuestionData>()
                .ForMember(dest => dest.UpdateTime, source => source.UseValue(DateTime.Now));
            configuration.CreateMap<QuestionData, QuestionDataEditModel>()
                .ForMember(d => d.CreateUserId, s => s.MapFrom(x => x.Question.CreateUserId))
                .ForMember(d => d.QuestionName, s => s.MapFrom(x => x.Question.Name));
        }
    }
}