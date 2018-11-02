using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace FastDFS.Client
{
    internal class ProtoCommon
    {
        /// <summary>
        /// receive package info
        /// </summary>
        public class RecvPackageInfo
        {
            public byte Errno;
            public byte[] Body;

            public RecvPackageInfo(byte errno, byte[] body)
            {
                this.Errno = errno;
                this.Body = body;
            }
        }
        /// <summary>
        /// receive header info
        /// </summary>
        public class RecvHeaderInfo
        {
            public byte Errno;
            public long BodyLen;

            public RecvHeaderInfo(byte errno, long bodyLen)
            {
                this.Errno = errno;
                this.BodyLen = bodyLen;
            }
        }

        public const byte FdfsProtoCmdQuit = 82;
        public const byte TrackerProtoCmdServerListGroup = 91;
        public const byte TrackerProtoCmdServerListStorage = 92;
        public const byte TrackerProtoCmdServerDeleteStorage = 93;

        public const byte TrackerProtoCmdServiceQueryStoreWithoutGroupOne = 101;
        public const byte TrackerProtoCmdServiceQueryFetchOne = 102;
        public const byte TrackerProtoCmdServiceQueryUpdate = 103;
        public const byte TrackerProtoCmdServiceQueryStoreWithGroupOne = 104;
        public const byte TrackerProtoCmdServiceQueryFetchAll = 105;
        public const byte TrackerProtoCmdServiceQueryStoreWithoutGroupAll = 106;
        public const byte TrackerProtoCmdServiceQueryStoreWithGroupAll = 107;
        public const byte TrackerProtoCmdResp = 100;
        public const byte FdfsProtoCmdActiveTest = 111;
        public const byte StorageProtoCmdUploadFile = 11;
        public const byte StorageProtoCmdDeleteFile = 12;
        public const byte StorageProtoCmdSetMetadata = 13;
        public const byte StorageProtoCmdDownloadFile = 14;
        public const byte StorageProtoCmdGetMetadata = 15;
        public const byte StorageProtoCmdUploadSlaveFile = 21;
        public const byte StorageProtoCmdQueryFileInfo = 22;
        public const byte StorageProtoCmdUploadAppenderFile = 23;  //create appender file
        public const byte StorageProtoCmdAppendFile = 24;  //append file
        public const byte StorageProtoCmdModifyFile = 34;  //modify appender file
        public const byte StorageProtoCmdTruncateFile = 36;  //truncate appender file

        public const byte StorageProtoCmdResp = TrackerProtoCmdResp;

        public const byte FdfsStorageStatusInit = 0;
        public const byte FdfsStorageStatusWaitSync = 1;
        public const byte FdfsStorageStatusSyncing = 2;
        public const byte FdfsStorageStatusIpChanged = 3;
        public const byte FdfsStorageStatusDeleted = 4;
        public const byte FdfsStorageStatusOffline = 5;
        public const byte FdfsStorageStatusOnline = 6;
        public const byte FdfsStorageStatusActive = 7;
        public const byte FdfsStorageStatusNone = 99;

        /**
        * for overwrite all old metadata
        */
        public const byte StorageSetMetadataFlagOverwrite = (byte)'O';

        /**
        * for replace, insert when the meta item not exist, otherwise update it
        */
        public const byte StorageSetMetadataFlagMerge = (byte)'M';

        public const int FdfsProtoPkgLenSize = 8;
        public const int FdfsProtoCmdSize = 1;
        public const int FdfsGroupNameMaxLen = 16;
        public const int FdfsIpaddrSize = 16;
        public const int FdfsDomainNameMaxSize = 128;
        public const int FdfsVersionSize = 6;
        public const int FdfsStorageIdMaxSize = 16;

        public const string FdfsRecordSeperator = "\u0001";
        public const string FdfsFieldSeperator = "\u0002";

        public const int TrackerQueryStorageFetchBodyLen = FdfsGroupNameMaxLen
                            + FdfsIpaddrSize - 1 + FdfsProtoPkgLenSize;
        public const int TrackerQueryStorageStoreBodyLen = FdfsGroupNameMaxLen
                            + FdfsIpaddrSize + FdfsProtoPkgLenSize;

        protected const int ProtoHeaderCmdIndex = FdfsProtoPkgLenSize;
        protected const int ProtoHeaderStatusIndex = FdfsProtoPkgLenSize + 1;

        public const byte FdfsFileExtNameMaxLen = 6;
        public const byte FdfsFilePrefixMaxLen = 16;
        public const byte FdfsFilePathLen = 10;
        public const byte FdfsFilenameBase64Length = 27;
        public const byte FdfsTrunkFileInfoLen = 16;

        public const byte ErrNoEnoent = 2;
        public const byte ErrNoEio = 5;
        public const byte ErrNoEbusy = 16;
        public const byte ErrNoEinval = 22;
        public const byte ErrNoEnospc = 28;
        public const byte Econnrefused = 61;
        public const byte ErrNoEalready = 114;

        public const long InfiniteFileSize = 256 * 1024L * 1024 * 1024 * 1024 * 1024L;
        public const long AppenderFileSize = InfiniteFileSize;
        public const long TrunkFileMarkSize = 512 * 1024L * 1024 * 1024 * 1024 * 1024L;
        public const long NormalLogicFilenameLength = FdfsFilePathLen + FdfsFilenameBase64Length + FdfsFileExtNameMaxLen + 1;
        public const long TrunkLogicFilenameLength = NormalLogicFilenameLength + FdfsTrunkFileInfoLen;

        private ProtoCommon()
        {
        }

        public static string GetStorageStatusCaption(byte status)
        {
            switch (status)
            {
                case FdfsStorageStatusInit:
                    return "INIT";
                case FdfsStorageStatusWaitSync:
                    return "WAIT_SYNC";
                case FdfsStorageStatusSyncing:
                    return "SYNCING";
                case FdfsStorageStatusIpChanged:
                    return "IP_CHANGED";
                case FdfsStorageStatusDeleted:
                    return "DELETED";
                case FdfsStorageStatusOffline:
                    return "OFFLINE";
                case FdfsStorageStatusOnline:
                    return "ONLINE";
                case FdfsStorageStatusActive:
                    return "ACTIVE";
                case FdfsStorageStatusNone:
                    return "NONE";
                default:
                    return "UNKOWN";
            }
        }
        /// <summary>
        /// pack header by FastDFS transfer protocol
        /// </summary>
        /// <param name="cmd">which command to send</param>
        /// <param name="pkgLen">package body length</param>
        /// <param name="errno">status code, should be (byte)0</param>
        /// <returns>packed byte buffer</returns>
        public static byte[] PackHeader(byte cmd, long pkgLen, byte errno)
        {
            byte[] header;
            byte[] hexLen;
            header = new byte[FdfsProtoPkgLenSize + 2];
            for (int i = 0; i < header.Length; i++)
                header[i] = (byte)0;
            //hex_len = BitConverter.GetBytes(pkg_len);
            hexLen = ProtoCommon.Long2Buff(pkgLen);
            Array.Copy(hexLen, 0, header, 0, hexLen.Length);
            header[ProtoHeaderCmdIndex] = cmd;
            header[ProtoHeaderStatusIndex] = errno;
            return header;
        }
        /// <summary>
        /// receive pack header
        /// </summary>
        /// <param name="input">input stream</param>
        /// <param name="expectCmd">expect response command</param>
        /// <param name="expectBodyLen">expect response package body length</param>
        /// <returns>errno and pkg body length</returns>
        public static RecvHeaderInfo RecvHeader(Stream input, byte expectCmd, long expectBodyLen)
        {
            byte[] header;
            int bytes;
            long pkgLen;
            header = new byte[FdfsProtoPkgLenSize + 2];
            if ((bytes = input.Read(header, 0, header.Length)) != header.Length)
            {
                throw new IOException("recv package size " + bytes + " != " + header.Length);
            }
            if (header[ProtoHeaderCmdIndex] != expectCmd)
            {
                throw new IOException("recv cmd: " + header[ProtoHeaderCmdIndex] + " is not correct, expect cmd: " + expectCmd);
            }
            if (header[ProtoHeaderStatusIndex] != 0)
            {
                return new RecvHeaderInfo(header[ProtoHeaderStatusIndex], 0);
            }
            //pkg_len = BitConverter.ToInt64(header, 0);
            pkgLen = ProtoCommon.Buff2Long(header, 0);
            if (pkgLen < 0)
            {
                throw new IOException("recv body length: " + pkgLen + " < 0!");
            }
            if (expectBodyLen >= 0 && pkgLen != expectBodyLen)
            {
                throw new IOException("recv body length: " + pkgLen + " is not correct, expect length: " + expectBodyLen);
            }
            return new RecvHeaderInfo((byte)0, pkgLen);
        }
        /// <summary>
        /// receive whole pack
        /// </summary>
        /// <param name="input">input stream</param>
        /// <param name="expectCmd">response command</param>
        /// <param name="expectBodyLen">response package body length</param>
        /// <returns>errno and reponse body(byte buff)</returns>
        public static RecvPackageInfo RecvPackage(Stream input, byte expectCmd, long expectBodyLen)
        {
            RecvHeaderInfo header = RecvHeader(input, expectCmd, expectBodyLen);
            if (header.Errno != 0)
            {
                return new RecvPackageInfo(header.Errno, null);
            }
            byte[] body = new byte[(int)header.BodyLen];
            int totalBytes = 0;
            int remainBytes = (int)header.BodyLen;
            int bytes;
            while (totalBytes < header.BodyLen)
            {
                if ((bytes = input.Read(body, totalBytes, remainBytes)) < 0)
                {
                    break;
                }

                totalBytes += bytes;
                remainBytes -= bytes;
            }
            if (totalBytes != header.BodyLen)
            {
                throw new IOException("recv package size " + totalBytes + " != " + header.BodyLen);
            }
            return new RecvPackageInfo((byte)0, body);
        }
        /// <summary>
        /// split metadata to name value pair array
        /// </summary>
        /// <param name="metaBuff">metadata</param>
        /// <returns>name value pair array</returns>
        public static NameValuePair[] SplitMetadata(string metaBuff)
        {
            return SplitMetadata(metaBuff, FdfsRecordSeperator, FdfsFieldSeperator);
        }
        /// <summary>
        /// split metadata to name value pair array
        /// </summary>
        /// <param name="metaBuff">metadata</param>
        /// <param name="recordSeperator">record/row seperator</param>
        /// <param name="filedSeperator">field/column seperator</param>
        /// <returns>name value pair array</returns>
        public static NameValuePair[] SplitMetadata(string metaBuff, string recordSeperator, string filedSeperator)
        {
            string[] rows;
            string[] cols;
            NameValuePair[] metaList;
            rows = metaBuff.Split(recordSeperator.ToCharArray());
            metaList = new NameValuePair[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                cols = rows[i].Split(filedSeperator.ToCharArray(), 2);
                metaList[i] = new NameValuePair(cols[0]);
                if (cols.Length == 2)
                {
                    metaList[i].Value = cols[1];
                }
            }
            return metaList;
        }
        /// <summary>
        /// pack metadata array to string
        /// </summary>
        /// <param name="metaList">metadata array</param>
        /// <returns>packed metadata</returns>
        public static string PackMetadata(NameValuePair[] metaList)
        {
            if (metaList.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder(32 * metaList.Length);
            sb.Append(metaList[0].Name).Append(FdfsFieldSeperator).Append(metaList[0].Value);
            for (int i = 1; i < metaList.Length; i++)
            {
                sb.Append(FdfsRecordSeperator);
                sb.Append(metaList[i].Name).Append(FdfsFieldSeperator).Append(metaList[i].Value);
            }
            return sb.ToString();
        }
        /// <summary>
        /// send quit command to server and close socket
        /// </summary>
        /// <param name="sock">the Socket object</param>
        public static void CloseSocket(TcpClient sock)
        {
            if (!sock.Connected) return;
            byte[] header = PackHeader(FdfsProtoCmdQuit, 0, (byte)0);
            using (sock)
            {
                sock.GetStream().Write(header, 0, header.Length);
            }
        }
        /// <summary>
        /// send ACTIVE_TEST command to server, test if network is ok and the server is alive
        /// </summary>
        /// <param name="sock">the Socket object</param>
        /// <returns></returns>
        public static bool ActiveTest(TcpClient sock)
        {
            byte[] header;
            header = PackHeader(FdfsProtoCmdActiveTest, 0, (byte)0);
            sock.GetStream().Write(header, 0, header.Length);

            RecvHeaderInfo headerInfo = RecvHeader(sock.GetStream(), TrackerProtoCmdResp, 0);
            return headerInfo.Errno == 0 ? true : false;
        }
        /// <summary>
        /// long convert to buff (big-endian)
        /// </summary>
        /// <param name="n">long number</param>
        /// <returns>8 bytes buff</returns>
        public static byte[] Long2Buff(long n)
        {
            byte[] bs;

            bs = new byte[8];
            bs[0] = (byte)((n >> 56) & 0xFF);
            bs[1] = (byte)((n >> 48) & 0xFF);
            bs[2] = (byte)((n >> 40) & 0xFF);
            bs[3] = (byte)((n >> 32) & 0xFF);
            bs[4] = (byte)((n >> 24) & 0xFF);
            bs[5] = (byte)((n >> 16) & 0xFF);
            bs[6] = (byte)((n >> 8) & 0xFF);
            bs[7] = (byte)(n & 0xFF);

            return bs;
        }
        /// <summary>
        /// buff convert to long
        /// </summary>
        /// <param name="bs">the buffer (big-endian)</param>
        /// <param name="offset">the start position based 0</param>
        /// <returns>long number</returns>
        public static long Buff2Long(byte[] bs, int offset)
        {
            return ((long)(bs[offset]) << 56) |
                    ((long)(bs[offset + 1]) << 48) |
                    ((long)(bs[offset + 2]) << 40) |
                    ((long)(bs[offset + 3]) << 32) |
                    ((long)(bs[offset + 4]) << 24) |
                    ((long)(bs[offset + 5]) << 16) |
                    ((long)(bs[offset + 6]) << 8) |
                     bs[offset + 7];
        }
        /// <summary>
        /// buff convert to int
        /// </summary>
        /// <param name="bs">the buffer (big-endian)</param>
        /// <param name="offset">the start position based 0</param>
        /// <returns>int number</returns>
        public static int Buff2Int(byte[] bs, int offset)
        {
            return bs[offset] << 24 |
                    bs[offset + 1] << 16 |
                    bs[offset + 2] << 8 |
                     (bs[offset + 3]);
        }
        /// <summary>
        /// buff convert to ip address
        /// </summary>
        /// <param name="bs">the buffer (big-endian)</param>
        /// <param name="offset">the start position based 0</param>
        /// <returns>ip address</returns>
        public static string GetIpAddress(byte[] bs, int offset)
        {
            if (bs[0] == 0 || bs[3] == 0) //storage server ID
            {
                return "";
            }

            int n;
            StringBuilder sbResult = new StringBuilder(16);
            for (int i = offset; i < offset + 4; i++)
            {
                n = (bs[i] >= 0) ? bs[i] : 256 + bs[i];
                if (sbResult.Length > 0)
                {
                    sbResult.Append(".");
                }
                sbResult.Append(n.ToString());
            }

            return sbResult.ToString();
        }

        public static string Md5(byte[] source)
        {
            char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] tmp = md5.ComputeHash(source, 0, source.Length);
                char[] str = new char[32];
                int k = 0;
                for (int i = 0; i < 16; i++)
                {
                    str[k++] = hexDigits[tmp[i] >> 4 & 0xf];
                    str[k++] = hexDigits[tmp[i] & 0xf];
                }

                return new string(str);
            }
        }
        /// <summary>
        /// get token for file URL
        /// </summary>
        /// <param name="remoteFilename">the filename return by FastDFS server</param>
        /// <param name="ts">unix timestamp, unit: second</param>
        /// <param name="secretKey">the secret key</param>
        /// <returns>token string</returns>
        public static string GetToken(string remoteFilename, int ts, string secretKey)
        {
            byte[] bsFilename = FDFSConfig.Charset.GetBytes(remoteFilename);
            byte[] bsKey = FDFSConfig.Charset.GetBytes(secretKey);
            byte[] bsTimestamp = FDFSConfig.Charset.GetBytes(ts.ToString());

            byte[] buff = new byte[bsFilename.Length + bsKey.Length + bsTimestamp.Length];
            Array.Copy(bsFilename, 0, buff, 0, bsFilename.Length);
            Array.Copy(bsKey, 0, buff, bsFilename.Length, bsKey.Length);
            Array.Copy(bsTimestamp, 0, buff, bsFilename.Length + bsKey.Length, bsTimestamp.Length);

            return Md5(buff);
        }
        /// <summary>
        /// generate slave filename
        /// </summary>
        /// <param name="masterFilename">the master filename to generate the slave filename</param>
        /// <param name="prefixName">the prefix name to generate the slave filename</param>
        /// <param name="extName">the extension name of slave filename, null for same as the master extension name</param>
        /// <returns>slave filename string</returns>
        public static string GenSlaveFilename(string masterFilename, string prefixName, string extName)
        {
            string trueExtName;
            int dotIndex;

            if (masterFilename.Length < 28 + FdfsFileExtNameMaxLen)
            {
                throw new FDFSException("master filename \"" + masterFilename + "\" is invalid");
            }

            dotIndex = masterFilename.IndexOf('.', masterFilename.Length - (FdfsFileExtNameMaxLen + 1));
            if (extName != null)
            {
                if (extName.Length == 0)
                {
                    trueExtName = "";
                }
                else if (extName[0] == '.')
                {
                    trueExtName = extName;
                }
                else
                {
                    trueExtName = "." + extName;
                }
            }
            else
            {
                if (dotIndex < 0)
                {
                    trueExtName = "";
                }
                else
                {
                    trueExtName = masterFilename.Substring(dotIndex);
                }
            }

            if (trueExtName.Length == 0 && prefixName.Equals("-m"))
            {
                throw new FDFSException("prefix_name \"" + prefixName + "\" is invalid");
            }

            if (dotIndex < 0)
            {
                return masterFilename + prefixName + trueExtName;
            }
            else
            {
                return masterFilename.Substring(0, dotIndex) + prefixName + trueExtName;
            }
        }

        /// <summary>
        /// unix时间戳转换成日期
        /// </summary>
        /// <param name="timestamp">时间戳（秒）</param>
        /// <returns></returns>
        public static DateTime UnixTimestampToDateTime(long timestamp)
        {
            var start = new DateTime(1970, 1, 1, 8, 0, 0, DateTime.Now.Kind);
            return start.AddSeconds(timestamp);
        }

        /// <summary>
        /// 日期转换成unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            var start = new DateTime(1970, 1, 1, 8, 0, 0, dateTime.Kind);
            return Convert.ToInt64((dateTime - start).TotalSeconds);
        }
    }
}
