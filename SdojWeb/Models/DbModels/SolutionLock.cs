using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SdojWeb.Models.DbModels
{
    public class SolutionLock
    {
        [Key, ForeignKey("Solution")]
        public int SolutionId { get; set; }

        public Solution Solution { get; set; }

        public Guid LockClientId { get; set; }

        public DateTime LockEndTime { get; set; }
    }
}