using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.WebModels;

namespace WebApi.ModelMappers
{
    public static class JobRequestMapper
    {
        public static JobRequest CreateFrom(this Models.JobRequest source)
        {
            return new JobRequest
            {
                RequestId = source.RequestId,
                //ImagePath = HttpContext.Current.Request.Url.Host+ "\\" + source.ImagePath,
                ImagePath = source.ImagePath,
                ServiceName = source.Service != null ? source.Service.ServiceName : String.Empty,
                UserObject = source.User != null ? source.User.CreateFrom() : null
                
            };
        }
    }
}