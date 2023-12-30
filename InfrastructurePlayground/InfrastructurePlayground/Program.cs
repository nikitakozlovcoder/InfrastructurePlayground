using Amazon.Runtime;
using Amazon.S3;
using InfrastructurePlayground.Configuration;
using KafkaFlow;
using Services.Files;
using Services.Options;

var builder = WebApplication.CreateBuilder(args);

var s3Options = builder.Configuration.GetSection("S3").Get<S3Options>()!;

builder.Services.AddAppOptions(builder.Configuration);
builder.Services.AddAppServices();
builder.Services.AddAppS3(s3Options);
builder.Services.AddAppKafka(builder.Configuration);
builder.AddAppTelemetry();

builder.Services.AddHealthChecks()
    .AddS3(x =>
    {
        x.BucketName = s3Options.Bucket;
        x.Credentials = new BasicAWSCredentials(s3Options.AccessKey, s3Options.SecretKey);
        x.S3Config = new AmazonS3Config
        {
            ForcePathStyle = true,
            ServiceURL = s3Options.Url!
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

await app.Services.GetRequiredService<IAppFileProvider>().Initialise();

app.UseHealthChecksPrometheusExporter("/healthmetrics");

var bus = app.Services.CreateKafkaBus();
await bus.StartAsync();

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.MapHealthChecks("_health");
app.MapControllers();
app.Run();

await bus.StopAsync();