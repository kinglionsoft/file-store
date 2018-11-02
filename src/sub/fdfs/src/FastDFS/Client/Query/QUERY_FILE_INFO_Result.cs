using System;

namespace FastDFS.Client
{
    public class QUERY_FILE_INFO_Result: IRequestResult
	{
		public long FileSize { get; private set; }

		public DateTime CreateTime { get; private set; }

        public long Crc32 { get; private set; }

        public void Deserialize(byte[] responseByte)
		{
			byte[] numArray = new byte[8];
			byte[] numArray1 = new byte[8];
			byte[] numArray2 = new byte[8];
			Array.Copy(responseByte, 0, numArray, 0, (int)numArray.Length);
			Array.Copy(responseByte, 8, numArray1, 0, (int)numArray1.Length);
			Array.Copy(responseByte, 16, numArray2, 0, (int)numArray2.Length);
			this.FileSize = Util.BufferToLong(responseByte, 0);
			DateTime dateTime = new DateTime(0x7b2, 1, 1);
			this.CreateTime = dateTime.AddSeconds((double)Util.BufferToLong(responseByte, 8));
			this.Crc32 = Util.BufferToLong(responseByte, 16);
		}
	}
}