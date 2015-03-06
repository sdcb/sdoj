using SdojJudger.Models;
using System;

namespace SdojJudger.Database
{
    public struct DbHashModel
    {
        public int Id { get; set; }

        public long UpdateTicks { get; set; }

        public static explicit operator DbHashModel(DataHashModel b)
        {
            return new DbHashModel
            {
                Id = b.Id, 
                UpdateTicks = b.UpdateTime.Ticks
            };
        }

        public static explicit operator DataHashModel(DbHashModel b)
        {
            return new DataHashModel
            {
                Id = b.Id, 
                UpdateTime = new DateTime(b.UpdateTicks)
            };
        }
    }
}
