using OpenQA.Selenium.Chrome;
public class Driver
{
  /// <summary>
  /// Setup silent headless chrome driver service
  /// </summary>
  public static ChromeDriver Create()
  {
    ChromeOptions options = new ChromeOptions();
    options.AddArguments(
      "--no-sandbox",
      "--headless",
      "--disable-gpu",
      "--disable-logging",
      "--disable-dev-shm-usage",
      "--disable-crash-reporter",
      "--window-size=1920,1080",
      "--disable-extensions",
      "--log-level=3",
      "--user-agent=Chrome/73.0.3683.86",
      "--output=/dev/null"
    );

    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
    service.SuppressInitialDiagnosticInformation = true;
    service.HideCommandPromptWindow = true;
    service.EnableVerboseLogging = false;
    return new ChromeDriver(service, options);
  }
}
