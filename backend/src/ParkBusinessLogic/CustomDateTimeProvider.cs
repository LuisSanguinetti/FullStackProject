using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IParkBusinessLogic;

namespace Park.BusinessLogic;
public class CustomDateTimeProvider : ICustomDateTimeProvider
{
    private DateTime? _customTime;

    public DateTime GetNowUtc()
    {
        return _customTime ?? DateTime.UtcNow;
    }

    public void SetCustomTime(DateTime customTime)
    {
        _customTime = customTime;
    }

    public void ClearCustomTime()
    {
       _customTime = null;
    }
}
