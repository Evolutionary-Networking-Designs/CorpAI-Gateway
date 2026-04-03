using CorpAI.Gateway.Services;
using NUnit.Framework;

namespace CorpAI.Gateway.Tests.Services;

[TestFixture]
public class ThemeGeneratorServiceTests
{
    [Test]
    public void GetCssFileName_ShouldReturnValidFormat()
    {
        var service = new ThemeGeneratorService();
        Assert.That(service.GetCssFileName(), Does.Match(@"^theme\.[a-f0-9]{8}\.css$"));
    }

    [Test]
    public void GetCssContent_ShouldContainRootVariables()
    {
        var service = new ThemeGeneratorService();
        Assert.That(service.GetCssContent(), Does.Contain(":root"));
    }
}