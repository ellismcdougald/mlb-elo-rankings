using Amazon.Lambda.Core;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System.Globalization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DayHandler;

public class Game(int pk, string date, string season, string gt, int hId, int vId, bool winner) {
  public int GamePk { get; set; } = pk;
  public string OfficialDate { get; set; } = date;
  public string Season { get; set; } = season;
  public string GameType { get; set; } = gt;
  public int HomeTeamId { get; set; } = hId;
  public int AwayTeamId {get; set; } = vId;
  public bool HomeIsWinner {get; set;} = winner;

  public void print() {
    Console.WriteLine($"ID: {GamePk}, Date: {OfficialDate}");
  }

  public async void putGame(AmazonDynamoDBClient dbClient) {
    var request = new PutItemRequest {
      TableName = "GamesCollection",
      Item = new Dictionary<string, AttributeValue>() {
        {"ID", new AttributeValue {N = GamePk.ToString()}},
        {"Date", new AttributeValue {S = OfficialDate}},
        {"Season", new AttributeValue {S = Season}},
        {"GameType", new AttributeValue {S = GameType}},
        {"HomeTeamId", new AttributeValue {N = HomeTeamId.ToString()}},
        {"AwayTeamId", new AttributeValue {N = AwayTeamId.ToString()}},
        {"HomeIsWinner", new AttributeValue {BOOL = HomeIsWinner}},
      }
    };
    await dbClient.PutItemAsync(request);
  }
}

public class Function
{
    public static async Task FunctionHandler()
    {
      HttpClient httpClient = new HttpClient();

      AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
      clientConfig.ServiceURL = "http://dynamodb.us-west-2.amazonaws.com";
      AmazonDynamoDBClient dbClient = new AmazonDynamoDBClient(clientConfig); 

      Dictionary<int, double> teamRatings = await getTeamRatings(dbClient);

      Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
      DateTime yesterday = DateTime.Today.AddDays(-1);
      string yesterdayString = yesterday.ToShortDateString().Replace("/", "-");

      string response = await httpClient.GetStringAsync($"http://statsapi.mlb.com/api/v1/schedule/games/?sportId=1&startDate={yesterdayString}&endDate={yesterdayString}");
      var result = JsonConvert.DeserializeObject<dynamic>(response);
      if(result == null) return;
      if(result.dates.Count == 0) return;
      var gameJsons = result.dates[0].games;
      foreach(var gameJson in gameJsons) {
        if(gameJson.gameType != "R") continue;
        if(gameJson.status.codedGameState != "F") continue;
        Game game = new Game(gameJson.gamePk.ToObject<int>(), gameJson.officialDate.ToObject<string>(), gameJson.season.ToObject<string>(), gameJson.gameType.ToObject<string>(), gameJson.teams.home.team.id.ToObject<int>(), gameJson.teams.away.team.id.ToObject<int>(), gameJson.teams.home.isWinner.ToObject<bool>());
        await handleGame(game, teamRatings, dbClient);
      }

      return;
    }

    static async Task handleGame(Game game, Dictionary<int, double> teamRatings, AmazonDynamoDBClient dbClient) {
    double changeInRating = changeInRating = calculateChangeInRating(teamRatings[game.HomeTeamId], teamRatings[game.AwayTeamId]);

    // Update home team rating
    var homeTeamUpdateRequest = new UpdateItemRequest {
      TableName = "MLBElo-TeamRatings",
      Key = new Dictionary<string,AttributeValue>() { { "ID", new AttributeValue { N = game.HomeTeamId.ToString() } } },
      ExpressionAttributeNames = new Dictionary<string,string>()
      {   
        {"#R", "Rating"},
      },
      ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
      {
        {":rating",new AttributeValue { N = (game.HomeIsWinner ? changeInRating : -changeInRating).ToString() }},
      },
      UpdateExpression = "SET #R = #R + :rating"
    };
    await dbClient.UpdateItemAsync(homeTeamUpdateRequest);

    // Update away team rating
    var awayTeamUpdateRequest = new UpdateItemRequest {
      TableName = "MLBElo-TeamRatings",
      Key = new Dictionary<string,AttributeValue>() { { "ID", new AttributeValue { N = game.AwayTeamId.ToString() } } },
      ExpressionAttributeNames = new Dictionary<string,string>()
      {   
        {"#R", "Rating"},
      },
      ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
      {
        {":rating",new AttributeValue { N = (game.HomeIsWinner ? -changeInRating : changeInRating).ToString() }},
      },
      UpdateExpression = "SET #R = #R + :rating"
    };
    await dbClient.UpdateItemAsync(awayTeamUpdateRequest);
  }

  static async Task<Dictionary<int, double>> getTeamRatings(AmazonDynamoDBClient dbClient) {
    var request = new ScanRequest {
      TableName = "MLBElo-TeamRatings"
    };
    var res = await dbClient.ScanAsync(request);
    int id;
    double rating;
    Dictionary<int, double> teamRatings = new Dictionary<int, double>();
    foreach (Dictionary<string, AttributeValue> item
      in res.Items)
    {
      id = int.Parse(item["ID"].N);
      rating = double.Parse(item["Rating"].N);
      teamRatings.Add(id, rating);
    }
    return teamRatings;
  }

  static double calculateChangeInRating(double teamRating, double opponentRating) {
    double teamExpectedScore = 1 / (1 + Math.Pow(10, (opponentRating - teamRating) / 400));
    double change = 32 * (1 - teamExpectedScore);
    return change;
  }
}