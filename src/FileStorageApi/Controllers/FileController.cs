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

            var tasks = Request.Form.Files
                .Select(file =>
                {
                    if (string.IsNullOrWhiteSpace(input.Extension))
                    {
                        if (string.IsNullOrWhiteSpace(file.FileName))
                        {
                            throw new UserFriendlyException("请手动设置文件扩展名");
                        }

                        var ext = Path.GetExtension(file.FileName);
                        if (string.IsNullOrWhiteSpace(ext))
                        {
                            throw new UserFriendlyException("请手动设置文件扩展名");
                        }

                        input.Extension = ext;
                    }

                    var model = new UploadFileModel(Request.Form.Files[0].OpenReadStream(),
                        input.Extension,
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