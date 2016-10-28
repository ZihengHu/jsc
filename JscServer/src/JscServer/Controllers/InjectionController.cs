using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JscServer.Models;
using JscServer.Services;

namespace JscServer.Controllers
{
    public class InjectionController : ControllerBase
    {
        private readonly IInjectionService _injectionService;
        public InjectionController(IInjectionService injectionService)
        {
            _injectionService = injectionService;
        }

        [HttpGet]
        public async Task<Injection[]> GetAsync([FromQuery(Name = "url")] List<string> urls)
        {
            return await Task.WhenAll(urls.Select(url => _injectionService.InjectAsync(url))).ConfigureAwait(false);
        }

    }
}
