using System;
using IParkBusinessLogic;

namespace obligatorio.WebApi.Infra;

public class DefaultSystemClock : ISystemClock
{
    public DateTime Now() => DateTime.UtcNow;
}
