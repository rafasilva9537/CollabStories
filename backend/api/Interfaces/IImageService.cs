namespace api.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile image, string directoryName);
    void DeleteImage(string imageName, string directoryName);
}