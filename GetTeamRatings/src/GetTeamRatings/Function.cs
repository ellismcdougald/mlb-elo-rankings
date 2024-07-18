using Amazon.Lambda.Core;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GetTeamRatings;

public class Function
{
    
    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public static async Task<object> FunctionHandler()
    {
        AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
        clientConfig.ServiceURL = "http://dynamodb.us-west-2.amazonaws.com";
        AmazonDynamoDBClient dbClient = new AmazonDynamoDBClient(clientConfig); 

        //Dictionary<int, double> teamRatings = await getTeamRatings(dbClient);
        Dictionary<string, string>[] teamRatings = await getTeamRatingsWithInfo(dbClient);
        Console.WriteLine();

        var headersObj = new Dictionary<string, string>();
        headersObj["Content-Type"] = "application/json";
        
        return new {
          statusCode = 200,
          headers = headersObj,
          body = teamRatings
        };
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

  static async Task<Dictionary<string, string>[]> getTeamRatingsWithInfo(AmazonDynamoDBClient dbClient) {
    var request = new ScanRequest {
      TableName = "MLBElo-TeamRatings"
    };
    var res = await dbClient.ScanAsync(request);
    string id, cityName, teamName, rating;
    Dictionary<string, string>[] teamRatings = new Dictionary<string, string>[30];
    int i = 0;
    foreach (Dictionary<string, AttributeValue> item
        in res.Items)
      {
        id = item["ID"].N;
        cityName = item["City"].S;
        teamName = item["Name"].S;
        rating = item["Rating"].N;
        teamRatings[i] = new Dictionary<string, string>();
        teamRatings[i].Add("id", id);
        teamRatings[i].Add("cityName", cityName);
        teamRatings[i].Add("teamName", teamName);
        teamRatings[i].Add("rating", rating);
        i++;
      }
    return teamRatings;
  }
}