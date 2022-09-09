using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aisistant.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDictionary<string,string> envVars;

    public IndexModel(ILogger<IndexModel> logger, IDictionary<string,string> _envVars)
    {
        _logger = logger;
        envVars=_envVars;
    }

    public void OnGet()
    {
        var sessionID = HttpContext.Session.Id;
        if (!HttpContext.Session.Keys.Contains("User"))
        {
            HttpContext.Session.SetString("User", $"Authenticated{new Random().NextInt64(1, 2 ^ 64 - 1)}");
        }
        ViewData.Add("RevAIKey",envVars["TESTAPIKEY2"]);
        ViewData.Add("UA friendly", $"The UA is {Request.Headers["User-Agent"]}");
    }
}
