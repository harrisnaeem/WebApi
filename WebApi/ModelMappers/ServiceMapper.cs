using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.WebModels;

namespace WebApi.ModelMappers
{
    public static class ServiceMapper
    {
        public static Service CreateFrom(this Models.Service source)
        {
            return new Service
            {
                ServiceName = source.ServiceName
            };
        }
    }
}