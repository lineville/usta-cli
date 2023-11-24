using Spectre.Console.Cli;
using Spectre.Console;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class ListRankingsCommand : Command<RankingsSettings>
{
#nullable disable
  public override int Execute(CommandContext context, RankingsSettings settings)
  {
#nullable enable
    var configuration = context.Data as IConfiguration
      ?? throw new Exception("Failed to load configuration from appsettings.json");

    Utilities.InteractiveFallback(settings, configuration, context.Name);

    var table = new Table
    {
      Title = new TableTitle($"{settings.Section} {(settings.Gender == Gender.M ? "Men's" : "Women's")} {settings.Level} {settings.Format} USTA Rankings", new Style(Color.Aqua, Color.Black)),
      Border = TableBorder.HeavyEdge
    };

    // Add header row 
    table.AddColumns(new TableColumn[]
    {
      new TableColumn(new Text("Name", new Style(Color.Blue, Color.Black)).LeftJustified()),
      new TableColumn(new Text("District", new Style(Color.Red, Color.Black)).LeftJustified()),
      new TableColumn(new Text("Section", new Style(Color.Yellow, Color.Black)).LeftJustified()),
      new TableColumn(new Text("National", new Style(Color.Green, Color.Black)).LeftJustified()),
      new TableColumn(new Text("Points", new Style(Color.Gold1, Color.Black)).LeftJustified()),
      new TableColumn(new Text("Location", new Style(Color.Purple, Color.Black)).LeftJustified())
    });

    List<Player> players = new List<Player>();
    int pageSize = 20;

    AnsiConsole
    .Status()
    .Spinner(new TennisBallSpinner())
    .Start("Searching for rankings", _ctx =>
    {
      var pageNumber = 1;
      var keepFetching = true;
      // Keep fetching as long as there are more players to fetch and we haven't exceeded settings.Top many players
      while (keepFetching)
      {
        // Create the chrome driver service
        using (var driver = Driver.Create())
        {
          // Scrape a page of players, add them to the list, update the live table and increment the page number
#nullable disable
          var pageOfPlayers = ScrapeRankings(driver, configuration, settings, context.Name, pageNumber).Take(Math.Min(pageSize, settings.Top.Value - players.Count()));
#nullable enable
          players.AddRange(pageOfPlayers);
          pageNumber++;
          if (pageOfPlayers.Count() < pageSize || players.Count() >= settings.Top)
          {
            keepFetching = false;
          }
        } // Driver gets disposed here
      }
      foreach (var player in players)
      {
        table.AddRow(new string[] { player.Name.ToString(), player.DistrictRank.ToString(), player.SectionRank.ToString(), player.NationalRank.ToString(), player.TotalPoints.ToString(), player.Location.ToString() });
      }
    });

    // Write to Console
    AnsiConsole.Clear();
    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();

    return 0;
  }

  /// <summary>
  /// Extracts the HTML element and returns a Player object
  /// </summary>
  public List<Player> ScrapeRankings(WebDriver driver, IConfiguration configuration, RankingsSettings settings, string context, int? pageNumber = 1)
  {
    var htmlElement = configuration.GetValue<string>("HTML_ELEMENT_TARGET")
      ?? throw new Exception("Failed to load HTML_ELEMENT_TARGET from appsettings.json");

    var timeout = configuration.GetValue<int>("PAGE_LOAD_TIMEOUT");

    var url = Utilities.BuildUSTARankingURL(settings, configuration, context, pageNumber);

    // Navigate to the URL and wait for the page to load
    driver.Navigate().GoToUrl(url);
    var elements = driver.FindElements(By.ClassName(htmlElement));

    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
    wait.Until(d => d.FindElement(By.ClassName(htmlElement)));

    var playerElements = elements.Chunk(9);

    return playerElements.Select(p => new Player(p[3].Text, p[0].Text, p[1].Text, p[2].Text, p[4].Text, p[5].Text)).ToList();
  }
}