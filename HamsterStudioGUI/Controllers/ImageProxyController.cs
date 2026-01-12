using Microsoft.AspNetCore.Mvc;

namespace HamsterStudioGUI.Controllers
{
    [ApiController]
    [Route("api")]
    public class ImageProxyController : ControllerBase
    {
        [HttpGet("image-proxy")]
        public IActionResult GetImage([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL parameter is required.");
            }
            try
            {
                var uri = new Uri(url);
                var client = new System.Net.Http.HttpClient();
                var response = client.GetAsync(uri).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve image.");
                }
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                var imageData = response.Content.ReadAsByteArrayAsync().Result;
                return File(imageData, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving image: {ex.Message}");
            }
        }



    }
}
