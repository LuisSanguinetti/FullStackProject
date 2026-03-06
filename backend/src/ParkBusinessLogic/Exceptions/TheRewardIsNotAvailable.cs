using System.Xml.Linq;

namespace Park.BusinessLogic.Exceptions;
public class TheRewardIsNotAvailable : Exception
{
    public TheRewardIsNotAvailable(string reward)
        : base($"The reward: {reward}, is not available")
    {
    }
}
