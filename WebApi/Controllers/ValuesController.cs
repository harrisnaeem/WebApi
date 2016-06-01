using System;
using System.Collections.Generic;
using System.Data.Spatial;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.Models;
using WebApi.ResponseModels;

namespace WebApi.Controllers
{
    public class LocationDetails
    {
        public string city { get; set; }
        public string region { get; set; }
        public string country { get; set; }
    }

    public class ValuesController : ApiController
    {
        LocationAppDbEntities db = new LocationAppDbEntities();

        // Post
        [HttpPost]
        public AuthResponseModel Login(string username, string password)
        {
            AuthResponseModel model = new AuthResponseModel { isSuccess = false };
            var query = (from p in db.Users
                         where (p.Username == username) && (p.Password == password)
                         select new { p.Name });

            if (query.Count() == 1)
            {
                model.isSuccess = true;
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
        public List<User> FindUsersWithinRangeAndService(double lat1, double lng1, double measure, string userEmail,int serviceId)
        {
            //AIzaSyBd_nVfEOscouHG6AOssfeK04e3LvTWewo      google map matrix api key
            //AIzaSyBd_nVfEOscouHG6AOssfeK04e3LvTWewo      geocoding key
            
            List<User> userList = new List<User>();
            var u = db.Users.Where(x => x.Username != userEmail && x.ServiceId == serviceId).ToList();
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

        public double deg2rad(double deg)       // converting degrees to radians
        {
            return deg * (Math.PI / 180);
        }

        [HttpPost]
        public AuthResponseModel SaveJobRequest(int serviceId, string imageString, string userEmail, double lat, double lng, double range)
        {
            AuthResponseModel model = new AuthResponseModel { isSuccess = false };
            JobRequest request = new JobRequest();
            List<User> usersList = new List<User>();
            
            usersList= FindUsersWithinRangeAndService(lat, lng, range,userEmail,serviceId);

            var userId = db.Users.Where(x => x.Username == userEmail).Select(x => new {x.Id});
            var firstOrDefault = userId.FirstOrDefault();
            if (firstOrDefault != null) 
            {
                int currentUserId = Convert.ToInt32(firstOrDefault.Id);

                if (usersList.Count > 0)
                {
                
                    imageString = imageString.Replace(" ", "+");   // original string cometimes contains '+' 
                    byte[] contents = Convert.FromBase64String(imageString.Trim('\0'));    
                    //byte[] contents = ConvertToBytes(imageString);
                
                    string subpath = "~/Images";
                    bool exists = Directory.Exists(HttpContext.Current.Server.MapPath(subpath));

                    if (!exists)
                    {
                        Directory.CreateDirectory(HttpContext.Current.Server.MapPath(subpath));
                    }
                    string fileName = "Image" + DateTime.Now.TimeOfDay + ".png";
                    var savePath = HttpContext.Current.Server.MapPath(subpath);
                    var imagePath = Path.Combine(savePath, Path.GetFileName(fileName));
                    File.WriteAllBytes(imagePath, contents);

                    request.ServiceTypeId = serviceId;
                    request.ImagePath = imagePath;
                    request.RequestUserId = currentUserId;
                    db.JobRequests.Add(request);
                    db.SaveChanges();

                    model.isSuccess = true;
                }
                else
                {
                    model.isSuccess = false;
                    model.message = "An error occurred";
                }
            }
            return model;
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
        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
