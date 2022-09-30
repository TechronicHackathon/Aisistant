
using dotenv.net;
using Microsoft.EntityFrameworkCore;
public class Startup
{
    public IConfiguration configRoot
    {
        get;
    }
    public Startup(IConfiguration configuration)
    {
        configRoot = configuration;
    }
    public void ConfigureServices(IServiceCollection services)
    {

        DotEnv.Fluent()
        .WithExceptions()
            .WithOverwriteExistingVars()
            .Load();
        var envVars = DotEnv.Read();
        services.AddDistributedMemoryCache();
        services.AddDbContext<Aisistant.Data.AIAgentDBContext>(
            options => options.UseInMemoryDatabase(databaseName: "AIAgent")
        );
        services.AddTransient<Services.ICoHereAPI>(sp => new Services.CoHereAPI(envVars["TESTAPIKEY"]));
        services.AddTransient<Services.IWikiAPI>(sp => new Services.WikiAPI("apikey"));
        services.AddSingleton<IDictionary<string, string>>(sp => envVars);
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(15);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        services.AddControllers();

        services.AddRazorPages();

        services.AddDbContext<Aisistant.Data.AIAgentDBContext>(
            options => options.UseInMemoryDatabase(databaseName: "AIAgent")
        );
        services.AddTransient<Services.ICoHereAPI>(sp => new Services.CoHereAPI(envVars["TESTAPIKEY"]));
        services.AddTransient<Services.IWikiAPI>(sp => new Services.WikiAPI("apikey"));
    }
    public async Task Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        using (var scope = app.Services.CreateScope())
        {
            var dbcontext = scope.ServiceProvider.GetService<Aisistant.Data.AIAgentDBContext>();
            await dbcontext.Database.EnsureCreatedAsync();
        }
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseSession();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();
       // testEndpoint(app);
        app.Run();
    }
    public async Task testEndpoint(WebApplication app)
    {

        //test API endpoints
        Thread t = new Thread(new ThreadStart(async () =>
        {
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbcontext = scope.ServiceProvider.GetService<Aisistant.Data.AIAgentDBContext>();
                    var apicon = (Aisistant.Controllers.ApiController)Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(scope.ServiceProvider, typeof(Aisistant.Controllers.ApiController));
                    await apicon.Log("In the last section, we examined some early aspects of memory. In this section, what we’re going to do is discuss some factors that influence memory. So let’s do that by beginning with the concept on slide two, and that concept is overlearning. Basically in overlearning, the idea is that you continue to study something after you can recall it perfectly. So you study some particular topic whatever that topic is. When you can recall it perfectly, you continue to study it. This is a classic way to help when one is taking comprehensive finals later in the semester. So when you study for exam one and after you really know it all, you continue to study it. That will make your comprehensive final easier.",
                    2, 8);
                    await apicon.Log("Let me just share my screen here. Okay, can you all see a browser window?",
            12, 20);

                    var msgresult = await apicon.GetInterestingMessage();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }));
        t.Start();

    }
}