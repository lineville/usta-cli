using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Spectre.Console.Cli;

public class UnsubscribeRankingsCommand : Spectre.Console.Cli.Command<RankingsSettings>
{
#nullable disable
  public override int Execute(CommandContext context, RankingsSettings settings)
  {
#nullable enable
    var configuration = context.Data as IConfiguration
      ?? throw new Exception("Failed to load configuration from appsettings.json");

    Utilities.InteractiveFallback(settings, configuration, context.Name);

    DeleteSubscriber(configuration, settings).GetAwaiter().GetResult();

    return 0;
  }

  public async Task DeleteSubscriber(IConfiguration configuration, RankingsSettings settings)
  {
    var connection = MongoClientSettings.FromConnectionString($"mongodb+srv://admin:{configuration["MONGO_PASSWORD"]}@prod.gngcbq9.mongodb.net/?retryWrites=true&w=majority");
    connection.ServerApi = new ServerApi(ServerApiVersion.V1);
    var client = new MongoClient(connection);
    var db = client.GetDatabase("usta-cli-subscribers");

    var subscribers = db.GetCollection<RankingsSettings>("subscribers");

    var builder = Builders<RankingsSettings>.Filter;
    var filter = builder.And(new List<FilterDefinition<RankingsSettings>>()
      {
        builder.Eq(r => r.Level, settings.Level),
        builder.Eq(r => r.Email, settings.Email),
        builder.Eq(r => r.Name, settings.Name),
        builder.Eq(r => r.Format, settings.Format),
        builder.Eq(r => r.Gender, settings.Gender),
        builder.Eq(r => r.Section, settings.Section)
      });

    await subscribers.DeleteOneAsync(filter);

    Console.WriteLine($"Successfully unsubscribed to rankings updates for {settings.Name}, level {settings.Level}, section {settings.Section}, format {settings.Format}");
  }
}