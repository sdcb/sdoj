using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojWeb.Models.DbModels
{
    public class QuestionFunction
    {
        public int Id { get; set; }
        
        [Index(IsUnique = true)]
        public int QuestionId { get; set; }

        [Required, MaxLength(30)]
        public string Name { get; set; }

        public Question Question { get; set; }

        public ICollection<FunctionParameter> Parameters { get; set; }

        public ParameterTypes ReturnType { get; set; }

        public int TimeLimitMs { get; set; }

        public float MemoryLimitMb { get; set; }
    }

    public enum ParameterTypes
    {
        Int32 = 0,         // int
        Double = 1,        // double
        String = 2,        // string
        ArrayOfInt32 = 3,  // int[]
        ArrayOfDouble = 4, // double[]
        ArrayOfString = 5, // string[]
    }

    public class FunctionParameter
    {
        public int Id { get; set; }

        public int QuestionFunctionId { get; set; }

        public QuestionFunction QuestionFunction { get; set; }

        public ParameterTypes ParameterType { get; set; }

        public string Name { get; set; }
    }

    public class FunctionData
    {
        public int Id { get; set; }

        public int FunctionId { get; set; }

        public QuestionFunction Function { get; set; }

        [Required, MaxLength(DataLimit)]
        public byte[] Arguments { get; set; }

        [Required, MaxLength(DataLimit)]
        public byte[] ExpectedReturn { get; set; }

        public DateTime UpdateTime { get; set; }

        public const int DataLimit = 4 * 1024 * 1024; // 4 MB
    }
}