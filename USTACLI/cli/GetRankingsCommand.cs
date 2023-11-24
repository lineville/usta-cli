using Spectre.Console.Cli;
using Spectre.Console;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class GetRankingsCommand : Command<RankingsSettings>
{
#nullable disable
  public override int Execute(CommandContext context, RankingsSettings settings)
  {
#nullable enable
    var configuration = context.Data as IConfiguration
      ?? throw new Exception("Failed to load configuration from appsettings.json");

    Utilities.InteractiveFallback(settings, configuration, context.Name);

    var maxRetries = configuration.GetValue<int>("MAX_RETRIES");
    var retries = 0;

    RankingsReport? player = null;

    AnsiConsole
    .Status()
    .Spinner(new TennisBallSpinner())
    .Start("Searching for rankings", _ctx =>
    {
      // Try a few times in case of failures
      while (retries < maxRetries && player == null)
      {
        // Create the chrome driver service
        using (var driver = Driver.Create())
        {
          try
          {
            // Scrape the player ranking
            player = ScrapePlayerRanking(driver, configuration, settings, context.Name);
          }
          catch (Exception)
          {
            Console.WriteLine($"Failed to find ranking for {settings.Name} in {settings.Section}. Searching for ranking in any section.");
            settings.Section = null;
            retries++;
          }
          finally
          {
            driver.Quit();
          }
        }
      }
    });

    if (player == null)
    {
      throw new Exception($"Failed to find ranking for {settings.Name}");
    }
    else
    {
      AnsiConsole.Clear();
      player.Print();
      AnsiConsole.WriteLine();
    }
    return 0;
  }

  /// <summary>
  /// Extracts the HTML element and returns a Player object
  /// </summary>
  public RankingsReport ScrapePlayerRanking(WebDriver driver, IConfiguration configuration, RankingsSettings settings, string context)
  {
    var htmlElement = configuration.GetValue<string>("HTML_ELEMENT_TARGET")
      ?? throw new Exception("Failed to load HTML_ELEMENT_TARGET from appsettings.json");

    var timeout = configuration.GetValue<int>("PAGE_LOAD_TIMEOUT");

    var url = Utilities.BuildUSTARankingURL(settings, configuration, context);
    // Navigate to the URL and wait for the page to load
    driver.Navigate().GoToUrl(url);

    // Extra delay to allow the page to reload after name is searched
    Thread.Sleep(timeout * 1000);

    var elements = driver.FindElements(By.ClassName(htmlElement));

    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
    wait.Until(d => d.FindElement(By.ClassName(htmlElement)));

    if (elements[3].Text != settings.Name) // Throw if the name doesn't match
    {
      throw new NotFoundException($"No ranking found for {settings.Name}");
    }

    return new RankingsReport()
    {
      Player = new Player(elements[3].Text, elements[0].Text, elements[1].Text, elements[2].Text, elements[4].Text, elements[5].Text),
      Settings = settings
    };
  }
}