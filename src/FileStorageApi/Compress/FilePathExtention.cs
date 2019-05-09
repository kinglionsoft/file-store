using System;
using System.IO;
using System.Reflection;

namespace FileStorageApi.Compress
{
    public static class FilePathExtention
    {
        private static readonly object SyncLock = new object();

        /// <summary>
        /// 获取运行启动路径
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyPath()
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();

            var path = assembly.CodeBase;
            path = path.Substring(8, path.Length - 8);
            path = Path.GetDirectoryName(path);

            return path;
        }

        /// <summary>
        /// 创建时间戳随机字符目录
        /// </summary>
        /// <param name="baseDir">上级目录</param>
        /// <returns>创建的目录名</returns>
        public static string CreateTimeRandomDir(string baseDir)
        {
            return TimeRandomDirInnder(ref baseDir); 
        }

        private static string TimeRandomDirInnder(ref string baseDir)
        {
            var dir = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetRandomFileName().Substring(0, 5)}";
            dir = Path.Combine(baseDir, dir);
            lock (SyncLock)
            {
                if (Directory.Exists(dir))
                {
                    TimeRandomDirInnder(ref baseDir);
                }
                else
                {
                    Directory.CreateDirectory(dir);
                }
            }

            return dir;
        }
    }
}
