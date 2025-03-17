using System;
using Cysharp.Threading.Tasks;
using GameFoundation.Scripts.Network.Signal;
using GameFoundation.Scripts.Utilities.LogService;
using UniRx;
using VContainer; // Dù không dùng pool, vẫn sử dụng VContainer cho DI

namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.Signals;

    /// <summary>
    /// Giao diện cung cấp các phương thức gửi HTTP request.
    /// </summary>
    public interface IHttpService
    {
        string Host { get; set; }
        UniTask<TK> SendPostAsync<T, TK>(object httpRequestData = null, string jwtToken = "") where T : BasePostRequest<TK>;
        UniTask<TK> SendGetAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseGetRequest<TK>;
        UniTask<TK> SendPutAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = false) where T : BasePutRequest<TK>;
        UniTask<TK> SendPatchAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BasePatchRequest<TK>;
        UniTask<TK> SendDeleteAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseDeleteRequest<TK>;
        UniTask Download(string address, string filePath, OnDownloadProgressDelegate onDownloadProgress);
        UniTask<byte[]> DownloadAndReadStreaming(string address, OnDownloadProgressDelegate onDownloadProgress);
        string GetDownloadPath(string path);
        BoolReactiveProperty HasInternetConnection { get; set; }
        BoolReactiveProperty IsProcessApi { get; set; }
    }

    /// <summary>
    /// Delegate cho tiến trình download.
    /// </summary>
    public delegate void OnDownloadProgressDelegate(long downloaded, long downloadLength);

    public abstract class BaseHttpRequest
    {
        [Inject] private SignalBus signalBus;

        public abstract void Process(object responseData);

        public virtual void ErrorProcess(int statusCode)
        {
            signalBus.Fire(new MissStatusCodeSignal());
        }

        public virtual void ErrorProcess(object errorData) { }

        public virtual void PredictProcess(object requestData) { }

        public class MissStatusCodeException : Exception { }
    }

    public abstract class BasePostRequest<T> : BaseHttpRequest<T>
    {
        protected BasePostRequest(ILogService logger) : base(logger) { }
    }

    public abstract class BaseGetRequest<T> : BaseHttpRequest<T>
    {
        protected BaseGetRequest(ILogService logger) : base(logger) { }
    }

    public abstract class BasePutRequest<T> : BaseHttpRequest<T>
    {
        protected BasePutRequest(ILogService logger) : base(logger) { }
    }

    public abstract class BasePatchRequest<T> : BaseHttpRequest<T>
    {
        protected BasePatchRequest(ILogService logger) : base(logger) { }
    }

    public abstract class BaseDeleteRequest<T> : BaseHttpRequest<T>
    {
        protected BaseDeleteRequest(ILogService logger) : base(logger) { }
    }

    // Chú ý: Phần pool đã bị loại bỏ vì IMemoryPool là của Zenject.
    // Nếu cần, bạn có thể tự cài đặt giải pháp pooling riêng.
    
    public abstract class BaseHttpRequest<T> : BaseHttpRequest, IDisposable
    {
        protected readonly ILogService Logger;

        protected BaseHttpRequest(ILogService logger)
        {
            this.Logger = logger;
        }

        // Dispose không cần xử lý pooling, để trống hoặc thêm logic giải phóng tài nguyên khác nếu cần.
        public virtual void Dispose() { }

        public override void Process(object responseData)
        {
            this.PreProcess((T)responseData);
            this.Process((T)responseData);
            this.PostProcess((T)responseData);
        }

        public abstract void Process(T responseData);
        public virtual void PostProcess(T responseData) { }
        public virtual void PreProcess(T responseData) { }
    }

    public interface IFakeResponseAble<out T>
    {
        T FakeResponse();
    }
}
