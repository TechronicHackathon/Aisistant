using System.Text.Json;
using System.Text.Json.Serialization;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
namespace Services;
public interface ICoHereAPI
{
    public Task<String> GetKeywords(string paragraph);
    public Task<string> FindMostSimilarPhrase(string reference, List<String> testValues);

}
public class CohereRequestGenerate
{
    public string model { get; set; }
    public string prompt { get; set; }
    public int max_tokens { get; set; }
    public double temperature { get; set; }
    public int k { get; set; }
    public int p { get; set; }
    public double frequency_penalty { get; set; }
    public int presence_penalty { get; set; }
    public List<string> stop_sequences { get; set; }
    public string return_likelihoods { get; set; }

}
public class CoHereEmbedRequest
{
    public string model { get; set; }
    public List<string> texts { get; set; }


}
public class CoHereEmbedResponse
{

    public string id { get; set; }
    public List<string> texts { get; set; }
    public List<double[]> embeddings { get; set; }

}
public class CohereResponseGenerate
{
    public class Generation
    {
        public string text { get; set; }
    }
    public string id { get; set; }
    public List<Generation> generations { get; set; }
    public string prompt { get; set; }

}
public class CoHereAPI : ICoHereAPI
{
    int MAXEMBEDLENGTH = 1000;

