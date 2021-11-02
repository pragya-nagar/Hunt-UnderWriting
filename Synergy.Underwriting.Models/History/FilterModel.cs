using System;

namespace Synergy.Underwriting.Models
{
    public class FilterModel
    {
        public FilterModel()
        {
            this.DateFrom = DateTime.Today.AddDays(-1);
            this.DateTo = DateTime.Today.AddDays(+1);
        }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}
