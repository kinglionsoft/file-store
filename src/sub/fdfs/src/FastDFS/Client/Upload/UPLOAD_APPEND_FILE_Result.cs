using System;

namespace FastDFS.Client
{
    internal class UPLOAD_APPEND_FILE_Result: IRequestResult
    {
        public string GroupName;

        public string FileName;

        public void Deserialize(byte[] responseBody)
        {
            byte[] numArray = new byte[16];
            Array.Copy(responseBody, numArray, 16);
            this.GroupName = Util.ByteToString(numArray).TrimEnd(new char[1]);
            byte[] numArray1 = new byte[(int)responseBody.Length - 16];
            Array.Copy(responseBody, 16, numArray1, 0, (int)numArray1.Length);
            this.FileName = Util.ByteToString(numArray1).TrimEnd(new char[1]);
        }
    }
}