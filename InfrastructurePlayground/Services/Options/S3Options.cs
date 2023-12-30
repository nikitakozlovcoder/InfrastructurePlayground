namespace Services.Options;

public class S3Options
{
    public required string Bucket { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string Url { get; set; }
    public double ExpirationMinutes { get; set; }
    public bool UseHttps { get; set; }
}