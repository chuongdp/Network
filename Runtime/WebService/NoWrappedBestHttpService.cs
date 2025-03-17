namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.Scripts.Interfaces;
    using System.Text;
    using GameFoundation.Scripts.Utilities.LogService;
    using global::Models;

    public class NoWrappedRequestAndResponseService : BestBaseHttpProcess
    {
        public NoWrappedRequestAndResponseService(ILogService logger, NetworkLocalData localData, NetworkConfig networkConfig)
            : base(logger, localData, networkConfig)
        {
        }

        protected override StringBuilder SetParam<T, TK>(object httpRequestData) { return base.SetParam<T, TK>(httpRequestData); }
    }
}