using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileStorage.Application;
using FileStorage.Core;
using FileStorageApi.Controllers.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileStorageApi.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly UploadOption _uploadOption;

        public FileController(IFileStorageService fileStorageService, IOptions<UploadOption> optionAccessor)
        {
            _fileStorageService = fileStorageService;
            _uploadOption = optionAccessor.Value;
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

            if (Request.Form.Files.Count > _uploadOption.MaxUpload)
            {
                return BadRequest(ApiResult.Failed($"超过单次上传数量限制：{_uploadOption.MaxUpload}"));
            }

            if (string.IsNullOrWhiteSpace(input.Extension)
                && Request.Form.Files.Any(x => string.IsNullOrWhiteSpace(x.FileName)
                                               || string.IsNullOrWhiteSpace(Path.GetExtension(x.FileName))))
            {
                return BadRequest(ApiResult.Failed("请手动设置文件扩展名"));
            }

            var tasks = Request.Form.Files
                .Select(file =>
                {
                    string ext = null;
                    if (!string.IsNullOrWhiteSpace(file.FileName))
                    {
                        ext = Path.GetExtension(file.FileName);
                    }
                    var model = new UploadFileModel(file.OpenReadStream(),
                        ext ?? input.Extension,
                        input.Group);
                    return _fileStorageService.UploadAsync(model);
                })
                .ToArray();
            await Task.WhenAll(tasks);

            return Json(new ApiResult<List<string>>(tasks.Select(x => x.Result).ToList()));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(ApiResult.Failed("Url不能为空"));
            }
            await _fileStorageService.DeleteAsync(url);
            return Ok(ApiResult.Succeed());
        }

        public IActionResult Status()
        {
            return Ok();
        }
    }
}