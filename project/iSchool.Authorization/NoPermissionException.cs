using System;

namespace iSchool.Authorization
{
    public class NoPermissionException : Exception
    {
		public NoPermissionException(int code, string msg) : this(msg, code) { }
    
        public NoPermissionException(string msg, int code) : base(msg) 
        { 
			this.Code = code;
        }
        
        public int Code { get; }
    }
}
