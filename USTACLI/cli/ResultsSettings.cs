using Spectre.Console.Cli;

public class ResultsSettings : CommandSettings
{
  [CommandOption("-u|--usta-number <USTA_NUMBER>")]
  public string? USTANumber { get; set; }
}