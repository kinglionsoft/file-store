using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileStorage.Core;

namespace FileStorage.Application
{
    public class FileStorageService : IFileStorageService
    {
        private const string ZipExt = ".zip";
        private const char CompressIdentifier = '?';

        private readonly IStorageProvider _storageProvider;
        private readonly IFastDfsHttpClient _fastDfsHttpClient;

        public FileStorageService(IStorageProvider storageProvider, IFastDfsHttpClient fastDfsHttpClient)
        {
            _storageProvider = storageProvider;
            _fastDfsHttpClient = fastDfsHttpClient;
        }

        public Task<string> UploadAsync(UploadFileModel model, CancellationToken token = default)
        {
            return _storageProvider.UploadAsync(model, token);
        }

        public Task DeleteAsync(string url, CancellationToken token = default)
        {
            return _storageProvider.DeleteFileAsync(url, token);
        }

        /// <summary>
        /// 批量打包下载
        /// </summary>
        /// <param name="input">{"path": "url"}</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<string> DownloadAsync(Dictionary<string, string> input, CancellationToken token = default)
        {
            var rootFile = ReadPathLevel(input);

            var tmpFile = Path.GetTempFileName();
            using (var zip = ZipFile.Open(tmpFile, ZipArchiveMode.Update))
            {
                await CompressZip(zip, rootFile, token);
            }

            return tmpFile;
        }

        #region 压缩

        /// <summary>
        /// 读取压缩包、文件夹、嵌套压缩包的路径层级结构，返回根压缩包文件信息
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private static MyCompressFile ReadPathLevel(Dictionary<string, string> dic)
        {
            var rootPackage = new MyCompressFile("root.zip", true);
            foreach (var keyPair in dic)
            {
                var parentPackage = rootPackage;
                //去掉开始的斜杠，否则会多一级空目录
                var arrPath = keyPair.Key.Split(CompressIdentifier).Select(o=> o.TrimStart('/', '\\')).ToArray();
                //前面为上级嵌套压缩包路径，最后一份为文件相对上级压缩包的路径
                var fileRelativePath = arrPath[arrPath.Length - 1];
                for (int i = 0; i < arrPath.Length - 1; i++) 
                {
                    var packageRelativePath = arrPath[i];
                    if (!parentPackage.InternalFiles.TryGetValue(packageRelativePath, out var subPackage))
                    {
                        subPackage = new MyCompressFile(packageRelativePath, true);
                        parentPackage.InternalFiles.Add(packageRelativePath, subPackage);
                    }

                    parentPackage = subPackage;
                }
                var file = new MyCompressFile(fileRelativePath) {FileUrl = keyPair.Value};

                parentPackage.InternalFiles.Add(fileRelativePath, file);
            }
            //如果根压缩包下仅有一个文件且为压缩包，则去掉最外层根压缩包壳
            if (rootPackage.InternalFiles.Count == 1 && rootPackage.InternalFiles.First().Value.IsPackage)
            {
                rootPackage = rootPackage.InternalFiles.First().Value;
            }

            return rootPackage;
        }

        private async Task CompressZip(ZipArchive zip, MyCompressFile compressFile, CancellationToken token = default)
        {
            foreach (var internalFilePair in compressFile.InternalFiles)
            {
                var entryKey = internalFilePair.Key;
                var internalFile = internalFilePair.Value;
                if (internalFile.IsPackage)
                {
                    //如果是压缩包，获取有效的Key
                    entryKey = GetValidEntryKey(compressFile, entryKey);

                    var tmpFile = Path.GetTempFileName();
                    using (var childZip = ZipFile.Open(tmpFile, ZipArchiveMode.Update))
                    {
                        await CompressZip(childZip, internalFile, token);
                    }
                    internalFile.TempFilePath = tmpFile;
                }
                var entry = zip.CreateEntry(entryKey);
                using (var stream = internalFile.IsPackage //如果是压缩包，则读取文件；否则直接读取下载的文件流
                    ? File.OpenRead(internalFile.TempFilePath) 
                    : await _fastDfsHttpClient.DownloadAsync(internalFile.FileUrl, token))
                {
                    using (var entryStream = entry.Open())
                    {
                        await stream.CopyToAsync(entryStream);
                    }
                }
                //子压缩包数据已经写入父压缩包后，删除子压缩包文件
                if (internalFile.IsPackage)
                {
                    File.Delete(internalFile.TempFilePath); 
                }
            }
        }

        private static string GetValidEntryKey(MyCompressFile compressFile, string entryKey)
        {
            if (entryKey.EndsWith(ZipExt, StringComparison.InvariantCultureIgnoreCase))
            {
                return entryKey;
            }
            //如果是压缩包，且不是zip结尾，则在后缀添加.zip（不能直接改后缀，存在同名的rar、7z、zip压缩包或文件夹）
            var renameKey = entryKey + ZipExt;
            //添加.zip后，如果存在同名，需要区分（极端情况处理，及其少见）
            //abc.rar文件  abc.7z文件  abc.zip文件  abc.rar.zip文件夹  abc.7z.zip文件 (同级目录存在这5个文件时的类似极端情况)
            //不仅需要判断同层级文件名相同，还需要判断同层次是否存在同名文件夹
            int i = 0;
            while (compressFile.InternalFiles.ContainsKey(renameKey) || 
                   compressFile.InternalFiles.Keys.Any(key => key.StartsWith($"{renameKey}/") //判断同层次文件夹是否相同
                                                              || key.StartsWith($"{renameKey}\\")))
            {
                i += 1;
                renameKey = $"{entryKey}重名区分({i}){ZipExt}";
            }

            return renameKey;
        }

        #endregion

        /// <summary>
        /// 私有压缩文件（用于解析嵌套压缩包层级，逐级压缩）
        /// </summary>
        private class MyCompressFile
        {
            /// <summary>
            /// 在父压缩包(不是根压缩包)中的EntityKey，同级目录需保持唯一
            /// </summary>
            public string ArchiveEntityKey { get; }

            /// <summary>
            /// 文件的下载网络地址(压缩包无下载地址)
            /// </summary>
            public string FileUrl { get; set; }

            /// <summary>
            /// 本地临时目录
            /// </summary>
            public string TempFilePath { get; set; }

            /// <summary>
            /// 文件是否为压缩包
            /// </summary>
            public bool IsPackage { get; }

            /// <summary>
            /// 内部文件(如果为压缩包则可添加内部文件)，同级目录EntityKey需保持唯一
            /// </summary>
            public Dictionary<string, MyCompressFile> InternalFiles { get; } = new Dictionary<string, MyCompressFile>();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="archiveEntityKey">在父压缩包(不是根压缩包)中的EntityKey，同级目录需保持唯一</param>
            /// <param name="isPackage">该文件本身是否压缩包</param>
            public MyCompressFile(string archiveEntityKey, bool isPackage = false)
            {
                ArchiveEntityKey = archiveEntityKey;
                IsPackage = isPackage;
            }
        }
    }
}
