using System.Reflection;
using api.Services;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace api.UnitTests.Services;

public class ImageServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IImageService _imageService;

    public ImageServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        IWebHostEnvironment environment = Substitute.For<IWebHostEnvironment>();
        environment.ContentRootPath.Returns("../../../Services/"); //TODO: find better way to get path

        _imageService = new ImageService(environment);
    }

    [Fact]
    public void SaveImageAsync_GivenValidFormFile_ReturnCorrectFileName()
    {
        // TODO: implement test
    }

    [Fact]
    public void DeleteImage_GivenExistingImageName_SuccessfullyDeleteImage()
    {
        // TODO: implement test
    }
}