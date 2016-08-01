using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.WebModels;

namespace WebApi.RequestResponseModels
{
    public class UserResponseModel
    {
        public IEnumerable<User> usersList { get; set; }

        public Object serviceList { get; set; }
    }
}