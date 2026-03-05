using System.IO;
using System.Text;
using System.Threading.Tasks;
using Domain;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;

namespace Controller.Test;

[TestClass]
public class PlugInControllerTest
{
        [TestMethod]
    public async Task Upload_ReturnsOk_OnSuccess()
    {
        // Arrange
        var pluginBytes = Encoding.UTF8.GetBytes("dummy dll bytes");
        var formFile = CreateFormFile(pluginBytes, "Plugins.dll", "application/octet-stream");

        var mockLogic = new Mock<IPlugInLogic>();
        mockLogic
            .Setup(l => l.UploadAsync(It.IsAny<Stream>(), "Plugins.dll", "my-name"))
            .ReturnsAsync(new ScoringStrategyMeta
            {
                Name = "my-name",
                FileName = "Plugins.dll",
                FilePath = "/app/Plugins/xxx/Plugins.dll",
                IsActive = false,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });

        var controller = new PlugInController(mockLogic.Object);

        // Act
        var result = await controller.Upload(formFile, "my-name");

        // Assert
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok, "Expected OkObjectResult");
        Assert.AreEqual(200, ok.StatusCode ?? 200);
        mockLogic.Verify(l => l.UploadAsync(It.IsAny<Stream>(), "Plugins.dll", "my-name"), Times.Once);
    }

    [TestMethod]
    public async Task Upload_ReturnsBadRequest_WhenFileMissingOrEmpty()
    {
        // Arrange
        var mockLogic = new Mock<IPlugInLogic>();
        var controller = new PlugInController(mockLogic.Object);

        // Act
        var result = await controller.Upload(plugin: null, name: null);

        // Assert
        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad, "Expected BadRequestObjectResult");
        Assert.AreEqual(400, bad.StatusCode ?? 400);

        mockLogic.Verify(l => l.UploadAsync(
            It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var ms = new MemoryStream(content);
        return new FormFile(ms, 0, content.Length, "plugin", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [TestMethod]
public async Task Upload_ReturnsBadRequest_WhenFileEmpty()
{
    var mockLogic = new Mock<IPlugInLogic>(MockBehavior.Strict);
    var controller = new PlugInController(mockLogic.Object);

    var emptyFile = new FormFile(new MemoryStream(Array.Empty<byte>()), 0, 0, "plugin", "empty.dll");

    var result = await controller.Upload(emptyFile, "whatever");

    var bad = result as BadRequestObjectResult;
    Assert.IsNotNull(bad);
    var pd = bad.Value as ProblemDetails;
    Assert.IsNotNull(pd);
    StringAssert.Contains(pd.Title!, "Invalid upload");
    mockLogic.VerifyNoOtherCalls();
}

    [TestMethod]
    public async Task Upload_ReturnsBadRequest_On_InvalidOperationException()
    {
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "plugin", "x.dll");
        var mockLogic = new Mock<IPlugInLogic>();
        mockLogic.Setup(l => l.UploadAsync(It.IsAny<Stream>(), "x.dll", "name"))
                 .ThrowsAsync(new InvalidOperationException("bad plugin"));

        var controller = new PlugInController(mockLogic.Object);

        var result = await controller.Upload(file, "name");

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad);
        var pd = bad.Value as ProblemDetails;
        Assert.IsNotNull(pd);
        Assert.AreEqual("Invalid plugin", pd.Title);
        StringAssert.Contains(pd.Detail!, "bad plugin");
    }

    [TestMethod]
    public async Task Upload_ReturnsBadRequest_On_BadImageFormatException()
    {
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "plugin", "x.dll");
        var mockLogic = new Mock<IPlugInLogic>();
        mockLogic.Setup(l => l.UploadAsync(It.IsAny<Stream>(), "x.dll", "name"))
                 .ThrowsAsync(new BadImageFormatException("not a managed dll"));

        var controller = new PlugInController(mockLogic.Object);

        var result = await controller.Upload(file, "name");

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad);
        var pd = bad.Value as ProblemDetails;
        Assert.IsNotNull(pd);
        Assert.AreEqual("Invalid DLL", pd.Title);
        StringAssert.Contains(pd.Detail!, "not a managed dll");
    }
}
