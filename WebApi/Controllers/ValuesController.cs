using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;
using WebApi.ModelMappers;
using WebApi.Models;
using WebApi.RequestResponseModels;

namespace WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        readonly LocationAppDbEntities db = new LocationAppDbEntities();

        [HttpPost]
        public AuthResponseModel Login(LoginModel lModel)
        {
            // Get login model in this APi and do working according to it
            AuthResponseModel model = new AuthResponseModel { isSuccess = false };
            var query = (from p in db.Users
                         where (p.Username == lModel.username) && (p.Password == lModel.password)
                         select new { p.Name });

            if (query.Count() == 1)
            {
                model.isSuccess = true;
                model.message = "Successfully Logged In!";
            }
            else
            {
                model.isSuccess = false;
                model.message = "Invalid Username or Password!";
            }
            return model;
        }

        [HttpPost]
        public AuthResponseModel Register(User usermodel)
        {
            AuthResponseModel model = new AuthResponseModel { isSuccess = false };
            User user = new User();
            LocationDetails loc_details=new LocationDetails();
            if (usermodel != null)
            {
                var existingUser = db.Users.Where(x => x.Username == usermodel.Username).ToList();     // checking if username already exists in the database as it is a unique
                if (existingUser.Count == 0)    // this username is not present in database, good to add it
                {
                    user.Name = usermodel.Name;
                    user.Username = usermodel.Username;
                    user.Password = usermodel.Password;
                    user.Age = usermodel.Age;
                    user.Contact = usermodel.Contact;
                    user.Latitude = usermodel.Latitude;
                    user.Longitude = usermodel.Longitude;
                    user.TimeFrom = TimeSpan.Parse(usermodel.TimeFrom.ToString());
                    user.TimeTo = TimeSpan.Parse(usermodel.TimeTo.ToString());
                    loc_details = getLocationDetails(usermodel.Latitude, usermodel.Longitude);
                    if (loc_details != null)
                    {
                        user.Region = loc_details.region;
                        user.City = loc_details.city;
                        user.Country = loc_details.country;
                    }
                    else
                    {
                        user.Country = getCountry(usermodel.Latitude, usermodel.Longitude);
                        user.Region = getRegion(usermodel.Latitude, usermodel.Longitude);
                        user.City = getCity(usermodel.Latitude, usermodel.Longitude);
                    }

                    user.ServiceId = usermodel.ServiceId;
                    user.DeviceId = usermodel.DeviceId;

                    db.Users.Add(user);
                    db.SaveChanges();
                    model.isSuccess = true;

                }
                else
                {
                    model.message = "This Username already exixts!";
                }
            }
            return model;
        }

        public string getRegion(double lat, double lng)
        {
            string region;
            try
            {
                XElement xelement = XElement.Load(@"https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lng + "&location_type=ROOFTOP&result_type=street_address&key=AIzaSyBd_nVfEOscouHG6AOssfeK04e3LvTWewo");

                var regionName =
                    from seg in xelement.Descendants("address_component")
                    where seg.Element("type").Value.Equals("route")
                    select seg.Element("long_name").Value;
                region = regionName.ToList()[0].ToString();

            }
            catch (Exception e)
            {
                return "";
            }
            return region;
        }
        public string getCountry(double lat, double lng)
        {
            string country = "";
            try
            {
                XElement xelement = XElement.Load(@"https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lng + "&location_type=ROOFTOP&result_type=street_address&key=AIzaSyBd_nVfEOscouHG6AOssfeK04e3LvTWewo");

                var countryName =
                    from seg in xelement.Descendants("address_component")
                    where seg.Element("type").Value.Equals("country")
                    select seg.Element("long_name").Value;
                country = countryName.ToList()[0].ToString();

            }
            catch (Exception e)
            {
                return "";
            }
            return country;
        }
        public string getCity(double lat, double lng)
        {
            string city = "";
            try
            {
                XElement xelement = XElement.Load(@"https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lng + "&location_type=ROOFTOP&result_type=street_address&key=AIzaSyBd_nVfEOscouHG6AOssfeK04e3LvTWewo");

                var cityName =
                    from seg in xelement.Descendants("address_component")
                    where seg.Element("type").Value.Equals("locality")
                    select seg.Element("long_name").Value;
                city = cityName.ToList()[0].ToString();
                
            }
            catch (Exception e)
            {
                return "";
            }
            return city;
        }

        public LocationDetails getLocationDetails(double lat, double lng)
        {
            LocationDetails details= new LocationDetails();
            try
            {
                XElement xelement = XElement.Load(@"https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lng + "&location_type=ROOFTOP&result_type=street_address&key=AIzaSyBd_nVfEOscouHG6AOssfeK04e3LvTWewo");

                var cityName =
                    from seg in xelement.Descendants("address_component")
                    where seg.Element("type").Value.Equals("locality")
                    select seg.Element("long_name").Value;
                details.city = cityName.ToList()[0].ToString();

                var countryName =
                    from seg in xelement.Descendants("address_component")
                    where seg.Element("type").Value.Equals("country")
                    select seg.Element("long_name").Value;
                details.country = countryName.ToList()[0].ToString();

                var regionName =
                    from seg in xelement.Descendants("address_component")
                    where seg.Element("type").Value.Equals("route")
                    select seg.Element("long_name").Value;
                details.region = regionName.ToList()[0].ToString();

            }
            catch (Exception e)
            {
                return null;
            }
            return details;
        }

        [HttpGet]
        public Object GetServices()
        {
            var list = db.Services.Select(x => new { x.Id, x.ServiceName }).ToList();
            return list;
        }

        [HttpPost]
        public List<User> FindUsersWithinRangeAndService(double lat1, double lng1, double measure, string userEmail,int serviceId,string timeFrom,string timeTo)
        {
            TimeSpan? time1 = null;
            TimeSpan? time2 = null;
            if (timeTo != null && !timeTo.Equals("") && timeFrom != null && !timeFrom.Equals(""))
            {
                time1 = TimeSpan.Parse(timeFrom);
                time2 = TimeSpan.Parse(timeTo);
            }
            List<User> userList = new List<User>();
            var u = db.Users.Where(x => x.Username != userEmail && x.ServiceId == serviceId
                                    && x.TimeFrom >= time1 && x.TimeTo <= time2).ToList();
            if (u.Count > 0)
            {
                for (int i = 0; i < u.Count; i++)
                {
                    double lat2 = u[i].Latitude;
                    double lng2 = u[i].Longitude;
                    double distance = GetDistanceFromLatLonInKm(lat1, lat2, lng1, lng2);
                    if (distance <= measure)
                    {
                        userList.Add(u[i]);
                    }
                }
                return userList;
            }
            return userList;
        }

        public double GetDistanceFromLatLonInKm(double lat1, double lat2, double lng1, double lng2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = deg2rad(lng2 - lng1);
            var a =
              Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
              Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
              Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
              ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        // converting degrees to radians
        public double deg2rad(double deg)       
        {
            return deg * (Math.PI / 180);
        }


        [HttpPost]
        public Object SaveJobRequest(SaveJobRequestModel jrModel)
        {
            AuthResponseModel model = new AuthResponseModel { isSuccess = false };
            JobRequest request = new JobRequest();
            List<User> usersList = new List<User>();
            string imagePath = null; 
            string imageString = jrModel.imageString;

            var user = db.Users.FirstOrDefault(x => x.Username == jrModel.userEmail);
            if (user != null)
            {
                var currentUserId = Convert.ToInt32(user.Id);
                usersList = FindUsersWithinRangeAndService(jrModel.lat, jrModel.lng, jrModel.range, jrModel.userEmail,jrModel.serviceId,jrModel.timeFrom,jrModel.timeTo);
                if (usersList.Count > 0)
                {
                    byte[] contents;
                    if (imageString != null)
                    {
                        imageString = imageString.Replace(" ", "+");

                        if (imageString.StartsWith("data:image"))
                        {
                            contents = ConvertToBytes(imageString);
                        }
                        else
                        {
                            contents = Convert.FromBase64String(imageString.Trim('\0'));
                        }

                        // storing the images in the 'Images' folder in the project
                        string subpath = "~/Images"; 
                        bool exists = Directory.Exists(HttpContext.Current.Server.MapPath(subpath));

                        if (!exists)
                        {
                            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(subpath));
                        }
                        string fileName = "Image" + DateTime.Now.TimeOfDay + ".png";
                        var savePath = HttpContext.Current.Server.MapPath(subpath);
                        imagePath = Path.Combine(savePath, Path.GetFileName(fileName));

                        using (var imageFile = new FileStream(imagePath, FileMode.Create))
                        {
                            imageFile.Write(contents, 0, contents.Length);
                            imageFile.Flush();

                        }
                        request.ServiceTypeId = jrModel.serviceId;
                        request.ImagePath = imagePath;
                        request.RequestUserId = currentUserId;
                    }
                    else
                    {
                        request.ServiceTypeId = jrModel.serviceId;
                        request.ImagePath = null;
                        request.RequestUserId = currentUserId;
                    }
                    
                    db.JobRequests.Add(request);
                    db.SaveChanges();

                    //calling the method that will send push notifications collectively
                    if (jrModel.autoContact)
                    {
                        var check = SendGroupNotification(user, imagePath);

                        if (check)
                        {
                            model.isSuccess = true;
                            model.message = "Job request saved!";
                        }
                        else
                        {
                            model.isSuccess = false;
                            model.message = "An error occurred while sending the notifications";
                        }
                    }
                    else
                    {
                        return Json(new { usersList = usersList.Select(x => x.CreateFrom()) });      // mapping user model to user web model
                    }
                }
                else
                {
                    model.isSuccess = false;
                    model.message = "No Other Users Exist With This Service";
                }
            }
            return model;
        }

        // this method is used send notifications to all users if user selects auto contact from appliation while posting a job
        //it needs to be changed it should only be sent to those whose service id is same with the request service id
        private bool SendGroupNotification(User user, string imageUrl)
        {
            bool flag = false;
            string api_key = "NzdlZTVkYzItZDMyMC00YmRlLWIzOTItY2I3YzBiNmUyNTZm";
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
            if (user != null)
            {
                string message = "This job is posted by: \n" + user.Name + "\n" + user.Contact + "\n" +user.City;
                if (request != null)
                {
                    request.KeepAlive = true;
                    request.Method = "POST";
                    request.ContentType = "application/json";

                    request.Headers.Add("authorization", "Basic "+api_key);

                    var serializer = new JavaScriptSerializer();
                    var obj = new
                    {
                        app_id = "458070bb-b5ab-4c4e-926b-d370d87ba009",
                        contents = new { en = message },
                        //data = new { image = "http://betacares.azurewebsites.net/DomainContents/Domainkey-107/Images/HireGroupImage/HireGroupImageId-19/HireGroupImage_52.png" },
                        data = new { image = imageUrl },
                        included_segments = new string[] { "All" }
                    };
                    var param = serializer.Serialize(obj);
                    byte[] byteArray = Encoding.UTF8.GetBytes(param);

                    try
                    {
                        using (var writer = request.GetRequestStream())
                        {
                            writer.Write(byteArray, 0, byteArray.Length);
                        }

                        string responseContent=null;
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseContent = reader.ReadToEnd();
                            }
                        }
                        if (responseContent != null)
                        {
                            // parsing the json returned by OneSignal Push API 
                            dynamic json = JObject.Parse(responseContent);
                            int noOfRecipients = json.recipients;
                            if (noOfRecipients > 0)
                            {
                                flag = true;
                            }
                        }
                        
                    }
                    catch (WebException ex)
                    {
                        flag = false;
                    }

                    
                }
            }
            return flag;
        }

        public byte[] ConvertToBytes(string imageString)
        {
            if (string.IsNullOrEmpty(imageString))
            {
                return null;
            }

            int firtsAppearingCommaIndex = imageString.IndexOf(',');

            if (firtsAppearingCommaIndex < 0)
            {
                return null;
            }

            if (imageString.Length < firtsAppearingCommaIndex + 1)
            {
                return null;
            }

            string sourceSubString = imageString.Substring(firtsAppearingCommaIndex + 1);

            try
            {
                return Convert.FromBase64String(sourceSubString.Trim('\0'));
            }
            catch (FormatException)
            {
                return null;
            }
        }

        [HttpGet]
        // at the moment this method is being called when a user registers itself
        public void SendNotificationAfterSignup(string player_id)
        {
            bool flag = false;
            string api_key = "NzdlZTVkYzItZDMyMC00YmRlLWIzOTItY2I3YzBiNmUyNTZm";
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
            
            if (player_id != null)
            {
                string message = "You are successfully registered";
                if (request != null)
                {
                    request.KeepAlive = true;
                    request.Method = "POST";
                    request.ContentType = "application/json";

                    request.Headers.Add("authorization", "Basic " + api_key);

                    var serializer = new JavaScriptSerializer();
                    var obj = new
                    {
                        app_id = "458070bb-b5ab-4c4e-926b-d370d87ba009",
                        headings = new { en = "Dear User!"},
                        contents = new { en = message },
                        include_player_ids = new string[] { player_id }
                    };
                    var param = serializer.Serialize(obj);
                    byte[] byteArray = Encoding.UTF8.GetBytes(param);

                    try
                    {
                        using (var writer = request.GetRequestStream())
                        {
                            writer.Write(byteArray, 0, byteArray.Length);
                        }

                        string responseContent = null;
                        using (var response = request.GetResponse() as HttpWebResponse)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                responseContent = reader.ReadToEnd();
                            }
                        }

                    }
                    catch (WebException ex)
                    {
                        flag = false;
                    }


                }
            }
           // return flag;
        }

        [HttpPost]
        //this method is called when user selects users to send notifications
        public Object SendNotificationsToSelectedUsers(UserListModel ulModel)                     
        {
            List<string> userIds = ulModel.list;
            int count = 0;
            string message = "";
            string api_key = "NzdlZTVkYzItZDMyMC00YmRlLWIzOTItY2I3YzBiNmUyNTZm";
            
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            var push_sender = db.Users.FirstOrDefault(x => x.Username == ulModel.userEmail);   //user that is posting a request 
            if (push_sender != null)
            {
                message = "This job is posted by: \n" + push_sender.Name + "\n" + push_sender.Contact + "\n" + push_sender.City;
            }

            if (userIds.Count > 0 && request != null)
            {
                
                foreach (string t in userIds)
                {
                    var email = t;
                    var user = db.Users.FirstOrDefault(x => x.Username == email);                //selecting the user from db to whom notification is to be sent
                    if (user != null)
                    {
                        if (user.DeviceId != null)
                        {
                            request.KeepAlive = true;
                            request.Method = "POST";
                            request.ContentType = "application/json";

                            request.Headers.Add("authorization", "Basic " + api_key);

                            var serializer = new JavaScriptSerializer();
                            var obj = new
                            {
                                app_id = "458070bb-b5ab-4c4e-926b-d370d87ba009",
                                headings = new { en = "New Work Request!" },
                                contents = new { en = message },
                                include_player_ids = new string[] { user.DeviceId }
                            };
                            var param = serializer.Serialize(obj);
                            byte[] byteArray = Encoding.UTF8.GetBytes(param);

                            try
                            {
                                using (var writer = request.GetRequestStream())
                                {
                                    writer.Write(byteArray, 0, byteArray.Length);
                                }

                                string responseContent = null;
                                using (var response = request.GetResponse() as HttpWebResponse)
                                {
                                    using (var reader = new StreamReader(response.GetResponseStream()))
                                    {
                                        responseContent = reader.ReadToEnd();
                                    }
                                }
                                if (responseContent != null)
                                {
                                    // parsing the json returned by OneSignal Push API 
                                    dynamic json = JObject.Parse(responseContent);
                                    int noOfRecipients = json.recipients;
                                    if (noOfRecipients > 0)
                                    {
                                        count++;
                                    }
                                }
                            }
                            catch (WebException ex)
                            {
                                
                            }
                        }
                        
                    }
                    
                }
            }
            if (count == userIds.Count)
            {
                return "Notifications sent to " + count + " users";
            }
            else
                return "Notifications sent to " + count + " user[s]";
        }

    }
}
