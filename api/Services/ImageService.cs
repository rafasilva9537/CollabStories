using api.Constants;

namespace api.Services;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile image, string directoryName);
    void DeleteImage(string imageName, string directoryName);
}

public class ImageService : IImageService
{
    // TODO: change to some immutable collection
    private string[] _imgExtensions = [ ".jpg", ".png", ".jpeg" ];
    private readonly string _imagesPath = Path.Combine(DirectoryPathConstants.Media, DirectoryPathConstants.Images);
    private readonly IWebHostEnvironment _environment;

    public ImageService(IWebHostEnvironment environment)
    {

        _environment = environment;
    }

    public async Task<string> SaveImageAsync(IFormFile image, string directoryName)
    {
        var rootPath = _environment.ContentRootPath;
        string imageDirectoryPath = Path.Combine(rootPath, _imagesPath, directoryName);

        if(!Directory.Exists(imageDirectoryPath))
        {
            Directory.CreateDirectory(imageDirectoryPath);
        }

        string untrustedFileName = image.FileName;
        string extension = Path.GetExtension(untrustedFileName);

        if(!_imgExtensions.Contains(extension))
        {
            throw new ArgumentOutOfRangeException($"Only files with extensions {string.Join(", ", _imgExtensions)}are allowed");
        }

        string imageName = $"{Guid.NewGuid()}{extension}";
        string imageNameWithPath = Path.Combine(imageDirectoryPath, imageName);

        // TODO: handle if the file already exists (IOException)
        using(FileStream imageStream = new FileStream(imageNameWithPath, FileMode.CreateNew))
        {
            await image.CopyToAsync(imageStream);
        }

        return imageName;
    }

    public void DeleteImage(string imageName, string directoryName)
    {
        var rootPath = _environment.ContentRootPath;
        string imageDirectoryPath = Path.Combine(rootPath, _imagesPath, directoryName);
        string imageNameWithPath = Path.Combine(imageDirectoryPath, imageName);

        if(!File.Exists(imageNameWithPath))
        {
            throw new FileNotFoundException($"File does not exists. Unable to delete.");
        }
        
        File.Delete(imageNameWithPath);
    }

}