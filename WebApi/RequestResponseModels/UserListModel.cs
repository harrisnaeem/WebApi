using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Models;

namespace WebApi.RequestResponseModels
{
    public class UserListModel
    {
        public string userEmail { get; set; }
        public List<string> list { get; set; }
    }
}