using System;
using Synergy.Common.Domain.Models.Abstracts;

namespace Synergy.Underwriting.Models.Bid
{
    public class BidModel : IResultModel
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public string Number { get; set; }

        public string Entity { get; set; }

        public string Portfolio { get; set; }
    }

    //public class EventDumpField
    //{
    //    public int Id { get; set; }

    //    public string VisibleName { get; set; }
    //    public string DBFieldName { get; set; }
    //    public string TableName { get; set; }
    //    public bool IsEnable { get; set; }
    //}

    //public class EventDumpFieldEntity
    //{
    //    public int Id { get; set; }

    //    public string VisibleName { get; set; }
    //    public string DBFieldName { get; set; }

    //    public bool IsEnable { get; set; }
    //}

    //public class DumpModel
    //{
    //    ["InternalDeliquencyId"]
    //    public Guid InternalDeliquencyId { get; set; }

    //    ["EventNumber2"]
    //    public int EventNumber { get; set; }

    //    ["State2"]
    //    public string State { get; set; }
    //}

    //public class DumpModelEntity
    //{
    //    public Guid InternalDeliquencyId { get; set; }
    //    public int EventNumber { get; set; }
    //    public string State { get; set; }
    //}
}
