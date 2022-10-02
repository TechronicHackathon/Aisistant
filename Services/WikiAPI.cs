using WikiDotNet;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Services;
public interface IWikiAPI
{
    public Task<Tuple<string,string>> GetArticleFromSearch(string searchQuery);

}
public class WikiArticleTextSample
{
    public string batchcomplete { get; set; }
    public Query query { get; set; }
    public class Page
    {
        public int pageid { get; set; }
        public int ns { get; set; }
        public string title { get; set; }
        public string extract { get; set; }
    }

    public class Query
    {
        public Dictionary<string, Page> pages { get; set; }
    }

}
public class WikiAPI : IWikiAPI
{

    private readonly string apiKey;
    public WikiAPI(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<Tuple<string,string>> GetArticleFromSearch(string searchQuery)
    {
        WikiSearcher searcher = new();
        WikiSearchSettings searchSettings = new() { RequestId = $"Request ID{new Random().NextInt64(0, 100)}", ResultLimit = 1, ResultOffset = 0, Language = "en" };
        WikiSearchResponse response = searcher.Search(searchQuery, searchSettings);
        if (response.WasSuccessful && response.Query.SearchResults.Count()>0)
        {
            var result = response.Query.SearchResults[0];
            var title = result.Title;

            var body = await getArticleText(title);
            return Tuple.Create(body,title);
        }
        return Tuple.Create("","");;
    }
    private int getHeadingLevel(string header)
    {
        return header.Substring(0, header.IndexOf(' ')).Length;

    }
    private string getHeading(string header)
    {
        int startIdx = header.IndexOf(' ');
        return header.Substring(header.IndexOf(' '), header.LastIndexOf("==") - 1 - startIdx).Trim();

    }
    private string getBody(string fullText)
    {
        if (fullText.IndexOf("\n") > 0)
        {
            int startIdx = fullText.IndexOf("\n");

            return fullText.Substring(fullText.IndexOf("\n"), fullText.Length  - startIdx).Trim().Replace("\n", " ");
        }
        int startIdx2 = fullText.IndexOf("=");

        return fullText.Substring(fullText.IndexOf("="), fullText.Length  - startIdx2).Trim().Replace("\n", " ");

    }
    public void addOrUpdate(ref Dictionary<String, String> dict, string key, string value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] += " " + value;
        }
        else dict.Add(key, value);

    }
    public async Task<Dictionary<String, String>> QDParser(string[] wikiText, string heading, int idx, int hdLvl, Dictionary<string, string> retValue)
    {
        int curHdLvl = 0;
        if (!wikiText[idx].StartsWith("="))
        {
            addOrUpdate(ref retValue, heading, wikiText[idx].Trim().Replace("\n", " "));
            curHdLvl = hdLvl;
        }
        else
        {
            curHdLvl = getHeadingLevel(wikiText[idx]);
            if (curHdLvl > hdLvl)
            {
                if (heading == "Summary") heading = "";
                heading = heading + ">>>" + getHeading(wikiText[idx]);
            }
            else
            {
                var headingTree = heading.Split(">>>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                headingTree = headingTree.Take(curHdLvl).ToArray();
                headingTree[headingTree.Length - 1] = getHeading(wikiText[idx]);
                heading = String.Join(">>>", headingTree);

            }
            if (!wikiText[idx].Trim().EndsWith('='))
            {
                addOrUpdate(ref retValue, heading, getBody(wikiText[idx]));

            }

        }
        if (idx + 1 == wikiText.Count())
        {

            return retValue;
        }
        idx++;
        return await QDParser(wikiText, heading, idx, curHdLvl, retValue);

    }
    private async Task<string> getArticleText(string article_title)
    {
        article_title = System.Web.HttpUtility.HtmlEncode(article_title);
        var url = $"https://en.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&titles={article_title}&explaintext=1";
        using (var httpClient = new HttpClient())
        {
            using (var response = await httpClient.GetAsync(url))
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var article = await JsonSerializer.DeserializeAsync<WikiArticleTextSample>(apiResponse);
                return article.query.pages.First().Value.extract;
            }
        }
        return "";

    }

}