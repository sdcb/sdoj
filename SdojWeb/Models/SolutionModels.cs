using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SdojWeb.Models
{
    public class Solution
    {
        [HiddenInput]
        public int Id { get; set; }

        [HiddenInput, Required]
        public ApplicationUser CreateUser { get; set; }

        [HiddenInput, Required]
        public Question Question { get; set; }

        [Display(Name = "语言")]
        public Languages Language { get; set; }

        [Display(Name = "代码"), Required]
        public string Code { get; set; }

        [Display(Name = "状态")]
        public SolutionStatus Status { get; set; }

        [Display(Name = "提交时间")]
        public DateTime SubmitTime { get; set; }
    }

    public enum SolutionStatus
    {
        Queuing, 
        Juding, 
        Accepted, 
        WrongAnswer, 
    }

    public enum Languages
    {
        CSharp,
    }
}