using dotenv.net;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
// builder.Services.Configure<PositionOptions>(
//     builder.Configuration.GetSection(PositionOptions.Position));
// Add services to the container.
//builder.Services.AddTransient<Aisistant.Controllers.ApiContoller>();
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services); // calling ConfigureServices method
var app = builder.Build();
await startup.Configure(app, builder.Environment); // calling Configure method

// Configure the HTTP request pipeline.


