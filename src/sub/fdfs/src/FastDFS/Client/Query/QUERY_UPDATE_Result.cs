using System;

namespace FastDFS.Client
{
    internal class QUERY_UPDATE_Result: IRequestResult
    {
        public string GroupName;

        public string IPStr;

        public int Port;

        public void Deserialize(byte[] responseByte)
        {
            byte[] numArray = new byte[16];
            Array.Copy(responseByte, numArray, 16);
            this.GroupName = Util.ByteToString(numArray).TrimEnd(new char[1]);
            byte[] numArray1 = new byte[15];
            Array.Copy(responseByte, 16, numArray1, 0, 15);
            this.IPStr = (new string(FDFSConfig.Charset.GetChars(numArray1))).TrimEnd(new char[1]);
            byte[] numArray2 = new byte[8];
            Array.Copy(responseByte, 31, numArray2, 0, 8);
            this.Port = (int)Util.BufferToLong(numArray2, 0);
        }
    }
}