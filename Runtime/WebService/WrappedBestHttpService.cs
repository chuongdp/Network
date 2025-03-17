using GameFoundation.Scripts.Utilities.LogService;

namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.Scripts.Interfaces;

    public class WrappedBestHttpService : BestBaseHttpProcess, IWrapRequest, IWrapResponse
    {
        public WrappedBestHttpService(ILogService logger, ILocalData localData, NetworkConfig networkConfig)
            : base(logger, localData, networkConfig)
        {
        }
    }
}