namespace TestProject.Services
{
    public class FileBrowsingService(
        IConfiguration configuration,
        ILogger<FileBrowsingService> logger)
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<FileBrowsingService> _logger = logger;

        public record FileResult
        {
            public string Name { get; set; } = string.Empty;
            public long Length { get; set; }
            public string Url { get; set; } = string.Empty;
        }

        public record BrowsingResult
        {
            public IEnumerable<FileResult> Directories { get; set; } = [];
            public IEnumerable<FileResult> Files { get; set; } = [];
        }

        public BrowsingResult GetBrowsingResults(string? currentDirectory = null)
        {
            try
            {
                var homeDirectory = GetHomeDirectory(currentDirectory);
                var directoryInfo = new DirectoryInfo(homeDirectory);

                // It might be more efficient to use `EnumerateFileSystemInfos` and grab files/directories in 1 pass, but `FileSystemInfo` doesn't seem to have `Length`
                // information, so I went with this method instead.
                var files = directoryInfo.EnumerateFiles()
                    .OrderBy(fileInfo => fileInfo.Name)
                    .Select(fileInfo => new FileResult
                    {
                        Name = fileInfo.Name,
                        Length = fileInfo.Length,
                        Url = Path.Combine("/files", Path.GetRelativePath(_configuration["HomeDirectory"]!, fileInfo.FullName)),
                    });
                var directories = directoryInfo.EnumerateDirectories()
                    .OrderBy(fileInfo => fileInfo.Name)
                    .Select(fileInfo => new FileResult
                    {
                        Name = fileInfo.Name,
                        // Already validated this with homeDirectory, so assert it's not null with `!`.
                        // Also replace `\\` with `/`. Javascript seems to convert the `\\` character to `\` and breaks pathing for certain directories.
                        Url = Path.GetRelativePath(_configuration["HomeDirectory"]!, fileInfo.FullName).Replace("\\", "/"),
                    });

                return new BrowsingResult
                {
                    Directories = directories,
                    Files = files,
                };
            }
            catch (Exception e)
            {
                _logger.LogError("Error getting files: {}", e);
                throw;
            }
        }

        public async Task<bool> UploadFile(
            IFormFile file,
            string? currentDirectory = null)
        {
            var homeDirectory = GetHomeDirectory(currentDirectory);

            if (file.Length > 0)
            {
                try
                {
                    var path = Path.Combine(homeDirectory, file.FileName);
                    using var stream = File.Create(path);

                    await file.CopyToAsync(stream);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error uploading file: {}", e);
                    throw;
                }
            }

            return false;
        }

        public string GetHomeDirectory(string? currentDirectory = null)
        {
            // This check probably isn't needed since it's checked in Startup.cs, but if hot reloading is ever implemented there's
            // a chance it could be null, so might as well check.
            var homeDirectory = _configuration["HomeDirectory"] ?? throw new ArgumentNullException(nameof(currentDirectory));
            if (!string.IsNullOrWhiteSpace(currentDirectory))
            {
                homeDirectory = Path.Combine(homeDirectory, currentDirectory);
            }

            return homeDirectory;
        }
    }
}
