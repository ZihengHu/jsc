using JscServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JscServer.Services;

namespace JscServer.Controllers
{
    public class CoverageController: ControllerBase
    {
        private readonly ICoverageService _coverageService;

        public CoverageController(ICoverageService coverageService)
        {
            _coverageService = coverageService;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] List<Coverage> coverages)
        {
            await Task.WhenAll(coverages.Select(coverage => _coverageService.MergeCoverageAsync(coverage))).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet]
        public async Task<Coverage[]> GetAsync([FromQuery(Name = "url")] List<string> urls)
        {
            return await Task.WhenAll(urls.Select(url => _coverageService.GetCoverageAsync(url))).ConfigureAwait(false);
        }
    }
}
