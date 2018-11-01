using System;

namespace FileStorage.Core
{
    public class UserFriendlyException: Exception
    {
        public UserFriendlyException(string error) : base(error)
        {
            
        }
    }
}