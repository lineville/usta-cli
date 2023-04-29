using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

public class Utilities
{

  /// <summary>
  /// Interactive prompts to fill in missing CLI settings
  /// <summary>
  public static void InteractiveFallback(RankingsSettings settings, IConfiguration configuration, string context)
  {
    if (settings.Name == null && context != "list") 
    {
      var name = AnsiConsole.Prompt(
        new TextPrompt<string>("What's your [aqua]name[/]?"));
      settings.Name = name;
    }

    if (settings.Level == null)
    {
      var level = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Select [aqua]NTRP level[/]")
        .PageSize(10)
        .AddChoices(new[] { "3.0", "3.5", "4.0", "4.5", "5.0" }));
      settings.Level = level;
    }

    if (settings.Gender == null)
    {
      var gender = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Select [aqua]gender[/]")
        .PageSize(10)
        .AddChoices(new[] { "M 👨", "F 👩" }));
      settings.Gender = gender.StartsWith("M") ? Gender.M : Gender.F;
    }

    if (settings.Format == null)
    {
      var format = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Select [aqua]match format[/]")
        .PageSize(10)
        .AddChoices(new[]
          { $"SINGLES {(settings.Gender == Gender.M ? "🙋" : "🙋‍♀️")}",
            $"DOUBLES {(settings.Gender == Gender.M ? "👬" : "👭")}"
          }
        )
      );
      settings.Format = format.StartsWith("SINGLES") ? MatchFormat.SINGLES : MatchFormat.DOUBLES;
    }

    if (settings.Section == null)
    {
      var sectionNames = configuration.GetRequiredSection("SECTION_CODES").Get<Dictionary<string, string>>()?.Keys ?? throw new Exception("Failed to load SECTION_CODES from appsettings.json");
      var section = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Select [aqua]USTA section[/]")
        .PageSize(20)
        .AddChoices(sectionNames));
      settings.Section = section;
    }

    if (settings.Email == null && (context == "subscribe" || context == "unsubscribe"))
    {
      var email = AnsiConsole.Prompt(
        new TextPrompt<string>("What's your [aqua]email[/]?"));
      settings.Email = email;
    }
  }

  /// <summary>
  /// Interactive prompts to fill in missing CLI settings
  /// <summary>
  public static void InteractiveFallback(ResultsSettings settings, IConfiguration configuration, string context)
  {
    if (settings.USTANumber == null) 
    {
      var UstaNumber = AnsiConsole.Prompt(
        new TextPrompt<string>("What's your [aqua]USTA number[/]?"));
      settings.USTANumber = UstaNumber;
    }
  }


  /// <summary>
  /// Construct the correct URL from CLI args
  /// </summary>
  public static string BuildUSTARankingURL(RankingsSettings settings, IConfiguration configuration, string context)
  {
    var queryKeys = configuration.GetRequiredSection("QUERY_PARAMS").Get<Dictionary<string, string>>() ?? throw new Exception("Failed to load QUERY_PARAMS from appsettings.json");
    var sectionCodes = configuration.GetRequiredSection("SECTION_CODES").Get<Dictionary<string, string>>() ?? throw new Exception("Failed to load SECTION_CODES from appsettings.json");
    var ustaBaseURL = configuration.GetValue<string>("USTA_BASE_URL") ?? throw new Exception("Failed to load USTA_BASE_URL from appsettings.json");

    // Build the query string
    var queryParams = new Dictionary<string, string>()
    {
      { queryKeys["Format"], settings.Format?.ToString() ?? ""},
      { queryKeys["Gender"], settings.Gender?.ToString() ?? ""},
      { queryKeys["Level"], "level_" + settings.Level?.ToString().Replace(".", "_")},
      { queryKeys["Section"], sectionCodes[settings.Section ?? ""] }
    };

    if (context == "get")
    {
      queryParams.Add(queryKeys["Name"], settings.Name ?? "");
    }

    var url = QueryHelpers.AddQueryString(ustaBaseURL, queryParams);

    // Workaround for weird # getting ignored in QueryHelpers
    url = url.Insert(ustaBaseURL.Count(), "#") + "#tab=ntrp";
    return url;
  }

}