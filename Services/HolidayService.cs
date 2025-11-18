using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MoyuApp.Models;

namespace MoyuApp.Services
{
    public class HolidayService
    {
        private readonly HttpClient _httpClient;
        
        public HolidayService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// 从网络API获取节假日信息
        /// </summary>
        public async Task<List<Holiday>> GetHolidaysFromApiAsync(int year)
        {
            try
            {
                // 使用免费的节假日API
                var apiUrl = $"https://date.nager.at/api/v3/publicholidays/{year}/CN";
                
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var holidays = JsonConvert.DeserializeObject<List<PublicHolidayApiResponse>>(json);
                    
                    var result = new List<Holiday>();
                    foreach (var holiday in holidays)
                    {
                        result.Add(new Holiday
                        {
                            Name = holiday.LocalName ?? holiday.Name,
                            Month = holiday.Date.Month,
                            Day = holiday.Date.Day
                        });
                    }
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取网络节假日失败: {ex.Message}");
            }
            
            // 如果网络获取失败，返回默认的节假日
            return GetDefaultHolidays();
        }

        /// <summary>
        /// 获取默认的节假日（作为备用）
        /// </summary>
        private List<Holiday> GetDefaultHolidays()
        {
            return new List<Holiday>
            {
                new Holiday { Name = "元旦", Month = 1, Day = 1 },
                new Holiday { Name = "春节", Month = 2, Day = 1 },
                new Holiday { Name = "清明节", Month = 4, Day = 4 },
                new Holiday { Name = "劳动节", Month = 5, Day = 1 },
                new Holiday { Name = "端午节", Month = 6, Day = 14 },
                new Holiday { Name = "中秋节", Month = 9, Day = 21 },
                new Holiday { Name = "国庆节", Month = 10, Day = 1 }
            };
        }

        /// <summary>
        /// 获取当前年份的所有节假日
        /// </summary>
        public async Task<List<Holiday>> GetCurrentYearHolidaysAsync()
        {
            return await GetHolidaysFromApiAsync(DateTime.Now.Year);
        }
    }

    // API响应格式
    public class PublicHolidayApiResponse
    {
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        
        [JsonProperty("localName")]
        public string LocalName { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; } = string.Empty;
        
        [JsonProperty("fixed")]
        public bool Fixed { get; set; }
        
        [JsonProperty("global")]
        public bool Global { get; set; }
        
        [JsonProperty("counties")]
        public object Counties { get; set; } = null;
        
        [JsonProperty("launchYear")]
        public int? LaunchYear { get; set; }
        
        [JsonProperty("types")]
        public List<string> Types { get; set; } = new List<string>();
    }
}