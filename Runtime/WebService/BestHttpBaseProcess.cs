using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BestHTTP;
using Cysharp.Threading.Tasks;
using GameFoundation.Scripts.Network.WebService.Requests;
using GameFoundation.Scripts.Utilities.LogService;
using GameFoundation.Scripts.Utilities.Utils;
using global::Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;
using VContainer;

namespace GameFoundation.Scripts.Network.WebService
{
    using GameFoundation.DI;
    using GameFoundation.Scripts.Interfaces;

    public abstract class BestBaseHttpProcess : IHttpService, IInitializable, IDisposable
    {
        protected readonly ILogService      Logger;
        protected readonly NetworkLocalData LocalData;
        protected readonly NetworkConfig    NetworkConfig;

        protected BestBaseHttpProcess(ILogService logger, ILocalData localData, NetworkConfig networkConfig)
        {
            this.Logger        = logger;
            this.LocalData     = (NetworkLocalData)localData;
            this.NetworkConfig = networkConfig;
            this.Host          = networkConfig.Host;
        }

        protected Dictionary<HTTPMethods, int> RetryCount = new();

        public virtual void Initialize()
        {
            this.RetryCount[HTTPMethods.Post]   = 0;
            this.RetryCount[HTTPMethods.Get]    = 0;
            this.RetryCount[HTTPMethods.Put]    = 0;
            this.RetryCount[HTTPMethods.Patch]  = 0;
            this.RetryCount[HTTPMethods.Delete] = 0;
        }

        public virtual void Dispose() { }

        protected virtual void InitBaseRequest(HTTPRequest request, object httpRequestData, string token)
        {
            if (this is IWrapRequest)
                this.InitWrapData(request, httpRequestData, token);
            else
                this.InitNoWrapData(request, httpRequestData, token);
        }

        protected virtual void InitWrapData(HTTPRequest request, object httpRequestData, string token)
        {
            var wrappedData = new ClientWrappedHttpRequestData();
            wrappedData.Data = httpRequestData;
            request.AddHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", "Bearer " + token);

            if (!string.IsNullOrEmpty(GameVersion.Version))
                request.AddHeader("game-version", GameVersion.Version);

            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wrappedData));
        }

        protected virtual void InitNoWrapData(HTTPRequest request, object httpRequestData, string token)
        {
            if (!string.IsNullOrEmpty(token))
                request.AddHeader("Authorization", "Bearer " + token);

            if (!string.IsNullOrEmpty(GameVersion.Version))
                request.AddHeader("game-version", GameVersion.Version);

            request.AddHeader("Content-Type", "application/json");
            request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(httpRequestData));
        }

        #region Post

        public virtual void InitPostRequest(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public virtual async UniTask<TK> SendPostAsync<T, TK>(object httpRequestData = null, string jwtToken = "") where T : BasePostRequest<TK>
        {
            var httpRequestDefinition = (HttpRequestDefinitionAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute));

            if (httpRequestDefinition == null)
                throw new Exception($"Request {typeof(T)} chưa được định nghĩa! Hãy thêm HttpRequestDefinitionAttribute cho nó!");

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = Activator.CreateInstance(typeof(T), Logger) as T;
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);
                return responseData;
            }
#endif
            var response = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, null, jwtToken, true, HTTPMethods.Post);

            return response;
        }

        #endregion

        #region Get

        public virtual void InitGetRequest(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public virtual async UniTask<TK> SendGetAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseGetRequest<TK>
        {
            var httpRequestDefinition = (HttpRequestDefinitionAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute));

            if (httpRequestDefinition == null)
                throw new Exception($"Request {typeof(T)} chưa được định nghĩa! Hãy thêm HttpRequestDefinitionAttribute cho nó!");

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = Activator.CreateInstance(typeof(T), Logger) as T;
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);
                return responseData;
            }
#endif
            var parameters = this.SetParam<T, TK>(httpRequestData);
            var response   = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Get);

            return response;
        }

        #endregion

        #region Put

        public virtual void InitRequestPut(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public async UniTask<TK> SendPutAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = false) where T : BasePutRequest<TK>
        {
            var httpRequestDefinition = (HttpRequestDefinitionAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute));

            if (httpRequestDefinition == null)
                throw new Exception($"Request {typeof(T)} chưa được định nghĩa! Hãy thêm HttpRequestDefinitionAttribute cho nó!");

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = Activator.CreateInstance(typeof(T), Logger) as T;
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);
                return responseData;
            }
