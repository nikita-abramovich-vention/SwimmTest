using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync.Models;

namespace DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync
{
    public interface IStudentLabService
    {
        Task<IReadOnlyCollection<StudentLabInternship>> GetInternshipsAsync(DateTime? changeDate = null);
    }
}
