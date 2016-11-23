using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JscServer.Controllers
{
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            return await Task.FromResult(Ok($"Health: OK\n{DateTime.Now.ToString("yyyy-MM-dd H:mm:ss zzz")}"));
        }
    }
}
