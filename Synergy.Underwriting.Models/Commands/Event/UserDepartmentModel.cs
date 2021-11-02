using System;

namespace Synergy.Underwriting.Models.Commands.Event
{
    public class UserDepartmentModel
    {
        public Guid UserId { get; set; }

        public int DepartmentId { get; set; }
    }
}
