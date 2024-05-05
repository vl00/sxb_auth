using System;

namespace iSchool.Authorization
{
    public class NoLoginException : Exception
    {
		public NoLoginException(string url = null) : base("未登录")
        { 
			this.LoginUrl = url;
        }
        
        public string LoginUrl { get; }
    }
}
