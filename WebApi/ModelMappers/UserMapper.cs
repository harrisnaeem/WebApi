using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.WebModels;

namespace WebApi.ModelMappers
{
    public static class UserMapper
    {
        public static User CreateFrom(this Models.User source)
        {
            return new User
            {
                Id = source.Id,
                Name = source.Name,
                Username = source.Username,
                Age = source.Age,
                Contact = source.Contact,
                City = source.City,
                Country = source.Country,
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                Region = source.Region,
                Password = source.Password,
                ImageUrl = source.ImageUrl,
                TimeFrom = source.TimeFrom,
                TimeTo = source.TimeTo,
                Services = source.Services.Select(x=>x.CreateFrom()).ToList(),
                HourlyRate = source.HourlyRate,
                
            };
        }
    }
}