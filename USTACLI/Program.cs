using System.Reflection;
using Microsoft.Extensions.Configuration;
using Spectre.Console.Cli;

public class Program
{
  /// <summary>
  /// Main entry point
  /// </summary>
  public static int Main(string[] args)
  {
    // Parse command line arguments
    var app = new CommandApp();

    // Load appsettings.json static data
    var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();

    IConfiguration configuration = new ConfigurationBuilder()
      .SetBasePath(assemblyFolder)
      .AddJsonFile("appsettings.json", optional: false)
      .AddEnvironmentVariables()
      .Build();
      
    app.Configure(config =>
    {
      config.AddCommand<GetRankingsCommand>("get").WithDescription("Get player ranking").WithData(configuration);
      config.AddCommand<ListRankingsCommand>("list").WithDescription("List player rankings").WithData(configuration);
      config.AddCommand<SubscribeRankingsCommand>("subscribe").WithDescription("Subscribe to player rankings").WithData(configuration);
      config.AddCommand<UnsubscribeRankingsCommand>("unsubscribe").WithDescription("Unsubscribe from player rankings").WithData(configuration);

      // Hidden command used by GH actions (not intended for end user usage)
      config.AddCommand<ListSubscribersCommand>("subscribers").WithData(configuration).IsHidden();
    });

    return app.Run(args);
  }
}