    private readonly string apiKey;
    public CoHereAPI(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<string> FindMostSimilarPhrase(string reference, List<String> testValues)
    {
        testValues = testValues.Take(15).ToList();
        testValues.Add(reference);

        var keyWordsRequest = new CoHereEmbedRequest()
        {
            model = "large",
            texts = testValues
        };
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"BEARER {apiKey}");
            httpClient.DefaultRequestHeaders.Add("Cohere-Version", "2021-11-08");

            using (var response = await httpClient.PostAsJsonAsync("https://api.cohere.ai/embed", keyWordsRequest))
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var results = await JsonSerializer.DeserializeAsync<CoHereEmbedResponse>(apiResponse);
                int counter = 0;
                var result = new Dictionary<string, double[]>();
                if (results == null)
                {
                    //why??
                    return null;
                }
                foreach (var resEmbed in results.texts)
                {
                    if (!string.IsNullOrEmpty(resEmbed))
                    {
                        result.Add(resEmbed, results.embeddings[counter]);
                    }
                    else
                    {
                        //why??
                    }
                    counter++;

                }
                // var result = results.texts.Zip(results.embeddings, (first, second) => new { first, second })
                //     .ToDictionary(val => val.first??"<empty>", val => val.second);
                return calculateClosestsToReference(reference, result.ToDictionary(pair => pair.Key, pair => pair.Value));

            }
        }
        return "";
    }
    private string calculateClosestsToReference(string reference, Dictionary<string, double[]> embeddings)
    {
        var refEmbed = embeddings[reference];
        Vector<double> refVec = Vector<double>.Build.Dense(refEmbed);

        Dictionary<string, double> distances = new Dictionary<string, double>();
        double min = double.MaxValue;
        string minkey = "";
        foreach (var entry in embeddings)
        {
            var valVec = Vector<double>.Build.Dense(entry.Value);
            var dif = valVec - refVec;

            distances.Add(entry.Key, dif.L2Norm());
            if (distances[entry.Key] != 0 && distances[entry.Key] < min)
            {
                min = distances[entry.Key];
                minkey = entry.Key;
            }
        }

        return minkey;
    }

    public async Task<string> GetKeywords(string paragraph)
    {
        var keyWordsRequest = new CohereRequestGenerate()
        {
            model = "xlarge",
            prompt = $"What key concepts or people are being talked about in these passages?\nPassage: Is Wordle getting tougher to solve? Players seem to be convinced that the game has gotten harder in recent weeks ever since The New York Times bought it from developer Josh Wardle in late January. The Times has come forward and shared that this likely isn’t the case. That said, the NYT did mess with the back end code a bit, removing some offensive and sexual language, as well as some obscure words There is a viral thread claiming that a confirmation bias was at play. One Twitter user went so far as to claim the game has gone to “the dusty section of the dictionary” to find its latest words.\n\n3 Key Concepts: Wordle, games, difficulty \n--\nPassage: ArtificialIvan, a seven-year-old, London-based payment and expense management software company, has raised $190 million in Series C funding led by ARG Global, with participation from D9 Capital Group and Boulder Capital. Earlier backers also joined the round, including Hilton Group, Roxanne Capital, Paved Roads Ventures, Brook Partners, and Plato Capital.\n\n3 Key Concepts: ArtificialIvan, software, funding \n--\nPassage: The National Weather Service announced Tuesday that a freeze warning is in effect for the Bay Area, with freezing temperatures expected in these areas overnight. Temperatures could fall into the mid-20s to low 30s in some areas. In anticipation of the hard freeze, the weather service warns people to take action now.\n\n3 Key Concepts: Weather, freeze, warning\n--\nPassage: {paragraph}\n\n3 Key Concepts:",
            max_tokens = 50,
            temperature = .6,
            k = 0,
            p = 0,
            frequency_penalty = 0,
            presence_penalty = 0,
            stop_sequences = new List<string>() { "--" },
            return_likelihoods = "NONE"
        };
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"BEARER {apiKey}");
            httpClient.DefaultRequestHeaders.Add("Cohere-Version", "2021-11-08");

            using (var response = await httpClient.PostAsJsonAsync("https://api.cohere.ai/generate", keyWordsRequest))
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var article = await JsonSerializer.DeserializeAsync<CohereResponseGenerate>(apiResponse);
                return article.generations.First().text.TrimEnd('-').TrimEnd('-').Trim();
            }
        }


        return "";
    }
    public async Task<string> GetSummary(string paragraph)
    {
        /*
{ 
      "model": "xlarge", 
      "prompt": "Passage: Is Wordle getting tougher to solve? Players seem to be convinced that the game has gotten harder in recent weeks ever since The New York Times bought it from developer Josh Wardle in late January. The Times has come forward and shared that this likely isn’t the case. That said, the NYT did mess with the back end code a bit, removing some offensive and sexual language, as well as some obscure words There is a viral thread claiming that a confirmation bias was at play. One Twitter user went so far as to claim the game has gone to “the dusty section of the dictionary” to find its latest words.\n\ninteresting and exciting full sentences TLDR: Did you know that the New York times bought the game \"Wordle\"? Players think this has made it harder, but this isn'\''t the case! The New York Times has remove some offenseive content and even made it easier by removing obscure words!\n--\nPassage: ArtificialIvan, a seven-year-old, London-based payment and expense management software company, has raised $190 million in Series C funding led by ARG Global, with participation from D9 Capital Group and Boulder Capital. Earlier backers also joined the round, including Hilton Group, Roxanne Capital, Paved Roads Ventures, Brook Partners, and Plato Capital.\n\ninteresting and exciting full sentences TLDR: A startup that is only seven years old raised a whopping $190 million in funding! Wow! The funders included participation from D9 Capital Group and Boulder Capital, joining earlier backers Hilton Group, Roxanne Capital, Paved Roads Ventures, Brook Partners, and Plato Capital.\n--\nPassage: Autonomous ships that monitor the ocean, AI-driven satellite data analysis, passive acoustics or remote sensing and other applications of environmental monitoring make use of machine learning.For example, \\"Global Plastic Watch\\" is an AI-based satellite monitoring-platform for analysis/tracking of plastic waste sites to help prevention of plastic pollution – primarily ocean pollution – by helping identify who and where mismanages plastic waste, dumping it into oceans.\n\ninteresting and exciting full sentences TLDR: Did you realize AI is used in applications for environmental monitoring? For example, Global Plastic Watch is an AI-based satellite monitoring-platform for analysis/tracking of plastic waste sites to help prevention of plastic pollution.\n--\nPassage: In one study, researchers examined the effects of overlearning geography facts or word definitions.  After one week, overlearners recalled more geography facts and word definitions than non-overlearners, but this improvement gradually disappeared after the study. This research suggests that overlearning may be an inefficient study method for long-term retention of geography facts and word definitions. Overlearning improves short-term retention of material, but learners must also spend more time studying. Over time the improvements created by overlearning fade, and the learner is no better off than someone who did not spend time overlearning the material.\n\ninteresting and exciting full sentences TLDR:", 
      "max_tokens": 75, 
      "temperature": 0.8, 
      "k": 0, 
      "p": 1, 
      "frequency_penalty": 0.01, 
      "presence_penalty": 0, 
      "stop_sequences": ["--"], 
      "return_likelihoods": "NONE" 
    }
        */
        paragraph = paragraph.Replace("\n", " ").Replace("-", " ");
        if (paragraph.Length > MAXEMBEDLENGTH)
        {
            paragraph = paragraph.Substring(0, MAXEMBEDLENGTH);
        }
        var keyWordsRequest = new CohereRequestGenerate()
        {
            model = "xlarge",
            prompt = $"Passage: Is Wordle getting tougher to solve? Players seem to be convinced that the game has gotten harder in recent weeks ever since The New York Times bought it from developer Josh Wardle in late January. The Times has come forward and shared that this likely isn't the case. That said, the NYT did mess with the back end code a bit, removing some offensive and sexual language, as well as some obscure words There is a viral thread claiming that a confirmation bias was at play. One Twitter user went so far as to claim the game has gone to \"the dusty section of the dictionary\" to find its latest words.\n\ninteresting and exciting full sentences TLDR: Did you know that the New York times bought the game \"Wordle\"? Players think this has made it harder, but this isn't the case! The New York Times has remove some offensive content and even made it easier by removing obscure words!\n--\nPassage: ArtificialIvan, a seven-year-old, London-based payment and expense management software company, has raised $190 million in Series C funding led by ARG Global, with participation from D9 Capital Group and Boulder Capital. Earlier backers also joined the round, including Hilton Group, Roxanne Capital, Paved Roads Ventures, Brook Partners, and Plato Capital.\n\ninteresting and exciting full sentences TLDR: A startup that is only seven years old raised a whopping $190 million in funding! Wow! The funders included participation from D9 Capital Group and Boulder Capital, joining earlier backers Hilton Group, Roxanne Capital, Paved Roads Ventures, Brook Partners, and Plato Capital.\n--\nPassage: Autonomous ships that monitor the ocean, AI-driven satellite data analysis, passive acoustics or remote sensing and other applications of environmental monitoring make use of machine learning.For example, \"Global Plastic Watch\" is an AI-based satellite monitoring-platform for analysis/tracking of plastic waste sites to help prevention of plastic pollution – primarily ocean pollution – by helping identify who and where mismanages plastic waste, dumping it into oceans.\n\ninteresting and exciting full sentences TLDR: Did you realize AI is used in applications for environmental monitoring? For example, Global Plastic Watch is an AI-based satellite monitoring-platform for analysis/tracking of plastic waste sites to help prevention of plastic pollution.\n--\nPassage:{paragraph}\n\ninteresting and exciting full sentences TLDR:",
            max_tokens = 60,
            temperature = .8,
            k = 0,
            p = 1,
            frequency_penalty = .01,
            presence_penalty = 0,
            stop_sequences = new List<string>() { "--" },
            return_likelihoods = "NONE"
        };
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"BEARER {apiKey}");
            httpClient.DefaultRequestHeaders.Add("Cohere-Version", "2021-11-08");

            using (var response = await httpClient.PostAsJsonAsync("https://api.cohere.ai/generate", keyWordsRequest))
            {
                var apiResponse = await response.Content.ReadAsStreamAsync();
                var article = await JsonSerializer.DeserializeAsync<CohereResponseGenerate>(apiResponse);
                return article.generations.First().text.TrimEnd('-').TrimEnd('-').Trim();
            }
        }


        return "";
    }

}