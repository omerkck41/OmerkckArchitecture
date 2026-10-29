using AwesomeAssertions;
using Kck.AspNetCore.Controllers;
using Kck.AspNetCore.Helpers;
using Kck.AspNetCore.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Kck.AspNetCore.Tests;

public sealed class ResponseTests
{
    private sealed class TestController : KckApiControllerBase
    {
        public IActionResult Ok<T>(T data, string message = "", int statusCode = 200) => ApiSuccess(data, message, statusCode);

        public IActionResult Fail(string message, int statusCode = 400, string? errorType = null, string? detail = null) =>
            ApiFail(message, statusCode, errorType, detail);
    }

    private static DefaultHttpContext CreateContext(string method, string path = "/api/items")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.com");
        context.Request.Path = path;
        context.TraceIdentifier = "trace-1";
        return context;
    }

    private static TestController CreateController(DefaultHttpContext context) =>
        new() { ControllerContext = new ControllerContext { HttpContext = context } };

    // ---- KckApiControllerBase ----

    [Fact]
    public void ApiSuccess_Defaults_Returns200WithSuccessEnvelope()
    {
        var sut = CreateController(CreateContext("GET", "/api/orders"));

        var result = sut.Ok("payload", "done");

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(200);
        var body = obj.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        body.Success.Should().BeTrue();
        body.Data.Should().Be("payload");
        body.Message.Should().Be("done");
        body.StatusCode.Should().Be(200);
        body.Instance.Should().Be("/api/orders");
        body.ErrorType.Should().BeNull();
    }

    [Fact]
    public void ApiSuccess_CustomStatusCode_UsesItOnResultAndBody()
    {
        var sut = CreateController(CreateContext("POST"));

        var obj = sut.Ok(5, statusCode: 201).Should().BeOfType<ObjectResult>().Subject;

        obj.StatusCode.Should().Be(201);
        obj.Value.Should().BeOfType<ApiResponse<int>>().Which.StatusCode.Should().Be(201);
    }

    [Fact]
    public void ApiFail_Defaults_Returns400WithErrorEnvelope()
    {
        var sut = CreateController(CreateContext("POST", "/api/orders"));

        var obj = sut.Fail("bad", errorType: "ValidationError", detail: "name missing")
            .Should().BeOfType<ObjectResult>().Subject;

        obj.StatusCode.Should().Be(400);
        var body = obj.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        body.Success.Should().BeFalse();
        body.Message.Should().Be("bad");
        body.ErrorType.Should().Be("ValidationError");
        body.Detail.Should().Be("name missing");
        body.TraceId.Should().Be("trace-1");
        body.Instance.Should().Be("/api/orders");
        body.Data.Should().BeNull();
    }

    [Fact]
    public void ApiFail_CustomStatusCode_UsesItOnResultAndBody()
    {
        var sut = CreateController(CreateContext("GET"));

        var obj = sut.Fail("missing", 404).Should().BeOfType<ObjectResult>().Subject;

        obj.StatusCode.Should().Be(404);
        obj.Value.Should().BeOfType<ApiResponse<object>>().Which.StatusCode.Should().Be(404);
    }

    // ---- ApiResponseHelper.CreateSuccess ----

    [Theory]
    [InlineData("GET", 200)]
    [InlineData("PUT", 200)]
    [InlineData("PATCH", 200)]
    [InlineData("DELETE", 204)]
    [InlineData("get", 200)]
    public void CreateSuccess_MethodWithData_InfersStatusCode(string method, int expected)
    {
        var result = ApiResponseHelper.CreateSuccess("data", CreateContext(method), "ok");

        result.StatusCode.Should().Be(expected);
        result.Success.Should().BeTrue();
        result.Message.Should().Be("ok");
        result.Instance.Should().BeNull();
    }

    [Fact]
    public void CreateSuccess_GetWithNullData_ReturnsA404Failure()
    {
        var result = ApiResponseHelper.CreateSuccess<string?>(null, CreateContext("GET"), "order 7");

        result.StatusCode.Should().Be(404);
        result.Success.Should().BeFalse();
        result.Message.Should().Be("order 7");
        result.TraceId.Should().Be("trace-1");
    }

    [Fact]
    public void CreateSuccess_PostWithoutResourceId_Returns200WithoutLocation()
    {
        var result = ApiResponseHelper.CreateSuccess("data", CreateContext("POST"));

        result.StatusCode.Should().Be(200);
        result.Instance.Should().BeNull();
    }

    [Fact]
    public void CreateSuccess_PostWithResourceId_Returns201AndLocation()
    {
        var result = ApiResponseHelper.CreateSuccess("data", CreateContext("POST", "/api/items/"), resourceId: 42);

        result.StatusCode.Should().Be(201);
        result.Instance.Should().Be("https://example.com/api/items/42");
    }

    [Fact]
    public void CreateSuccess_NullContext_Throws()
    {
        var act = () => ApiResponseHelper.CreateSuccess("x", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ---- ApiResponseHelper.CreateFail ----

    [Theory]
    [InlineData("GET", 404)]
    [InlineData("POST", 400)]
    [InlineData("DELETE", 400)]
    public void CreateFail_InfersStatusCodeFromMethod(string method, int expected)
    {
        var result = ApiResponseHelper.CreateFail<string>("nope", CreateContext(method), "Err", "detail");

        result.StatusCode.Should().Be(expected);
        result.Success.Should().BeFalse();
        result.Message.Should().Be("nope");
        result.ErrorType.Should().Be("Err");
        result.Detail.Should().Be("detail");
        result.TraceId.Should().Be("trace-1");
    }

    [Fact]
    public void CreateFail_NullContext_Throws()
    {
        var act = () => ApiResponseHelper.CreateFail<string>("x", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ---- ApiResponseExtensions.ToActionResult ----

    [Fact]
    public void ToActionResult_WithInstance_AppendsLocationHeader()
    {
        var context = CreateContext("POST");
        var response = new ApiResponse<string> { StatusCode = 201, Instance = "https://example.com/api/items/1" };

        var result = response.ToActionResult(context);

        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(201);
        context.Response.Headers["Location"].ToString().Should().Be("https://example.com/api/items/1");
    }

    [Fact]
    public void ToActionResult_LocationAlreadySet_KeepsExistingHeader()
    {
        var context = CreateContext("POST");
        context.Response.Headers["Location"] = "/existing";
        var response = new ApiResponse<string> { StatusCode = 201, Instance = "/new" };

        response.ToActionResult(context);

        context.Response.Headers["Location"].ToString().Should().Be("/existing");
    }

    [Fact]
    public void ToActionResult_NoInstance_DoesNotSetLocation()
    {
        var context = CreateContext("GET");
        var response = new ApiResponse<string> { StatusCode = 200 };

        response.ToActionResult(context);

        context.Response.Headers.ContainsKey("Location").Should().BeFalse();
    }

    [Fact]
    public void ToActionResult_NullContext_ReturnsResultWithStatusCode()
    {
        var response = new ApiResponse<string> { StatusCode = 202, Instance = "/x" };

        var result = response.ToActionResult();

        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(202);
        obj.Value.Should().BeSameAs(response);
    }

    // ---- ApiResponse defaults ----

    [Fact]
    public void ApiResponse_Defaults_AreSuccessWithEmptyMessage()
    {
        var response = new ApiResponse<int>();

        response.Success.Should().BeTrue();
        response.Message.Should().BeEmpty();
        response.StatusCode.Should().Be(0);
        response.Data.Should().Be(0);
    }

    // ---- Helpers ----

    [Fact]
    public void GetClientIpAddress_NoRemoteIp_ReturnsUnknown()
    {
        CreateContext("GET").GetClientIpAddress().Should().Be("unknown");
    }

    [Fact]
    public void GetClientIpAddress_WithRemoteIp_ReturnsIt()
    {
        var context = CreateContext("GET");
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.1.2.3");

        context.GetClientIpAddress().Should().Be("10.1.2.3");
    }

    [Fact]
    public void CreateLocationHeader_AppendsIdAndTrimsTrailingSlash()
    {
        var context = CreateContext("POST", "/api/items/");

        LocationHelper.CreateLocationHeader(context.Request, 7).Should().Be("https://example.com/api/items/7");
    }

    [Fact]
    public void CreateLocationHeader_NullRequest_Throws()
    {
        var act = () => LocationHelper.CreateLocationHeader(null!, 1);

        act.Should().Throw<ArgumentNullException>();
    }
}
