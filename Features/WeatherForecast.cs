using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ASPCore_CQRS.Features.WeatherForecast
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }

    public class WeatherForecastService
    {
        private List<string> Summaries = new List<string> { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        public IList<string> GetSummaries() => Summaries;
        public void AddSummary(string summaryName)
        {
            if(!Summaries.Contains(summaryName))
                Summaries.Add(summaryName);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMediator _mediator;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            return await _mediator.Send(new WeatherForecastQuery());
        }

        [HttpPost]
        public async Task<IActionResult> Post(string newSummary)
        {
            await _mediator.Send(new WeatherForecastAddSummary { Summary = newSummary });
            return NoContent();
        }
    }

    /* Query demo MediatR classes */
    public class WeatherForecastQuery : IRequest<IEnumerable<WeatherForecast>> { }

    public class WeatherForecastQueryHandler : IRequestHandler<WeatherForecastQuery, IEnumerable<WeatherForecast>>
    {
        private readonly WeatherForecastService _service;

        public WeatherForecastQueryHandler(WeatherForecastService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<WeatherForecast>> Handle(WeatherForecastQuery request, CancellationToken cancellationToken)
        {
            var rng = new Random();
            var summaries = _service.GetSummaries();
            return await Task.FromResult(Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = summaries[rng.Next(summaries.Count())]
                }));
        }
    }

    /* Command demo MediatR classes */
    public class WeatherForecastAddSummary : IRequest
    {
        public string Summary { get; set; }
    }

    public class WeatherForecastAddSummaryHandler : AsyncRequestHandler<WeatherForecastAddSummary>
    {
        private readonly WeatherForecastService _service;

        public WeatherForecastAddSummaryHandler(WeatherForecastService service)
        {
            _service = service;
        }
        protected override Task Handle(WeatherForecastAddSummary request, CancellationToken cancellationToken)
        {
            // Do whatever validation you need here
            _service.AddSummary(request.Summary);
            return Task.CompletedTask;
        }
    }
}
