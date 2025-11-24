using BLL.DTOs.Instructor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces.Instructor;

public interface IInstructorDashboardService
{
    Task<InstructorDashboardDto> GetInstructorDashboardAsync(int instructorId);
}
