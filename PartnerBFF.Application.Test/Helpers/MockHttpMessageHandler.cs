using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PartnerBFF.Application.Test.Helpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode? _statusCode;
        private readonly object? _responseObject;
        private readonly Func<HttpResponseMessage>? _responder;

        public MockHttpMessageHandler(HttpStatusCode statusCode, object? responseObject = null)
        {
            _statusCode = statusCode;
            _responseObject = responseObject;
        }

        public MockHttpMessageHandler(Func<HttpResponseMessage> responder)
        {
            _responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _responder = () => response ?? new HttpResponseMessage(HttpStatusCode.OK);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (_responder != null)
                {
                    // allow responder to throw (e.g. TimeoutException) — propagate as task fault
                    return Task.FromResult(_responder());
                }

                var status = _statusCode ?? HttpStatusCode.OK;
                var response = new HttpResponseMessage(status);

                if (_responseObject != null)
                {
                    var json = JsonSerializer.Serialize(_responseObject);
                    response.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                return Task.FromException<HttpResponseMessage>(ex);
            }
        }
    }
}