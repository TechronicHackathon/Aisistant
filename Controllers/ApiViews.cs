using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Aisistant.Controllers;

[ApiController]
[Route("apiview/[action]")]
public class ApiViews : Controller
{
    private readonly Aisistant.Data.AIAgentDBContext dBContext;
    private readonly Services.CoHereAPI cohereAPI;
    private readonly Services.WikiAPI wikiAPI;
    private readonly ApiController apiController;

    public ApiViews(Aisistant.Data.AIAgentDBContext _dbcontext, Services.ICoHereAPI _cohereAPI, Services.IWikiAPI _wikiAPI)
    {
        //init controller
        dBContext = _dbcontext;
        cohereAPI = (Services.CoHereAPI)_cohereAPI;
        wikiAPI = (Services.WikiAPI)_wikiAPI;
        //apiController=new ApiController(_dbcontext,_cohereAPI,_wikiAPI);
    }

    [HttpGet]
    public async Task<ActionResult> GetInterestingMessage()
    {
         int retries = 0;
         string errMsg="";

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
                    text = "No db values"
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
            if(string.IsNullOrEmpty(mostSimilarKey)){
                errMsg="No similar phrase result";
                goto ErrorResult;
            }
            var summarizedText = await cohereAPI.GetSummary(parsedDoc[mostSimilarKey]);
 
            var ret = new
            {
                type = "Summary",
                input = inputPara,
                title = article.Item2,
                key = mostSimilarKey,
                text = summarizedText
            };
            var userMessage=new ToastPartial(){
                BodyText=summarizedText,
                TopicTitle=article.Item2,
                TimeDisplay=DateTime.Now,
                TimeToShow_s=30
            };
            return PartialView("/Pages/Shared/Partials/_toastPartial.cshtml",userMessage);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            errMsg=ex.ToString();
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
            text = errMsg
        });

    }

}