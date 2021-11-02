using System;

namespace Synergy.Underwriting.DAL.Commands.Models
{
    public class UserDepartmentsModel
    {
        public Guid UserId { get; set; }

        public int DepartmentId { get; set; }
    }
}