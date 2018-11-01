using System.IO;
using System.Threading.Tasks;
using FileStorage.Application;
using FileStorage.Core;
using FileStorageApi.Controllers.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApi.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileStorageService _fileStorageService;

        public FileController(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] FileUploadInput input)
        {
            if (Request.Form.Files == null || Request.Form.Files.Count == 0)
            {
                return BadRequest(ApiResult.Failed("未上传有效文件"));
            }

            if (string.IsNullOrWhiteSpace(input.Extension))
            {
                if (string.IsNullOrWhiteSpace(Request.Form.Files[0].FileName))
                {
                    return BadRequest(ApiResult.Failed("请手动设置文件扩展名"));
                }
                var ext = Path.GetExtension(Request.Form.Files[0].FileName);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    return BadRequest(ApiResult.Failed("请手动设置文件扩展名"));
                }

                input.Extension = ext;
            }

            var model = new UploadFileModel(Request.Form.Files[0].OpenReadStream(),
                input.Extension,
                input.Group);

            var url = await _fileStorageService.UploadAsync(model);

            return Json(new ApiResult<string>(url));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest();
            }
            await _fileStorageService.DeleteAsync(url);
            return Ok();
        }

        public IActionResult Status()
        {
            return Ok();
        }
    }
}