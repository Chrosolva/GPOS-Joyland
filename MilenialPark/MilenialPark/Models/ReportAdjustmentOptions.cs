using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilenialPark.Models
{
    public enum ReportRoundingMode
    {
        None,
        Nearest,
        Down,
        Up
    }

    public class ReportAdjustmentOptions
    {
        public bool Enabled { get; set; }
        public decimal Percentage { get; set; }
        public ReportRoundingMode RoundingMode { get; set; }

        public ReportAdjustmentOptions()
        {
            Enabled = false;
            Percentage = 0;
            RoundingMode = ReportRoundingMode.Nearest;
        }
    }
}
