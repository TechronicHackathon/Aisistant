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
    public class LogObject
    {
        public string? text { get; set; }
        public float? startT_S { get; set; }
        public float? endT_S { get; set; }
    }
    [HttpPost]
    public async Task<ActionResult> Log(LogObject logEntry)
    {
        var sessionID = Sessions.GetSessionID(HttpContext);

        dBContext.SessionTranscript.Add(new Data.Models.SessionTranscript()
        {
            sessionID = sessionID,
            timestamp = DateTime.UtcNow,
            chatMessage = logEntry.text,
            startT_S = (int)logEntry.startT_S,
            endT_S = (int)logEntry.endT_S,
            stale = false,
            type = "Log"
        });
        await dBContext.SaveChangesAsync();
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
        var relevantParagraph = dBContext.SessionTranscript.Where(st => st.sessionID == sessionID && st.stale == false).OrderBy(st => st.startT_S).ToList();
        foreach (var entry in relevantParagraph)
        {
            entry.stale = true;
        }
        await dBContext.SaveChangesAsync();
        try
        {
            var inputPara = String.Join('\n', relevantParagraph.Select(st => st.chatMessage));

            if (relevantParagraph.Count() == 0)
            {
                return new JsonResult(new
                {
                    type = "Error",
                    input = inputPara,
                    title = "",
                    key = "",
                    text = ""
                });
            }
        retry:
            var keywordsList = await cohereAPI.GetKeywords(inputPara);
            var article = await wikiAPI.GetArticleFromSearch(keywordsList);
            if (article.Item1 == null)
            {
                retries++;
                if (retries > 3) goto ErrorResult;
                goto retry;

            }
            var parsedDoc = await wikiAPI.QDParser(article.Item1.Split("\n=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries), "Summary", 0, 0, new Dictionary<string, string>());
            //var allMessages = dBContext.SessionTranscript.ToList();
            var mostSimilarKey = await cohereAPI.FindMostSimilarPhrase(keywordsList, parsedDoc.Keys.ToList());
            var summarizedText = await cohereAPI.GetSummary(parsedDoc[mostSimilarKey]);
 
            var ret = new
            {
                type = "Summary",
                input = inputPara,
                title = article.Item2,
                key = mostSimilarKey,
                text = summarizedText
            };
            return new JsonResult(ret);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        ErrorResult:
        foreach (var entry in relevantParagraph)
        {
            entry.stale = false;
        }
        await dBContext.SaveChangesAsync();

        return new JsonResult(new
        {
            type = "Error",
            input = "",
            title = "",
            key = "",
            text = ""
        });
    }

}