namespace MyMvcApp.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> SaveFileAsync(IFormFile? file, string webRootPath, string? existingFileUrl = null)
        {
            if (file == null || file.Length == 0)
                return existingFileUrl ?? string.Empty;

            var uploadsPath = Path.Combine(webRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (!string.IsNullOrEmpty(existingFileUrl))
            {
                var oldFilePath = Path.Combine(webRootPath, existingFileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            return "/uploads/" + fileName;
        }

        public static void DeleteFile(string webRootPath, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            var fullPath = Path.Combine(webRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}