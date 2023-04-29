using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Spectre.Console;
using Spectre.Console.Cli;

public class GetResultsCommand : Command<ResultsSettings>
{
#nullable disable
  public override int Execute(CommandContext context, ResultsSettings settings)
  {
#nullable enable

    //  TODO provide an option to just pass in a name not a USTA number
    // this means needing to navigate to the player search, and click search and find them this way...
    // ----------------------------

    // Load appsettings.json static data
    IConfiguration configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false)
      .Build();

    Utilities.InteractiveFallback(settings, configuration, context.Name);

    var maxRetries = configuration.GetValue<int>("MAX_RETRIES");
    var retries = 0;

    AnsiConsole
    .Status()
    .Spinner(new TennisBallSpinner())
    .Start("Searching for results", _ctx =>
    {
      // Try a few times in case of failures
      while (retries < maxRetries)
      {
        // Create the chrome driver service
        using (var driver = Driver.Create())
        {
          try
          {
            // Scrape the player ranking
            var results = ScrapePlayerResults(driver, configuration, settings, context.Name);
            AnsiConsole.Clear();
            AnsiConsole.WriteLine(results);

          }
          catch (Exception)
          {
            retries++;
          }
          finally
          {
            driver.Quit();
          }
        }
      }
    });

    return 0;
  }


  public static string ScrapePlayerResults(IWebDriver driver, IConfiguration configuration, ResultsSettings settings, string commandName)
  {
    // Navigate to the player search page

    var url = configuration.GetValue<string>("PLAYER_SEARCH_URL")?.Replace("<USTA_NUMBER>", settings.USTANumber);
    driver.Navigate().GoToUrl(url);


    var results = driver.FindElements(By.ClassName("v-grid__rows"))[0];

    var tournaments = results.FindElements(By.ClassName("v-grid__row"));

    foreach (var tournament in tournaments)
    {
      var matches = tournament.FindElements(By.ClassName("v-grid__row"));
      foreach (var match in matches)
      {
        AnsiConsole.WriteLine(match.Text);
      }
    }

    return "results";

  }
}