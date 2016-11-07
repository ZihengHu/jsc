using JscServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JscServer.Services
{
    public interface ICoverageService
    {
        Task MergeCoverageAsync(Coverage newCoverage);

        Task<Coverage> GetCoverageAsync(string url);

    }

    public class JsCoverCoverageService : ICoverageService
    {
        private static object merageLock = new object();
        private JscDbContext _context;

        public JsCoverCoverageService(JscDbContext context)
        {
            _context = context;
        }

        public async Task MergeCoverageAsync(Coverage newCoverage)
        {
            var lockTaken = false;
            try
            {
                Monitor.Enter(merageLock, ref lockTaken);

                var oldCoverage = _context.Coverages.Where(c => c.Id == newCoverage.Id).FirstOrDefault();

                if (oldCoverage == null)
                {
                    _context.Coverages.Add(newCoverage);
                }
                else
                {
                    var oldCoverageData = (JObject)JsonConvert.DeserializeObject(oldCoverage.Data);
                    var newCoverageData = (JObject)JsonConvert.DeserializeObject(newCoverage.Data);
                    
                }
                await _context.SaveChangesAsync();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(merageLock);
            }
        }

        public async Task<Coverage> GetCoverageAsync(string url)
        {
            return await Task.FromResult(_context.Coverages.Where(c => c.Url == url).OrderByDescending(c => c.InsertTime).FirstOrDefault()).ConfigureAwait(false);
        }

    }

}
