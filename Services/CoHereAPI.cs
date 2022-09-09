using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services;
public interface ICoHereAPI
{
    public Task<String> GetKeywords(string paragraph);

}
public class CohereRequestGenerate
{
    public string model { get; set; }
    public string prompt { get; set; }
    public int max_tokens { get; set; }
    public double temperature { get; set; }
    public int k { get; set; }
    public int p { get; set; }
    public int frequency_penalty { get; set; }
    public int presence_penalty { get; set; }
    public List<string> stop_sequences { get; set; }
    public string return_likelihoods { get; set; }

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

    private readonly string apiKey;
    public CoHereAPI(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<string> GetKeywords(string paragraph)
    {
        /*
        Export Code
You can use the following code to start integrating the current API configuration to your application.
python
js
go
curl
cli
curl --location --request POST 'https://api.cohere.ai/generate' \ 
  --header 'Authorization: BEARER {apiKey}' \ 
  --header 'Content-Type: application/json' \ 
  --header 'Cohere-Version: 2021-11-08' \ 
  --data-raw '{ 
      "model": "xlarge", 
      "prompt": "What key concepts or people are being talked about in these passages?\nPassage: Is Wordle getting tougher to solve? Players seem to be convinced that the game has gotten harder in recent weeks ever since The New York Times bought it from developer Josh Wardle in late January. The Times has come forward and shared that this likely isn’t the case. That said, the NYT did mess with the back end code a bit, removing some offensive and sexual language, as well as some obscure words There is a viral thread claiming that a confirmation bias was at play. One Twitter user went so far as to claim the game has gone to “the dusty section of the dictionary” to find its latest words.\n\n3 Key Concepts: Wordle, games, difficulty \n--\nPassage: ArtificialIvan, a seven-year-old, London-based payment and expense management software company, has raised $190 million in Series C funding led by ARG Global, with participation from D9 Capital Group and Boulder Capital. Earlier backers also joined the round, including Hilton Group, Roxanne Capital, Paved Roads Ventures, Brook Partners, and Plato Capital.\n\n3 Key Concepts: ArtificialIvan, software, funding \n--\nPassage: The National Weather Service announced Tuesday that a freeze warning is in effect for the Bay Area, with freezing temperatures expected in these areas overnight. Temperatures could fall into the mid-20s to low 30s in some areas. In anticipation of the hard freeze, the weather service warns people to take action now.\n\n3 Key Concepts: Weather, freeze, warning\n--\nPassage: And of course having gone through the flight training I received my wings\nand commission in October of 1952. And the- one of the reasons I opted for\nthe Marines, I knew there had never been a black pilot in the Marine Corps.\nSo I wanted to see if I could achieve that goal, which I was able to do.\nAnd then my first duty assignment would have been in Cherry Point, North\nCarolina. But I'\''d had enough of the South and decided I wanted to stay away\nfrom the South if I possibly could, so Headquarters Marine Corps, at my\nrequest, changed my orders to El Toro, El Toro, California.\nBut what I didn'\''t realize is that I'\''d jumped from the frying pan into the fire\nbecause El Toro was the training base for replacement pilots in Korea. So I\njumped from the frying pan into the Korean War via El Toro.\n\n3 Key Concepts:", 
      "max_tokens": 50, 
      "temperature": 0.6, 
      "k": 0, 
      "p": 0, 
      "frequency_penalty": 0, 
      "presence_penalty": 0, 
      "stop_sequences": ["--"], 
      "return_likelihoods": "NONE" 
    }' 

        **/
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
            httpClient.DefaultRequestHeaders.Add("Authorization",$"BEARER {apiKey}");
            httpClient.DefaultRequestHeaders.Add("Cohere-Version","2021-11-08");

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