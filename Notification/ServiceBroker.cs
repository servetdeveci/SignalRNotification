using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Threading.Tasks;

namespace SignalRNotification.Notification
{
    public class ServiceBroker
    {
        private readonly RequestDelegate _next;
        public IHubContext<NotifyHub> _hubContext;
        public ServiceBroker(RequestDelegate next, IHubContext<NotifyHub> hubContext)
        {
            _next = next;
            var result = StartServiceBroker();
            _hubContext = hubContext;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next(httpContext);
        }

        public async Task StartServiceBroker()
        {
            await using var con = new NpgsqlConnection(GetConnectionString());
            await con.OpenAsync();
            con.Notification += notification_OnChange;
            await using (var cmd = new NpgsqlCommand())
            {
                cmd.CommandText = "LISTEN notifyping;";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
            }

            while (true)
            {
                con.Wait();
            }
        }

        private async void notification_OnChange(object sender, NpgsqlNotificationEventArgs e)
        {
            //var dataPayload = JsonConvert.DeserializeObject<StockInfo>(e.Payload);

            // NOTIFICATION WITH SIGNALR
            await _hubContext.Clients.All.SendAsync("onChanged", e.Payload);
        }
        static string GetConnectionString()
        {
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = "192.168.1.111",
                Database = "Atlas_Lora_DB",
                Username = "Atlas",
                Password = "Atlas1254",
                Port = 5432,
                KeepAlive = 30
            };
            return csb.ConnectionString;
        }

    }

    public static class ServiceBrokerExtensions
    {
        public static IApplicationBuilder UseServiceBroker(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ServiceBroker>();
        }
    }
}
