using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.WebModels;
using Service = WebApi.Models.Service;

namespace WebApi.RequestResponseModels
{
    public class JobRequestModel
    {
        public IEnumerable<JobRequest> jobsList { get; set; }

        public Object serviceList { get; set; }
    }
}