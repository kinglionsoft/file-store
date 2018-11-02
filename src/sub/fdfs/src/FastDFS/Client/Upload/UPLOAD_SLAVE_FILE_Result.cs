using System;

namespace FastDFS.Client
{
    public class UPLOAD_SLAVE_FILE_Result: IRequestResult
    {
        // Fields
        public string FileName;
        public string GroupName;

        // Methods
        public void Deserialize(byte[] responseBody)
        {
            byte[] destinationArray = new byte[0x10];
            Array.Copy(responseBody, destinationArray, 0x10);
            this.GroupName = Util.ByteToString(destinationArray).TrimEnd(new char[1]);
            byte[] buffer2 = new byte[responseBody.Length - 0x10];
            Array.Copy(responseBody, 0x10, buffer2, 0, buffer2.Length);
            this.FileName = Util.ByteToString(buffer2).TrimEnd(new char[1]);
        }
    }
}