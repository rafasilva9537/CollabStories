using System.Collections.Immutable;
using api.Constants;
using api.Interfaces;

namespace api.Services;

public class ImageService : IImageService
{
    public static readonly ImmutableArray<string> ImgExtensions = [ ".jpg", ".png", ".jpeg" ];
    private readonly string _imagesPath = Path.Combine(DirectoryPathConstants.Media, DirectoryPathConstants.Images);
    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment)
    {

        _environment = environment;
    }

    public async Task<string> SaveImageAsync(IFormFile image, string directoryName)
    {
        string rootPath = _environment.ContentRootPath;
        string imageDirectoryPath = Path.Combine(rootPath, _imagesPath, directoryName);

        if(!Directory.Exists(imageDirectoryPath))
        {
            Directory.CreateDirectory(imageDirectoryPath);
        }

        string untrustedFileName = image.FileName;
        string extension = Path.GetExtension(untrustedFileName);

        if(!ImgExtensions.Contains(extension))
        {
            throw new ArgumentOutOfRangeException($"Only files with extensions {string.Join(", ", ImgExtensions)}are allowed");
        }

        string imageName = $"{Guid.NewGuid()}{extension}";
        string imageNameWithPath = Path.Combine(imageDirectoryPath, imageName);
        
        await using FileStream imageStream = new(imageNameWithPath, FileMode.CreateNew);
        await image.CopyToAsync(imageStream);

        return imageName;
    }

    public FileStream GetImage(string imageName, string directoryName)
    {
        string rootPath = _environment.ContentRootPath;
        string imageDirectoryPath = Path.Combine(rootPath, _imagesPath, directoryName);
        string imageNameWithPath = Path.Combine(imageDirectoryPath, imageName);
        
        FileStream imageStream = File.OpenRead(imageNameWithPath);
        return imageStream;
    }

    public void DeleteImage(string imageName, string directoryName)
    {
        string rootPath = _environment.ContentRootPath;
        string imageDirectoryPath = Path.Combine(rootPath, _imagesPath, directoryName);
        if(!Directory.Exists(imageDirectoryPath)) throw new DirectoryNotFoundException("Directory does not exist. Unable to delete.");
        
        string imageNameWithPath = Path.Combine(imageDirectoryPath, imageName);
        if(!File.Exists(imageNameWithPath)) throw new FileNotFoundException("File does not exist. Unable to delete.");
        
        File.Delete(imageNameWithPath);
    }

}