using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        public async Task<IActionResult> UploadCompressWrapper([FromForm] FileUploadInput input)
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

            var temp1 = new CompressFileUploadOutput
            {
                FileName = "测试压缩包临时文件1.pdf",
                FileMd5 = "d766fb30e5484f679f7fc94d2eceaf51",
                FileUrl = "http://store.yitu666.com:8880/group1/M00/00/5C/oYYBAFzHrdGAOT1-AEP5I0vi7W0983.pdf",
                FolderPath = null
            };
            var temp2 = new CompressFileUploadOutput
            {
                FileName = "测试压缩包临时文件2.docx",
                FileMd5 = "3c603ad8a4a8f57105d67f97fe227baa",
                FileUrl = "http://store.yitu666.com:8880/group1/M00/00/5F/ooYBAFzOlwqAfFQdAAcMrX9e-bg94.docx",
                FolderPath = null
            };

            var lst = new List<CompressFileUploadOutput> {temp1, temp2};
            return Json(new ApiResult<List<CompressFileUploadOutput>>(lst));
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


        /// <summary>
        /// 批量打包下载
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Download([FromBody] FilesDownloadModel input)
        {
            if (string.IsNullOrEmpty(input.FileName) || !(input.Files?.Count > 0))
            {
                return BadRequest(ApiResult.Failed("参数无效"));
            }

            if (Regex.IsMatch(input.FileName, "[\\\\/:*?\"<>|&]"))
            {
                return BadRequest(ApiResult.Failed("filename含有非法字符"));
            }

            if (input.Files.Any(x => string.IsNullOrWhiteSpace(x.Value) 
                                     || Regex.IsMatch(x.Key, "[\\:*?\"<>|]")))
            {
                return BadRequest(ApiResult.Failed("files中的文件名含有非法字符"));
            }


            var zipFile = await _fileStorageService.DownloadAsync(input.Files);

            return File(System.IO.File.OpenRead(zipFile), "application/zip", input.FileName);
        }

        /// <summary>
        /// 批量打包下载
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Download(string fileName, [FromQuery]Dictionary<string, string> files)
        {
            return await Download(new FilesDownloadModel
            {
                FileName = fileName,
                Files = files
            });
        }
    }
}