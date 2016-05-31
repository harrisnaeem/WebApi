using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.ResponseModels
{
    public class AuthResponseModel
    {
        public bool isSuccess { get; set; }
        public string message { get; set; }
    }
}