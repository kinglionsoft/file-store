using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileStorageApi.Controllers.Dto;
using Microsoft.AspNetCore.Http;
using SharpCompress.Archives;
using SharpCompress.Readers;

namespace FileStorageApi.Compress
{
    public class CompressTool
    {
        private readonly string _tempDir;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempDir">解压/压缩的临时目录</param>
        public CompressTool(string tempDir)
        {
            _tempDir = tempDir;
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="formFiles">上传的文件</param>
        /// <returns></returns>
        public async Task<List<CompressFileUploadOutput>> Decompression(IFormFileCollection formFiles)
        {
            var allIsPackage = formFiles.All(file => IsPackage(Path.GetExtension(file.FileName).ToLower()));
            if (!allIsPackage)
            {
                throw new FormatException("Unsupported format exists in the files.\r\nOnly rar, zip, 7z are supported.");
            }

            var tasks = formFiles.Select(file =>
            {
                return Task.Run(() =>
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var compressFolder = $"{Path.GetFileName(file.FileName)}?";
                        return ArchiveStreamRead(stream, null, compressFolder);
                    }
                });
            }).ToArray();
            await Task.WhenAll(tasks);

            return tasks.SelectMany(task => task.Result).ToList();
        }

        protected async Task<List<CompressFileUploadOutput>> ArchiveFileRead(string filePath, ReaderOptions readerOptions = null, 
            string compressFolder = null)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return await ArchiveStreamRead(stream, readerOptions, compressFolder);
            }
        }

        protected async Task<List<CompressFileUploadOutput>> ArchiveStreamRead(Stream stream, 
            ReaderOptions readerOptions = null, string compressFolder = null)
        {
            var allFileInfo = new List<CompressFileUploadOutput>();
            using (var archive = ArchiveFactory.Open(stream, readerOptions))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.IsDirectory)
                    {
                        continue;
                    }

                    var entryFileInfos = await ArchiveEntryRead(entry, readerOptions, compressFolder);
                    allFileInfo.AddRange(entryFileInfos);
                }
            }

            return allFileInfo;
        }

        private async Task<List<CompressFileUploadOutput>> ArchiveEntryRead(IArchiveEntry entry, ReaderOptions readerOptions, 
            string compressFolder)
        {
            var lstFileInfo = new List<CompressFileUploadOutput>();
            var entryKey = entry.Key;

            var fileExt = Path.GetExtension(entryKey).ToLower();
            var tempFilePath = Path.Combine(_tempDir, $"{Guid.NewGuid().ToString("N").ToUpper()}{fileExt}");
            entry.WriteToFile(tempFilePath);

            var fileFolder = CombinePath(compressFolder, Path.GetDirectoryName(entryKey));
            if (IsPackage(fileExt))
            {
                //压缩包的文件名作为一层目录结构，并在后面加 ? 用于表示该一层目录为压缩包，主要为了区分文件夹名为test.rar情况
                var childerFolder = CombinePath(fileFolder, $"{Path.GetFileName(entryKey)}?");

                var childFileInfos = await ArchiveFileRead(tempFilePath, readerOptions, childerFolder);
                lstFileInfo.AddRange(childFileInfos);

                return lstFileInfo;
            }

            var uploadFileInfo = new CompressFileUploadOutput
            {
                TempFilePath = tempFilePath,
                FileName = Path.GetFileName(entryKey),
                FolderPath = fileFolder?.Replace('\\', '/'), //层级机构统一使用 / ，不同的压缩格式，解压的路径Key分隔符不同
                FileMd5 = CalcMd5(tempFilePath)
            };
            lstFileInfo.Add(uploadFileInfo);

            return lstFileInfo;
        }

        private static bool IsPackage(string fileExt)
        {
            if (fileExt.StartsWith(".zip") || fileExt.StartsWith(".rar") || fileExt.StartsWith(".7z"))
            {
                return true;
            }
            var arrTarExt = new []{ ".tar" , ".gz" , ".bz" , ".xz", ".wim", ".lzh" };
            if (arrTarExt.Any(fileExt.StartsWith))
            {
                throw new FormatException("The package or internal package format is not supported.\r\nOnly rar, zip, 7z are supported.");
            }

            return false;
        }

        private static string CalcMd5(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static string CombinePath(string parentPath, string childPath)
        {
            if (string.IsNullOrEmpty(parentPath))
            {
                return childPath;
            }
            if (string.IsNullOrEmpty(childPath))
            {
                return parentPath;
            }
            return Path.Combine(parentPath, childPath);
        }
    }
}
