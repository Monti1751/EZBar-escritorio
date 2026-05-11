using Moq;
using FluentAssertions;
using EZBarEscritorio.Infrastructure.Network;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Moq.Protected;
using System.Threading;

namespace EZBarEscritorio.Tests
{
    public class ServiceTests
    {
        [Fact]
        public async Task NgrokApiService_GetAsync_ReturnsData()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new List<string> { "item1", "item2" })
            };

            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com") };
            var service = new NgrokApiService(httpClient);

            var result = await service.GetAsync<string>("/test");
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task PostAsync_ReturnsFalseOnFailure()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com") };
            var service = new NgrokApiService(httpClient);

            var result = await service.PostAsync("/test", new { });
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PutAsync_ReturnsTrueOnSuccess()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com") };
            var service = new NgrokApiService(httpClient);

            var result = await service.PutAsync("/test", new { });
            result.Should().BeTrue();
        }

        [Fact]
        public async Task PutAsync_ReturnsFalseOnFailure()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com") };
            var service = new NgrokApiService(httpClient);

            var result = await service.PutAsync("/test", new { });
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PatchAsync_ReturnsTrueOnSuccess()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://test.com") };
            var service = new NgrokApiService(httpClient);

            var result = await service.PatchAsync("/test", new { });
            result.Should().BeTrue();
        }
    }

    public class AuthInterceptorTests
    {
        [Fact]
        public async Task SendAsync_AddsAuthAndNgrokHeaders()
        {
            var interceptor = new AuthInterceptor("user", "pass");
            var innerHandler = new Mock<HttpMessageHandler>();
            interceptor.InnerHandler = innerHandler.Object;

            innerHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var invoker = new HttpMessageInvoker(interceptor);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Contains("ngrok-skip-browser-warning").Should().BeTrue();
        }
    }
}
