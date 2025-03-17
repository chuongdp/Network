namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;

    public class WrappedResponseNoRequestHttpServices : BestBaseHttpProcess, IWrapResponse
    {
        public WrappedResponseNoRequestHttpServices(ILogService logger, NetworkLocalData localData, NetworkConfig networkConfig)
            : base(logger, localData, networkConfig)
        {
        }
    }
}