#endif
            var parameters = this.SetParam<T, TK>(httpRequestData);
            var response   = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Put);

            return response;
        }

        #endregion

        #region Patch

        public virtual void InitRequestPatch(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public async UniTask<TK> SendPatchAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BasePatchRequest<TK>
        {
            var httpRequestDefinition = (HttpRequestDefinitionAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute));

            if (httpRequestDefinition == null)
                throw new Exception($"Request {typeof(T)} chưa được định nghĩa! Hãy thêm HttpRequestDefinitionAttribute cho nó!");

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = Activator.CreateInstance(typeof(T), Logger) as T;
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);
                return responseData;
            }
#endif
            var parameters = this.SetParam<T, TK>(httpRequestData);
            var response   = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Patch);

            return response;
        }

        #endregion

        #region Delete

        public virtual void InitDeleteRequest(HTTPRequest request, object httpRequestData, string token) { this.InitBaseRequest(request, httpRequestData, token); }

        public virtual async UniTask<TK> SendDeleteAsync<T, TK>(object httpRequestData = null, string jwtToken = "", bool includeBody = true) where T : BaseDeleteRequest<TK>
        {
            var httpRequestDefinition = (HttpRequestDefinitionAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(HttpRequestDefinitionAttribute));

            if (httpRequestDefinition == null)
                throw new Exception($"Request {typeof(T)} chưa được định nghĩa! Hãy thêm HttpRequestDefinitionAttribute cho nó!");

#if (DEVELOPMENT_BUILD || UNITY_EDITOR) && FAKE_DATA
            if (typeof(IFakeResponseAble<TK>).IsAssignableFrom(typeof(T)))
            {
                var baseHttpRequest = Activator.CreateInstance(typeof(T), Logger) as T;
                var responseData = ((IFakeResponseAble<TK>)baseHttpRequest).FakeResponse();
                baseHttpRequest.Process(responseData);
                return responseData;
            }
#endif
            var parameters = this.SetParam<T, TK>(httpRequestData);
            var response   = await this.TryGetResponse<T, TK>(httpRequestData, httpRequestDefinition.Route, parameters, jwtToken, includeBody, HTTPMethods.Delete);

            return response;
        }

        #endregion

        public async UniTask Download(string address, string filePath, OnDownloadProgressDelegate onDownloadProgress)
        {
            filePath = this.GetDownloadPath(filePath);
            var request = new HTTPRequest(new Uri(address));
            request.Timeout            =  TimeSpan.FromSeconds(this.GetDownloadTimeout());
            request.OnDownloadProgress =  (req, downloaded, length) => onDownloadProgress(downloaded, length);
            request.OnStreamingData    += OnData;
            request.DisableCache       =  true;
            var response = await request.GetHTTPResponseAsync();
            if (request.Tag is FileStream fs)
                fs.Dispose();

            switch (request.State)
            {
                case HTTPRequestStates.Finished:
                    if (response.IsSuccess)
                        this.Logger.Log($"Download {filePath} Done!");
                    else
                        this.Logger.Warning($"Request finished successfully, but server sent error. Status Code: {response.StatusCode}-{response.Message} Message: {response.DataAsText}");

                    break;
                default:
                    File.Delete(filePath);

                    break;
            }

            bool OnData(HTTPRequest req, HTTPResponse resp, byte[] dataFragment, int dataFragmentLength)
            {
                if (resp.IsSuccess)
                {
                    if (!(req.Tag is FileStream fileStream))
                        req.Tag = fileStream = new FileStream(filePath, FileMode.Create);
                    fileStream.Write(dataFragment, 0, dataFragmentLength);
                }

                return true;
            }
        }

        public async UniTask<byte[]> DownloadAndReadStreaming(string address, OnDownloadProgressDelegate onDownloadProgress)
        {
            var responseData = new byte[] { };
            var request      = new HTTPRequest(new Uri(address));
            request.Timeout            =  TimeSpan.FromSeconds(this.GetDownloadTimeout());
            request.OnDownloadProgress =  (req, downloaded, length) => onDownloadProgress(downloaded, length);
            request.OnStreamingData    += OnData;
            request.DisableCache       =  true;
            await request.GetHTTPResponseAsync();

            bool OnData(HTTPRequest req, HTTPResponse resp, byte[] dataFragment, int dataFragmentLength)
            {
                if (resp.IsSuccess)
                    responseData = dataFragment;

                return true;
            }

            return responseData;
        }

        public string GetDownloadPath(string path) { return $"{Application.persistentDataPath}/{path}"; }

        protected double GetHttpTimeout() { return this.NetworkConfig.HttpRequestTimeout; }

        protected double GetDownloadTimeout() { return this.NetworkConfig.DownloadRequestTimeout; }

        public BoolReactiveProperty HasInternetConnection { get; set; } = new(true);
        public BoolReactiveProperty IsProcessApi          { get; set; } = new(false);
        public string               Host                  { get; set; }

        protected virtual StringBuilder SetParam<T, TK>(object httpRequestData) where T : BaseHttpRequest<TK>
        {
            var parameters    = new StringBuilder();
            var propertyInfos = httpRequestData.GetType().GetProperties();

            if (propertyInfos.Length <= 0) return parameters;

            var parametersStr = $"{this.NetworkConfig.ParamDelimiter}" +
                                string.Join(this.NetworkConfig.ParamLink, propertyInfos.Select(propertyInfo =>
                                    typeof(IEnumerable<string>).IsAssignableFrom(propertyInfo.PropertyType)
                                        ? string.Join(this.NetworkConfig.ParamLink, (propertyInfo.GetValue(httpRequestData) as IEnumerable<string>)?.Select(value => $"{propertyInfo.Name}={value}")
                                                                                    ?? Enumerable.Empty<string>())
                                        : $"{propertyInfo.Name}={propertyInfo.GetValue(httpRequestData)}"
                                ));

            parameters.Append(parametersStr);

            return parameters;
        }

        private async UniTask<TK> TryGetResponse<T, TK>(object httpRequestData, string route, StringBuilder parameters, string jwtToken, bool includeBody, HTTPMethods methods)
            where T : BaseHttpRequest, IDisposable
        {
            this.IsProcessApi.Value  = true;
            this.RetryCount[methods] = 0;
            TK          response       = default;
            HTTPRequest request        = null;
            var         canRetry       = true;
            var         maximumRetry   = this.NetworkConfig.MaximumRetryStatusCode0;
            var         retryAttribute = (RetryAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(RetryAttribute));
            if (retryAttribute != null)
                maximumRetry = retryAttribute.RetryCount;

            while (canRetry && response == null && this.RetryCount[methods] < maximumRetry)
                try
                {
                    request         = new HTTPRequest(this.ReplaceUri($"{route}{parameters}"), methods);
                    request.Timeout = TimeSpan.FromSeconds(this.GetHttpTimeout());
                    if (includeBody)
                        switch (methods)
                        {
                            case HTTPMethods.Get:
                                this.InitGetRequest(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Post:
                                this.InitPostRequest(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Put:
                                this.InitRequestPut(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Patch:
                                this.InitRequestPatch(request, httpRequestData, jwtToken);

                                break;
                            case HTTPMethods.Delete:
                                this.InitDeleteRequest(request, httpRequestData, jwtToken);

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(methods), methods, null);
                        }

                    this.HasInternetConnection.Value = true;
                    response                         = await this.MainProcess<T, TK>(request, httpRequestData);
                    canRetry                         = false;
                }
                catch (AsyncHTTPException ex)
                {
                    if (ex.StatusCode == 0)
                    {
                        if (!this.NetworkConfig.AllowRetry)
                        {
                            this.RetryCount[methods] = maximumRetry;
                        }
                        else
                        {
                            this.RetryCount[methods]++;
                            this.HasInternetConnection.Value = true;
                            this.Logger.LogWithColor($"Retry {this.RetryCount[methods]}/{maximumRetry} cho request {request.Uri} Error: {ex.Message}, {ex.StatusCode}, {ex.Content}", Color.cyan);
                        }

                        if (this.RetryCount[methods] >= maximumRetry)
                        {
                            this.Logger.Log($"Request {request.Uri} Error");
                            this.HasInternetConnection.Value = false;
                            this.HandleAsyncHttpException(ex);
                        }
                    }
                    else
                    {
                        canRetry = false;
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(this.NetworkConfig.RetryDelay));
                }

            this.IsProcessApi.Value = false;

            return response;
        }

        protected Uri ReplaceUri(string route)
        {
            foreach (var keyValuePair in this.LocalData.ServerToken.ParameterNameToValue)
            {
                var parameterName  = keyValuePair.Key;
                var parameterValue = keyValuePair.Value;
                route = route.Replace($"{{{parameterName}}}", parameterValue);
            }

            return new Uri($"{this.Host}{route}");
        }

        protected async UniTask<TK> MainProcess<T, TK>(HTTPRequest request, object requestData)
            where T : BaseHttpRequest, IDisposable
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || SHOW_API_LOG
            try
            {
                this.Logger.Log($"{request.Uri} - [REQUEST] - Header: {request.DumpHeaders()} - \n Body:{Encoding.UTF8.GetString(request.GetEntityBody())}");
            }
            catch (Exception e)
            {
                this.Logger.Error($"{e.Message} - {e.StackTrace}");
            }
#endif
            var response = await request.GetHTTPResponseAsync();
#if UNITY_EDITOR || DEVELOPMENT_BUILD || SHOW_API_LOG
            this.Logger.Log($"{request.Uri} - [RESPONSE] - raw data: {response.DataAsText}");
#endif
            TK returnResponse = default;
            this.PreProcess<T>(request, response, (statusCode) =>
                {
                    var responseData = JObject.Parse(Encoding.UTF8.GetString(response.Data));
                    returnResponse = this.RequestSuccessProcess<T, TK>(responseData, requestData);
                },
                (statusCode) => { this.Logger.Error($"Error xử lý request {request.Uri} với status code {statusCode}"); });

            return returnResponse;
        }

        protected virtual TK RequestSuccessProcess<T, TK>(JObject responseData, object requestData)
            where T : BaseHttpRequest, IDisposable
        {
            var baseHttpRequest = Activator.CreateInstance(typeof(T), this.Logger) as T;
            var data            = responseData.ToObject<TK>();

            if (this is IWrapResponse)
            {
                if (responseData.TryGetValue("data", out var requestProcessData))
                {
                    data = requestProcessData.ToObject<TK>();
                    baseHttpRequest.Process(data);
                }
            }
            else
            {
                baseHttpRequest.Process(data);
            }

            baseHttpRequest.PredictProcess(requestData);
            this.PostProcess();

            return data;
        }

        protected void PreProcess<T>(HTTPRequest req, HTTPResponse resp, Action<int> onRequestSuccess, Action<int> onRequestError) where T : BaseHttpRequest, IDisposable
        {
            switch (req.State)
            {
                case HTTPRequestStates.Finished:
                    if (resp.IsSuccess)
                    {
                        onRequestSuccess(resp.StatusCode);
                    }
                    else
                    {
                        if (resp.StatusCode == 400)
                        {
                            var errorMessage = JsonConvert.DeserializeObject<ErrorResponse>(resp.DataAsText);
                            if (errorMessage != null)
                                this.Logger.Error($"{req.Uri} nhận error code: {errorMessage.Code}-{errorMessage.Message}");
                        }
                        else
                        {
                            this.Logger.Error($"{req.Uri} - Request finished successfully nhưng server trả error. Status Code: {resp.StatusCode}-{resp.Message} Message: {resp.DataAsText}");
                        }

                        onRequestError(resp.StatusCode);
                    }

                    break;
                case HTTPRequestStates.Error:
                    this.Logger.Error("Request kết thúc với lỗi! " + (req.Exception != null ? req.Exception.Message + "\n" + req.Exception.StackTrace : "No Exception"));

                    break;
                case HTTPRequestStates.Aborted:
                    this.Logger.Warning("Request bị hủy!");

                    break;
                case HTTPRequestStates.ConnectionTimedOut:
                    this.Logger.Error("Connection Timed Out!");

                    break;
                case HTTPRequestStates.TimedOut:
                    this.Logger.Error("Processing the request Timed Out!");

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void HandleAsyncHttpException(AsyncHTTPException ex)
        {
            this.Logger.Log("Status Code: " + ex.StatusCode);
            this.Logger.Log("Message: " + ex.Message);
            this.Logger.Log("Content: " + ex.Content);
        }

        private void PostProcess()
        {
            // Logic xử lý sau khi request.
        }
    }
}