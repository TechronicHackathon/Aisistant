using Microsoft.AspNetCore.Mvc;

namespace Aisistant.Controllers;

[ApiController]
[Route("api/[action]")]
public class ApiController : ControllerBase
{
    private readonly Aisistant.Data.AIAgentDBContext dBContext;
    private readonly Services.CoHereAPI cohereAPI;
    private readonly Services.WikiAPI wikiAPI;

    public ApiController(Aisistant.Data.AIAgentDBContext _dbcontext, Services.ICoHereAPI _cohereAPI, Services.IWikiAPI _wikiAPI)
    {
        //init controller
        dBContext = _dbcontext;
        cohereAPI = (Services.CoHereAPI)_cohereAPI;
        wikiAPI = (Services.WikiAPI)_wikiAPI;
    }

    [HttpGet]
    public ActionResult Create(string? action)
    {
        var sessionID = Sessions.GetSessionID(HttpContext);
        dBContext.SessionTranscript.Add(new Data.Models.SessionTranscript()
        {
            sessionID = sessionID,
            timestamp = DateTime.UtcNow,
            chatMessage = $"Starting Chat Session {new Random().NextInt64()}"
        });
        dBContext.SaveChanges();
        var allMessages = dBContext.SessionTranscript.ToList();

        var ret = new
        {
            answer = $"hello! {action}",
            messages = allMessages
        };

        return new JsonResult(ret);
    }

    [HttpPost]
    public async Task<ActionResult> Log(string? text, int? startT_S, int? endT_S)
    {
        var sessionID = Sessions.GetSessionID(HttpContext);

        dBContext.SessionTranscript.Add(new Data.Models.SessionTranscript()
        {
            sessionID = sessionID,
            timestamp = DateTime.UtcNow,
            chatMessage = text,
            startT_S = (int)startT_S,
            endT_S = (int)endT_S,
            stale = false,
            type = "Log"
        });
        dBContext.SaveChanges();
        //var allMessages = dBContext.SessionTranscript.ToList();

        var ret = new
        {
            status = "Logged"
        };

        return new JsonResult(ret);
    }
    [HttpPost]
    public async Task<ActionResult> GetInterestingMessage()
    {
        int retries = 0;

        var sessionID = Sessions.GetSessionID(HttpContext);
        var relevantParagraph = dBContext.SessionTranscript.Where(st => st.sessionID == sessionID && st.stale == false);
    retry:
        var keywordsList = await cohereAPI.GetKeywords(String.Join('\n', relevantParagraph.Select(st => st.chatMessage)));
        var article = await wikiAPI.GetArticleFromSearch(keywordsList);
        if (article == null)
        {
            retries++;
            if (retries > 3) return new JsonResult(new { status = "Failed" });
            goto retry;

        }
        var parsedDoc = await wikiAPI.QDParser(article.Split("\n=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries), "Summary", 0, 0, new Dictionary<string, string>());
        //var allMessages = dBContext.SessionTranscript.ToList();

        var ret = new
        {
            status = "Logged"
        };

        return new JsonResult(ret);
    }

}