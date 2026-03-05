using System;

namespace IParkBusinessLogic;

public interface ISystemClock
{
    DateTime Now(); // UTC
}
