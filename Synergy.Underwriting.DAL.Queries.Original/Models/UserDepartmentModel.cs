using System;

namespace Synergy.Underwriting.DAL.Queries.Original.Models
{
    public class UserDepartmentModel
    {
        public Guid UserId { get; set; }

        public int DepartmentId { get; set; }
    }
}