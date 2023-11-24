namespace USTACLI.Tests;

using Xunit;

public class RankingsTests : IClassFixture<RankingsTestFixture>
{
  private readonly RankingsTestFixture Fixture;

  public RankingsTests(RankingsTestFixture fixture)
  {
    Fixture = fixture;
  }

  [Fact]
  public void TestBuildUSTARankingURL()
  {
    var url = Utilities.BuildUSTARankingURL(Fixture.Settings, Fixture.Configuration, "get");

    Assert.Equal("https://www.usta.com/en/home/play/rankings.html#?ntrp-matchFormat=SINGLES&ntrp-rankListGender=M&ntrp-ntrpPlayerLevel=level_4_0&ntrp-sectionCode=S45&ntrp-searchText=Liam%20Neville#tab=ntrp&ntrp-page=1", url);
  }

  [Fact]
  public void TestChromeDriverService()
  {
    var driver = Driver.Create();

    Assert.NotNull(driver);

    driver.Quit();
    driver.Dispose();
  }

  [Fact]
  public void TestGetPlayerRanking()
  {
    var driver = Driver.Create();
    var command = new GetRankingsCommand();

    var player = command.ScrapePlayerRanking(driver, Fixture.Configuration, Fixture.Settings, "get");

    Assert.NotNull(player);

    driver.Quit();
    driver.Dispose();
  }

  [Fact]
  public void TestListPlayerRanking()
  {
    var driver = Driver.Create();
    var command = new ListRankingsCommand();

    var players = command.ScrapeRankings(driver, Fixture.Configuration, Fixture.Settings, "list");

    Assert.NotEmpty(players);

    driver.Quit();
    driver.Dispose();
  }

  [Fact]
  public async void TestSubUnsub()
  {
    var subscribeCommand = new SubscribeRankingsCommand();
    var getSubscribersCommand = new ListSubscribersCommand();
    var unsubscribeCommand = new UnsubscribeRankingsCommand();

    var subscribers = await getSubscribersCommand.GetSubscribers(Fixture.Configuration);

    Assert.NotEmpty(subscribers);

    await subscribeCommand.AddSubscriber(Fixture.Configuration, Fixture.Settings);

    var subscribersOneMore = await getSubscribersCommand.GetSubscribers(Fixture.Configuration);

    Assert.Equal(subscribers.Count + 1, subscribersOneMore.Count);

    await unsubscribeCommand.DeleteSubscriber(Fixture.Configuration, Fixture.Settings);

    var subscribersOneLess = await getSubscribersCommand.GetSubscribers(Fixture.Configuration);

    Assert.Equal(subscribersOneMore.Count - 1, subscribersOneLess.Count);
  }
}