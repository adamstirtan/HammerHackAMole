using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.OpenApi.Models;

namespace api;

public class Program
{
    public static int Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ISignalRServerBuilder signalRBuilder = builder.Services
            .AddSignalR(configure: options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.MaximumReceiveMessageSize = 32768;
                options.MaximumParallelInvocationsPerClient = 2;
                options.StreamBufferCapacity = 10;
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.WriteIndented = true;
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Encoder = null;
                options.PayloadSerializerOptions.IncludeFields = false;
                options.PayloadSerializerOptions.IgnoreReadOnlyFields = false;
                options.PayloadSerializerOptions.IgnoreReadOnlyProperties = false;
                options.PayloadSerializerOptions.MaxDepth = 0;
                options.PayloadSerializerOptions.DictionaryKeyPolicy = null;
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = false;
                options.PayloadSerializerOptions.DefaultBufferSize = 32768;
                options.PayloadSerializerOptions.ReferenceHandler = null;
                options.PayloadSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
                options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                options.PayloadSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.PayloadSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
            });

        string connectionString = builder.Configuration["AzureSignalR:ConnectionString"] ??
           throw new Exception("Azure SignalR connection string is missing.");

        signalRBuilder.AddAzureSignalR(connectionString);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "HammerHack-A-Mole API", Version = "v1" });
            c.AddSignalRSwaggerGen();
        });

        WebApplication app = builder.Build();

        app.UseCors("AllowFrontend");

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            c.RoutePrefix = string.Empty;
        });

        //app.UseHttpsRedirection();

        app.MapHub<GameHub>("/game-hub", options =>
        {
            options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
            options.TransportMaxBufferSize = 65536;
            options.TransportSendTimeout = TimeSpan.FromSeconds(10);
            options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(5);
            options.LongPolling.PollTimeout = TimeSpan.FromSeconds(10);
            options.CloseOnAuthenticationExpiration = true;
            options.ApplicationMaxBufferSize = 65536;
            options.MinimumProtocolVersion = 0;
        });

        try
        {
            app.Run();
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return -1;
        }
    }
}