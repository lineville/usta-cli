﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using CommandLine;
using Spectre.Console;

public class Program
{
  /// <summary>
  /// Construct the correct URL from CLI args
  /// </summary>
  public static string BuildUSTARankingURL(CLIOptions options, IConfiguration configuration)
  {
    var queryKeys = configuration.GetRequiredSection("QUERY_PARAMS").Get<Dictionary<string, string>>() ?? throw new Exception("Failed to load QUERY_PARAMS from appsettings.json");
    var sectionCodes = configuration.GetRequiredSection("SECTION_CODES").Get<Dictionary<string, string>>() ?? throw new Exception("Failed to load SECTION_CODES from appsettings.json");
    var ustaBaseURL = configuration.GetValue<string>("USTA_BASE_URL") ?? throw new Exception("Failed to load USTA_BASE_URL from appsettings.json");

    // Build the query string
    var queryParams = new Dictionary<string, string>()
    {
      { queryKeys["Name"], options.Name ?? "" },
      { queryKeys["Format"], options.Format?.ToString() ?? ""},
      { queryKeys["Gender"], options.Gender?.ToString() ?? ""},
      { queryKeys["Level"], "level_" + options.Level?.ToString().Replace(".", "_")},
      { queryKeys["Section"], sectionCodes[options.Section ?? ""] }
    };

    var url = QueryHelpers.AddQueryString(ustaBaseURL, queryParams);

    // Workaround for weird # getting ignored in QueryHelpers
    url = url.Insert(ustaBaseURL.Count(), "#") + "#tab=ntrp";
    return url;
  }

  /// <summary>
  /// Setup silent headless chrome driver service
  /// </summary>
  public static ChromeDriver CreateChromeDriverService()
  {
    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
    service.SuppressInitialDiagnosticInformation = true;

    ChromeOptions options = new ChromeOptions();
    options.AddArguments(
      "no-sandbox",
      "headless",
      "disable-gpu",
      "disable-logging",
      "disable-dev-shm-usage",
      "window-size=1920,1080",
      "disable-extensions",
      "log-level=OFF",
      "--user-agent=Chrome/73.0.3683.86",
      "output=/dev/null"
    );

    var driver = new ChromeDriver(service, options);
    return driver;
  }

  /// <summary>
  /// Extracts the HTML element and returns a Player object
  /// </summary>
  public static Player ScrapePlayerRanking(WebDriver driver, string url, IConfiguration configuration, CLIOptions options)
  {
    var htmlElement = configuration.GetValue<string>("HTML_ELEMENT_TARGET")
      ?? throw new Exception("Failed to load HTML_ELEMENT_TARGET from appsettings.json");

    var timeout = configuration.GetValue<int>("PAGE_LOAD_TIMEOUT");

    // Navigate to the URL and wait for the page to load
    driver.Navigate().GoToUrl(url);
    var wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeout));
    wait.Until(d => d.FindElement(By.ClassName(htmlElement)));

    var elements = driver.FindElements(By.ClassName(htmlElement));

    if (elements[3].Text != options.Name) // Throw if the name doesn't match
    {
      throw new NotFoundException($"No ranking found for {options.Name}");
    }

    return new Player()
    {
      NationalRank = int.Parse(elements[0].Text),
      SectionRank = int.Parse(elements[1].Text),
      DistrictRank = int.Parse(elements[2].Text),
      Options = options
    };
  }


  /// <summary>
  /// Main entry point
  /// </summary>
  public static void Main(string[] args)
  {
    // Load appsettings.json static data
    IConfiguration configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false)
      .Build();

    // Parse command line arguments
    var options = Parser.Default.ParseArguments<CLIOptions>(args).Value
      ?? throw new Exception("Failed to parse command line arguments");

    if (options.Name == null)
    {
      var name = AnsiConsole.Prompt(
        new TextPrompt<string>("Name 👉"));
      options.Name = name;
    }

    if (options.Level == null)
    {
      var level = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("NTRP level 👇")
        .PageSize(10)
        .AddChoices(new[] { "3.0", "3.5", "4.0", "4.5", "5.0" }));
      options.Level = level;
    }

    if (options.Gender == null)
    {
      var gender = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Gender 👇")
        .PageSize(10)
        .AddChoices(new[] { "M 👨", "F 👩" }));
      options.Gender = gender.StartsWith("M") ? Gender.M : Gender.F;
    }

    if (options.Format == null)
    {
      var format = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Format 👇")
        .PageSize(10)
        .AddChoices(new[]
          { $"SINGLES {(options.Gender == Gender.M ? "🙋" : "🙋‍♀️")}",
            $"DOUBLES {(options.Gender == Gender.M ? "👬" : "👭")}"
          }
        )
      );
      options.Format = format.StartsWith("SINGLES") ? MatchFormat.SINGLES : MatchFormat.DOUBLES;
    }

    if (options.Section == null)
    {
      var sectionNames = configuration.GetRequiredSection("SECTION_CODES").Get<Dictionary<string, string>>()?.Keys ?? throw new Exception("Failed to load SECTION_CODES from appsettings.json");
      var section = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Section 👇")
        .PageSize(20)
        .AddChoices(sectionNames));
      options.Section = section;
    }

    // Construct the URL from cli args
    var url = BuildUSTARankingURL(options, configuration);

    var maxRetries = configuration.GetValue<int>("MAX_RETRIES");
    var retries = 0;

    Player? player = null;

    AnsiConsole
    .Status()
    .Spinner(new TennisBallSpinner())
    .Start("Searching for ranking", _ctx =>
    {
      while (retries < maxRetries && player == null)
      {
        // Create the chrome driver service
        using (var driver = CreateChromeDriverService())
        {
          try
          {
            // Scrape the player ranking
            player = ScrapePlayerRanking(driver, url, configuration, options);
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

    if (player == null)
    {
      throw new Exception($"Failed to find ranking for {options.Name}");
    }
    else
    {
      Console.WriteLine(player.ToString());
    }

  }
}