using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SignalRSample.Models
{
    public class Response
    {
        public static string OK = "OK";
        public static string ERR = "ERROR";

        public Response()
        {
            this.Status = ERR;
        }

        public Response(string status)
        {
            this.Status = status;
        }

        public string Status { get; set; }
        public JObject Result { get; set; }
    }
}
