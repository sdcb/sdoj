using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using AutoMapper;
using SdojWeb.Infrastructure.Mapping;

namespace SdojWeb.Models
{
    public class QuestionData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Question Question { get; set; }

        public int QuestionId { get; set; }

        public string Input { get; set; }

        [Required]
        public string Output { get; set; }

        public DateTime UpdateTime { get; set; }
    }

    public class QuestionDataHashModel : IMapFrom<QuestionData>
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public DateTime UpdateTime { get; set; }

        public string Input { get; set; }

        public string Output { get; set; }
    }

    public class QuestionDataFullModel : IMapFrom<QuestionData>
    {
        public int Id { get; set; }

        public int QuestionId { get; set; }
    }

    public class QuestionDataSummaryModel : IMapFrom<QuestionData>
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
    }

    public class QuestionDataEditModel : IHaveCustomMapping, IMapFrom<QuestionData>
    {
        [HiddenInput, Required]
        public int Id { get; set; }

        [Display(Name = "题目ID"), Editable(false), Required]
        public int QuestionId { get; set; }

        [Display(Name = "输入数据"), DataType(DataType.MultilineText)]
        public string Input { get; set; }

        [Display(Name = "输出数据"), DataType(DataType.MultilineText), Required]
        public string Output { get; set; }

        public void CreateMappings(IConfiguration configuration)
        {
            configuration.CreateMap<QuestionDataEditModel, QuestionData>()
                .ForMember(dest => dest.UpdateTime, source => source.UseValue(DateTime.Now));
        }
    }
}