using GameFoundation.Scripts.Utilities.LogService;
using global::Models;
using VContainer;

namespace GameFoundation.Scripts.Network.WebService
{
    public class WrappedRequestNoResponseHttpServices : BestBaseHttpProcess, IWrapRequest
    {
        public WrappedRequestNoResponseHttpServices(ILogService logger, NetworkLocalData localData, NetworkConfig networkConfig)
            : base(logger, localData, networkConfig)
        {
        }
    }
}