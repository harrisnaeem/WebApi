using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Models;

namespace WebApi.WebModels
{
    public class JobRequest
    {
        public int RequestId { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }
        public virtual User UserObject { get; set; }
        public string ServiceName { get; set; }
        //this will hold the distance between the job poster and job seeker
        public double Distance { get; set; }   
    }
}