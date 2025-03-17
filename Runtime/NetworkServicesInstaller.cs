namespace GameFoundation.Scripts.Network
{
    using GameFoundation.Scripts.Network.Signal;
    using GameFoundation.Scripts.Network.WebService;
    using VContainer;
    using GameFoundation.Signals;

    public static class NetworkServicesVContainerInstaller
    {
        public static void RegisterNetworkServices(this IContainerBuilder builder, NetworkConfig networkConfig)
        {
            builder.DeclareSignal<MissStatusCodeSignal>();
            builder.Register<WrappedBestHttpService>(Lifetime.Singleton).As<IHttpService>();

            // Use this for no wrapped request and no wrapped response (data field in response is not wrapped)
            builder.Register<NoWrappedRequestAndResponseService>(Lifetime.Singleton).As<IHttpService>();

            // Use this for wrapped request and no wrapped response (data field in response is not wrapped)
            // builder.Register<WrappedRequestNoResponseHttpServices>(Lifetime.Singleton).As<IHttpService>();

            // Use this for no wrapped request and wrapped response (data field in response is wrapped)
            // builder.Register<WrappedResponseNoRequestHttpServices>(Lifetime.Singleton).As<IHttpService>();
        }
    }
}