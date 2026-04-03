using CorpAI.Gateway.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CorpAI.Gateway.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var httpFactory = new Mock<IHttpClientFactory>();
        var config = new Mock<IConfiguration>();
        var logger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(httpFactory.Object, config.Object, logger.Object);
    }

    [Test]
    public void GetStatus_ShouldReturnOk()
    {
        var result = _controller.GetStatus();
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
}