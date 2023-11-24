using Spectre.Console.Cli;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

[BsonIgnoreExtraElements]
public class RankingsSettings : CommandSettings
{
  [CommandOption("-n|--name")]
  public string? Name { get; set; }

  [CommandOption("-f|--format")]
  [BsonRepresentation(BsonType.String)]
  [JsonConverter(typeof(StringEnumConverter))]
  public MatchFormat? Format { get; set; }

  [CommandOption("-g|--gender")]
  [BsonRepresentation(BsonType.String)]
  [JsonConverter(typeof(StringEnumConverter))]
  public Gender? Gender { get; set; }

  [CommandOption("-l|--level")]
  public string? Level { get; set; }

  [CommandOption("-s|--section")]
  public string? Section { get; set; }

  [CommandOption("-o|--output")]
  [BsonRepresentation(BsonType.String)]
  [JsonConverter(typeof(StringEnumConverter))]
  public Output? Output { get; set; }

  [CommandOption("-e|--email")]
  public string? Email { get; set; }
  
  [CommandOption("-t|--top")]
  public int? Top { get; set; }
}

public enum MatchFormat
{
  SINGLES,
  DOUBLES
}

public enum Gender
{
  M,
  F
}

public enum Output
{
  json,
  html
}