using System.Collections.Generic;

namespace WebApi.RequestResponseModels
{
    public class AuthResponseModel
    {
        public bool isSuccess { get; set; }
        public string message { get; set; }

        public IEnumerable<WebApi.WebModels.User> usersList { get; set; }
    }
}