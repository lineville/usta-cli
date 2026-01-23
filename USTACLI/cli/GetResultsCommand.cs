using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Spectre.Console;
using Spectre.Console.Cli;

public class GetResultsCommand : Command<ResultsSettings>
{
#nullable disable
  public override int Execute(CommandContext context, ResultsSettings settings)
  {
#nullable enable

    // Load appsettings.json static data
    IConfiguration configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false)
      .Build();

    Utilities.InteractiveFallback(settings, configuration, context.Name);

    var maxRetries = configuration.GetValue<int>("MAX_RETRIES");
    var retries = 0;
    Panel? overview = null;
    Panel? results = null;

    AnsiConsole
    .Status()
    .Spinner(new TennisBallSpinner())
    .Start("Searching for results", _ctx =>
    {
      // Try a few times in case of failures
      while (retries < maxRetries && overview == null)
      {
        // Create the chrome driver service
        using (var driver = Driver.Create())
        {
          try
          {
            // Scrape the player ranking
            overview = ScrapePlayerOverview(driver, configuration, settings);
          }
          catch (Exception ex)
          {
            retries++;
          }
          finally
          {
            driver.Quit();
          }
        }
      }

      retries = 0;
      while (retries < maxRetries && results == null)
      {
        // Create the chrome driver service
        using (var driver = Driver.Create())
        {
          try
          {
            // Scrape the player ranking
            results = ScrapePlayerResults(driver, configuration, settings);
          }
          catch (Exception ex)
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

    if (overview == null || results == null)
    {
      throw new Exception($"Failed to find results for {settings.USTANumber}");
    }
    else
    {

      AnsiConsole.Clear();
      AnsiConsole.Write(overview);
      AnsiConsole.Write(results);
    }

    return 0;
  }

  public static Panel ScrapePlayerOverview(IWebDriver driver, IConfiguration configuration, ResultsSettings settings)
  {
    var url = configuration.GetValue<string>("PLAYER_SEARCH_URL")?.Replace("<USTA_NUMBER>", settings.USTANumber);
    driver.Navigate().GoToUrl(url);

    // Find the element that is a link with the text "SEE WTN PROFILE"
    var link = driver.FindElement(By.LinkText("SEE WTN PROFILE")).GetAttribute("href");

    // Click the link
    driver.Navigate().GoToUrl(link);

    var matchActivity = driver.FindElement(By.ClassName("match-activity_matchActivityTable__1fleO")).FindElements(By.TagName("tr"));

    var ratingRanges = driver.FindElement(By.ClassName("profile-styles_highsLowsGraphsWrapper__1oUhG")).FindElements(By.ClassName("ratings-range-graphic_wrapper__1gDq5"));

    // Find the element to toggle just singles results
    var matchFormatToggle = driver.FindElement(By.XPath(".//button/div[text()='Both']"));
    matchFormatToggle.Click();

    var dropdownOptions = driver.FindElement(By.ClassName("dropdown_dropdownMenu__eTZcT"));
    var singlesOption = dropdownOptions.FindElement(By.XPath(".//a[text()='Singles']"));
    singlesOption.Click();

    var singlesResults = driver.FindElement(By.ClassName("results-doughnut-chart_resultsDoughnutChartKeys__2X2AK")).FindElements(By.TagName("li"));
    var singlesWins = singlesResults[0].FindElements(By.TagName("span"))[0].Text;
    var singlesLosses = singlesResults[1].FindElements(By.TagName("span"))[0].Text;

    matchFormatToggle.Click();
    dropdownOptions = driver.FindElement(By.ClassName("dropdown_dropdownMenu__eTZcT"));
    var doublesOption = dropdownOptions.FindElement(By.XPath(".//a[text()='Doubles']"));
    doublesOption.Click();

    var doublesResults = driver.FindElement(By.ClassName("results-doughnut-chart_resultsDoughnutChartKeys__2X2AK")).FindElements(By.TagName("li"));
    var doublesWins = doublesResults[0].FindElements(By.TagName("span"))[0].Text;
    var doublesLosses = doublesResults[1].FindElements(By.TagName("span"))[0].Text;

    var grid = new Grid().AddColumns(new GridColumn(), new GridColumn());

    // Add the match activity to the panel
    foreach (var row in matchActivity)
    {
      var header = row.FindElement(By.TagName("th"));
      var data = row.FindElement(By.TagName("td"));
      grid.AddRow(new Text[] { new Text(header.Text).LeftJustified(), new Text(data.Text, new Style(Color.Cyan1)).RightJustified() });
    }

    // Add the WTN rating ranges to the panel
    grid.AddRow(new Text[] { new Text("Singles WTN Rating Range:").LeftJustified(), new Text(ratingRanges[0].FindElement(By.TagName("h2")).Text, new Style(Color.Cyan1)).RightJustified() });
    grid.AddRow(new Text[] { new Text("Doubles WTN Rating Range:").LeftJustified(), new Text(ratingRanges[1].FindElement(By.TagName("h2")).Text, new Style(Color.Cyan1)).RightJustified() });

    // Add the results to the panel
    grid.AddRow(new Text[] { new Text("Singles Wins").LeftJustified(), new Text(singlesWins, new Style(Color.Cyan1)).RightJustified() });
    grid.AddRow(new Text[] { new Text("Singles Losses").LeftJustified(), new Text(singlesLosses, new Style(Color.Cyan1)).RightJustified() });
    grid.AddRow(new Text[] { new Text("Doubles Wins").LeftJustified(), new Text(doublesWins, new Style(Color.Cyan1)).RightJustified() });
    grid.AddRow(new Text[] { new Text("Doubles Losses").LeftJustified(), new Text(doublesLosses, new Style(Color.Cyan1)).RightJustified() });

    var playerName = driver.FindElement(By.ClassName("profile-header_textWhite__2xHeI")).Text;

    var panel = new Panel(grid)
    {
      Header = new PanelHeader($"{playerName} Overview").Centered(),
      Border = BoxBorder.Heavy,
      BorderStyle = Style.Parse("cyan"),
    };

    return panel;
  }


  public static Panel ScrapePlayerResults(IWebDriver driver, IConfiguration configuration, ResultsSettings settings)
  {
    var url = configuration.GetValue<string>("PLAYER_SEARCH_URL")?.Replace("<USTA_NUMBER>", settings.USTANumber);
    driver.Navigate().GoToUrl(url);

    // Find the element that is a link with the text "SEE WTN PROFILE"
    var link = driver.FindElement(By.LinkText("SEE WTN PROFILE")).GetAttribute("href");

    // Click the link and navigate to the "results" tab
    driver.Navigate().GoToUrl(link);

    Thread.Sleep(1000);

    // Find the element that is a link with the text "results" and click it
    var resultsLink = driver.FindElement(By.ClassName("results-tab"));
    resultsLink.Click();

    Thread.Sleep(1000);

    // Find and click the element that says "show more" until it does not exist
    var showMore = driver.FindElement(By.XPath(".//button[.='SHOW MORE']"));

    while (showMore != null)
    {
      showMore.Click();
      Thread.Sleep(1000);

      try
      {
        showMore = driver.FindElement(By.XPath(".//button[.='SHOW MORE']"));
      }
      catch (NoSuchElementException)
      {
        showMore = null;
      }
    }

    var openAllResults = driver.FindElement(By.XPath(".//span[.='Open all results']"));
    openAllResults.Click();


    var allTournamentsContainer = driver.FindElement(By.ClassName("profile-styles_tournamentsList__1CKYW"));

    var tournaments = allTournamentsContainer.FindElements(By.ClassName("profile-styles_matchResultContainer__cLFWI"));

    var grid = new Grid().AddColumns(new GridColumn(), new GridColumn());

    foreach (var tournament in tournaments)
    {
      var tournamentName = tournament.FindElement(By.XPath(".//article/h5/span")).Text;
      var tournamentDate = tournament.FindElement(By.ClassName("profile-styles_matchResultsTournamentDate__2njIn")).Text;

      var tournamentResults = tournament.FindElement(By.ClassName("profile-styles_cardContainer__2grpO")).FindElements(By.TagName("li"));

      foreach (var match in tournamentResults)
      {
        var player = match.FindElements(By.ClassName("matchCardPlayer_fullname__2iFgh"))[0].Text;
        var opponent = match.FindElements(By.ClassName("matchCardPlayer_fullname__2iFgh"))[1].Text;

        var matchStatus = match.FindElement(By.ClassName("matchCard_matchStatus__20xdp"));
        var victory = matchStatus.FindElements(By.XPath(".//p/svg")) != null;

        grid.AddRow(new Text[] { new Text(player, new Style(victory ? Color.Green : Color.Red)).LeftJustified(), new Text(opponent, new Style(victory ? Color.Green : Color.Red)).RightJustified() });
      }

    }

    // TODO figure out how to best display the results

    // TODO go to the Overview tab and scrape other info there use panels and charts to replicate this

    var panel = new Panel(grid)
    {
      Header = new PanelHeader($"Match Results").Centered(),
      Border = BoxBorder.Heavy,
      BorderStyle = Style.Parse("cyan"),
    };

    return panel;
  }
}