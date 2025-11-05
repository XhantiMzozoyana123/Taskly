using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Domain.Entities;

namespace Taskly.Application.Constants
{
    public static class ApiConstant
    {
        public static void ExtractorHttpRequest(SearchDto searchDto, string domain)
        {
            var url = domain + ("api/extract/start");

            var request = WebRequest.Create(url);
            request.Method = "POST";

            var json = JsonConvert.SerializeObject(searchDto);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            var response = request.GetResponse();

            var respStream = response.GetResponseStream();

            var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();
        }

        public static void BatchExtractorHttpRequest(List<SearchDto> searchDtos, string domain)
        {
            var url = domain + ("api/extract/batch");

            var request = WebRequest.Create(url);
            request.Method = "POST";

            var json = JsonConvert.SerializeObject(searchDtos);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            var response = request.GetResponse();

            var respStream = response.GetResponseStream();

            var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();
        }


        public static void CampaignHttpRequest(Campaigns campaigns, string domain)
        {
            var url = domain + ("api/campaign/run");

            var request = WebRequest.Create(url);
            request.Method = "POST";

            var json = JsonConvert.SerializeObject(campaigns);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;

            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);

            var response = request.GetResponse();

            var respStream = response.GetResponseStream();

            var reader = new StreamReader(respStream);
            string data = reader.ReadToEnd();
        }
    }
}
