using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileStorage.Application;
using FileStorage.Core;
using FileStorageApi.Compress;
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

        /// <summary>
        /// 上传压缩包
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadPackage([FromForm] FileUploadInput input)
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

            return await DecompressionUpload(input);
        }

        /// <summary>
        /// 解压文件并上传
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private async Task<IActionResult> DecompressionUpload(FileUploadInput input)
        {
            string tempDir = null;
            try
            {
                tempDir = FilePathExtention.CreateTimeRandomDir(Path.GetTempPath());

                var lstFile = await new CompressTool(tempDir).Decompression(Request.Form.Files);
                var tasks = lstFile
                    .Select(async file =>
                    {
                        var ext = Path.GetExtension(file.FileName);
                        using (var stream = System.IO.File.OpenRead(file.TempFilePath))
                        {
                            var model = new UploadFileModel(stream, ext, input.Group);
                            file.FileUrl = await _fileStorageService.UploadAsync(model);
                        }
                        //上传成功，删除临时文件
                        System.IO.File.Delete(file.FileName);
                    }).ToArray();
                await Task.WhenAll(tasks);

                return Json(new ApiResult<List<CompressFileUploadOutput>>(lstFile));
            }
            catch(FormatException ex)
            {
                return Json(new ApiResult<string>(ex.Message));
            }
            finally
            {
                if (tempDir != null && Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
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
            if (Regex.IsMatch(input.FileName, @"[\\\/:*?""<>|&]"))
            {
                return BadRequest(ApiResult.Failed("filename含有非法字符"));
            }
            //英文 ? 作为嵌套压缩包的识别，可以有
            //为防止 / \ 路径分隔符在key中不同，但实际为同一个文件的情况，统一使用 / 作为路径分隔符
            if (input.Files.Any(x => string.IsNullOrWhiteSpace(x.Value) 
                                     || Regex.IsMatch(x.Key, @"[\\:*""<>|]"))) 
            {
                return BadRequest(ApiResult.Failed("files中的文件名含有非法字符"));
            }
            if (input.Files.Any(x => Regex.IsMatch(x.Key, @"\?[^\/-\/]")   //英文 ? 压缩包作为路径层级，后面必须跟 /
                                     || Regex.IsMatch(x.Key, @"\/[ \/]")   //斜杠 / 后面不能有空格或者 /
                                     || Regex.IsMatch(x.Key, @" [\/\?]")   //斜杠 / 和 ? 前面不能有空格
                                     || x.Key.StartsWith('?') || x.Key.EndsWith('?')
                                     || x.Key.StartsWith('/') || x.Key.EndsWith('/') 
                                     || x.Key.StartsWith(' ') || x.Key.EndsWith(' ')))
            {
                return BadRequest(ApiResult.Failed("files中的文件名含有非法路径"));
            }

            var zipFile = await _fileStorageService.DownloadAsync(input.Files);
            var fileName = input.FileName;
            if (!fileName.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
            {
                fileName += ".zip"; //只支持zip压缩
            }

            return File(System.IO.File.OpenRead(zipFile), "application/zip", fileName);
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