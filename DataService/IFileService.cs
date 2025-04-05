namespace FormulaOne.ChatService.DataService
{
    public interface IFileService
    {
        Task<string?> UploadRoomImageAsync(IFormFile file, int roomId);
        string GetImageUrl(int roomId, string extension);
    }

}
