//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public User()
        {
            this.JobRequests = new HashSet<JobRequest>();
            this.Services = new HashSet<Service>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> Age { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Contact { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public Nullable<System.TimeSpan> TimeFrom { get; set; }
        public Nullable<System.TimeSpan> TimeTo { get; set; }
        public string ImageUrl { get; set; }
        public string DeviceId { get; set; }
        public Nullable<double> HourlyRate { get; set; }
        public string Status { get; set; }
        public List<int> ServicesList { get; set; }
        public String CurrentUserId { get; set; }
        public virtual ICollection<JobRequest> JobRequests { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}
