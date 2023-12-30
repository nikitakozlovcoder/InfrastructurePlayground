using System.Diagnostics.Metrics;
using LoggingPlayground;
using Messaging.Contracts;
using Microsoft.AspNetCore.Mvc;
using Services.Files;

namespace InfrastructurePlayground.Controllers;

[Route("[controller]/[action]")]
public class AppController : ControllerBase
{
     private readonly IEventProducer<HelloMessage> _eventProducer;
     private readonly IAppFileProvider _appFileProvider;
     private readonly Meter _meter;

     public AppController(IEventProducer<HelloMessage> eventProducer,
          IAppFileProvider appFileProvider,
          IMeterFactory meterFactory)
     {
          _eventProducer = eventProducer;
          _appFileProvider = appFileProvider;
          _meter = meterFactory.Create(nameof(AppController));
     }

     public async Task<string> Hello()
     {
          await _eventProducer.ProduceAsync(new HelloMessage(Guid.NewGuid()));
          return "Hello world";
     }

     [HttpPost]
     public async Task<ActionResult> Upload(IFormFile file, CancellationToken ct)
     {
          _meter.CreateCounter<int>(nameof(Upload)).Add(1);
          await _appFileProvider.SaveFileAsync(file.OpenReadStream(), file.FileName, file.ContentType, ct);
          return Ok();
     }
     
     [HttpDelete("{key}")]
     public async Task<ActionResult> Delete(string key, CancellationToken ct)
     {
          _meter.CreateCounter<int>(nameof(Delete)).Add(1);
          await _appFileProvider.DeleteFileAsync(key, ct);
          return Ok();
     }
     
     [HttpGet("{key}")]
     public async Task<ActionResult> Get(string key, CancellationToken ct)
     {
          _meter.CreateCounter<int>(nameof(Get)).Add(1);
          var url = await _appFileProvider.GetFileUrlAsync(key, ct);
          return Redirect(url);
     }
}