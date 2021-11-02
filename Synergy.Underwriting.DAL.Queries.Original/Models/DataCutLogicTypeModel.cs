using Synergy.DataAccess.Abstractions.Interfaces;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class DataCutLogicTypeModel : IModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FieldDataType { get; set; }
    }
}
