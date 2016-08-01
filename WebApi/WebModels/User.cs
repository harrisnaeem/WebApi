using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.WebModels
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? Age { get; set; }
        public string Contact { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public TimeSpan? TimeFrom { get; set; }
        public TimeSpan? TimeTo { get; set; }
        public string ImageUrl { get; set; }
        public double? HourlyRate { get; set; }
        public List<Service> Services { get; set; }
    }
}