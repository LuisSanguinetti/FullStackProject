using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IParkBusinessLogic;
public interface ICustomDateTimeProvider
{
    DateTime GetNowUtc();
    void SetCustomTime(DateTime customTime);
    void ClearCustomTime();
}
