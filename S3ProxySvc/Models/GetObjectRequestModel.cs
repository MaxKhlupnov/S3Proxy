using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S3ProxySvc.Models
{
    public class GetObjectRequestModel
    {

        public string BucketName { get; set; }
        public string Key { get; set; }
    }

    public class GetTextObjectRequestModel : GetObjectRequestModel
    {
        public string Encoding { get; set; }
    }
}
