using Newtonsoft.Json;
using Markdig;
using Spectre.Console;
using Spectre.Console.Json;

public record RankingsReport
{
  public Player? Player { get; init; }
  public RankingsSettings? Settings { get; init; }

  public string ToMarkDown()
  {
    // Print out the player ranking as markdown
    return @$"## {this.Player?.Name}

### {this.Settings?.Section} {(this.Settings?.Gender == Gender.M ? "Men's" : "Women's")} {this.Settings?.Level} {this.Settings?.Format?.ToString().ToLower()}

- National Rank: {this.Player?.NationalRank}
- Section Rank: {this.Player?.SectionRank}
- District Rank: {this.Player?.DistrictRank}
- Total Points: {this.Player?.TotalPoints}
- Location: {this.Player?.Location}
";
  }

  public string ToJSON() => JsonConvert.SerializeObject(this, Formatting.Indented);

  public string ToHTML()
  {
    return Markdown.ToHtml(ToMarkDown());
  }

  public void Print()
  {
    switch (Settings?.Output)
    {
      case Output.json:
        AnsiConsole.Write(new JsonText(ToJSON()));
        break;
      case Output.html:
        AnsiConsole.Write(ToHTML());
        break;
      default:
        AnsiConsole.Write(ToMarkDown());
        break;
    }
  }
}