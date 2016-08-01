using System;

namespace WebApi.RequestResponseModels
{
    public class SaveJobRequestModel
    {
        public int serviceId { get; set; }
        public string imageString { get; set; }
        public string userEmail { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public double range { get; set; }
        public string timeFrom { get; set; }
        public string timeTo { get; set; }
        public string description { get; set; }
        public string postDate { get; set; }
        public bool autoContact { get; set; }
    }
}