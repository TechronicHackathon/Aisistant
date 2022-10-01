using dotenv.net;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
// builder.Services.Configure<PositionOptions>(
//     builder.Configuration.GetSection(PositionOptions.Position));
// Add services to the container.
//builder.Services.AddTransient<Aisistant.Controllers.ApiContoller>();
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services); // calling ConfigureServices method
// builder.WebHost.ConfigureKestrel((context, serverOptions) =>
// {
//     serverOptions.Listen(System.Net.IPAddress.Any, 5001, listenOptions =>
//     {
//         listenOptions.UseHttps();
//     });
// });
var app = builder.Build();
startup.Configure(app, builder.Environment); // calling Configure method
app.Run();




