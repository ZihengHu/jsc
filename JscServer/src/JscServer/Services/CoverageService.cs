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

        Task<Coverage> GetCoverageAsync(string id);
    }

    public class JsCoverCoverageService : ICoverageService
    {
        private static SemaphoreSlim mergeSemaphore = new SemaphoreSlim(1, 1);
        private JscDbContext _context;

        public JsCoverCoverageService(JscDbContext context)
        {
            _context = context;
        }

        public async Task MergeCoverageAsync(Coverage newCoverage)
        {
            await mergeSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {                
                var oldCoverage = _context.Coverages.Where(c => c.Id == newCoverage.Id).FirstOrDefault();

                if (oldCoverage == null)
                {
                    _context.Coverages.Add(newCoverage);
                }
                else
                {
                    var oldCoverageData = JsonConvert.DeserializeObject<CoverageData>(oldCoverage.Data);
                    var newCoverageData = JsonConvert.DeserializeObject<CoverageData>(newCoverage.Data);

                    var mergedCoverageData = new CoverageData() { };

                    mergedCoverageData.LineData = oldCoverageData.LineData.Select((n, i) => n + newCoverageData.LineData.ElementAt(i));
                    mergedCoverageData.FunctionData = oldCoverageData.FunctionData.Select((n, i) => n + newCoverageData.FunctionData.ElementAt(i));

                    mergedCoverageData.BranchData = new Dictionary<string, IEnumerable<BranchData>>();
                    foreach(KeyValuePair<string, IEnumerable<BranchData>> entry in oldCoverageData.BranchData)
                    {
                        mergedCoverageData.BranchData.Add(entry.Key, entry.Value.Select((oldBranchData, i) =>
                        {
                            if (oldBranchData == null)
                            {
                                return null;
                            }

                            IEnumerable<BranchData> newBranchDataList = null;
                            newCoverageData.BranchData.TryGetValue(entry.Key, out newBranchDataList);
                            var newBranchData = newBranchDataList.ElementAt(i);

                            return new BranchData
                            {
                                Position = oldBranchData.Position,
                                NodeLength = oldBranchData.NodeLength,
                                Src = oldBranchData.Src,
                                EvalFalse = oldBranchData.EvalFalse + newBranchData.EvalFalse,
                                EvalTrue = oldBranchData.EvalTrue + newBranchData.EvalTrue,
                            };
                        }));
                    }

                    oldCoverage.Data = JsonConvert.SerializeObject(mergedCoverageData);
                }
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            finally
            {
                mergeSemaphore.Release();
            }
        }

        public async Task<Coverage> GetCoverageAsync(string id)
        {
            return await Task.FromResult(_context.Coverages.Where(c => c.Id == id).FirstOrDefault()).ConfigureAwait(false);
        }

    }

}
