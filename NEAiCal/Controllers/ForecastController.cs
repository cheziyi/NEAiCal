using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NEAiCal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForecastController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> GetAsync()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.data.gov.sg/v1/environment/");
            HttpResponseMessage response = await client.GetAsync("4-day-weather-forecast");

            ForecastWrapper fList = null;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                fList = JsonConvert.DeserializeObject<ForecastWrapper>(json);
            }

            if (fList == null)
                return null;

            var calendar = new Calendar();

            foreach (var forecast in fList.items.First().forecasts)
            {
                var e = new CalendarEvent
                {
                    IsAllDay = true,
                    Start = new CalDateTime(forecast.date),
                    End = new CalDateTime(forecast.date),
                    Summary = forecast.forecast
                };

                calendar.Events.Add(e);
            }

            var serializer = new CalendarSerializer();
            return serializer.SerializeToString(calendar);
        }
    }

    public class ForecastItem
    {
        public DateTime date { get; set; }
        public String forecast { get; set; }
    }

    public class ForecastList
    {
        public DateTime timestamp { get; set; }
        public IEnumerable<ForecastItem> forecasts { get; set; }
    }

    public class ForecastWrapper
    {
        public IEnumerable<ForecastList> items { get; set; }
    }
}