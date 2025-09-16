using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TestProject.Services;

namespace TestProject.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class TestController(FileBrowsingService fileBrowsingService) : ControllerBase {

        private readonly FileBrowsingService _fileBrowsingService = fileBrowsingService;

        [HttpGet]
        public string Get(string? currentDirectory = null) {
            var fileBrowsingResults = _fileBrowsingService.GetBrowsingResults(currentDirectory);
            return JsonSerializer.Serialize(fileBrowsingResults);
        }

        [HttpPost]
        [Route(nameof(Upload))]
        public async Task<bool> Upload(
            IFormFile file,
            [FromForm] string? currentDirectory = null)
        {
            return await _fileBrowsingService.UploadFile(file, currentDirectory);
        }
    }
}