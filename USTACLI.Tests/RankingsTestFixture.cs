using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;

public class RankingsTestFixture
{
  public RankingsSettings Settings { get; set; }
  public IConfiguration Configuration { get; set; }

  public RankingsTestFixture()
  {
    Settings = new RankingsSettings()
    {
      Name = "Liam Neville",
      Format = MatchFormat.SINGLES,
      Gender = Gender.M,
      Level = "4.0",
      Section = "New England",
      Email = "email@gmail.com",
      Top = 50,
    };

    Configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.Test.json", false, true)
      .AddEnvironmentVariables()
      .Build();
  }
}