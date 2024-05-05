using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Models
{
    public class FileUploadResponseModel : RootModel
    {
        public string url { get; set; }
        public string cdnUrl { get; set; }
    }
}
