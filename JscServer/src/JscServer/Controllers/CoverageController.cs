using JscServer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JscServer.Controllers
{
    public class CoverageController: ControllerBase
    {
        [HttpPost]
        public object Post([FromBody] List<Coverage> coverages)
        {
            return new {My = "success" };
        }

        [HttpGet]
        public IActionResult Get([FromQuery(Name = "url")] List<string> urls)
        {
            return null;
        }
    }
}
