﻿using JscServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace JscServer.Services
{
    public interface ICoverageService
    {
        Task MergeCoverageAsync(Coverage newCoverage);

        Task<Coverage> GetCoverageAsync(string id);
    }

    public class JsCoverCoverageService : ICoverageService
    {
        private static readonly SemaphoreSlim _mergeSemaphore = new SemaphoreSlim(1, 1);
        private readonly JscDbContext _context;

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsCoverCoverageService(JscDbContext context)
        {
            _context = context;
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task MergeCoverageAsync(Coverage newCoverage)
        {
            await _mergeSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var oldCoverage = _context.Coverages.FirstOrDefault(c => c.Id == newCoverage.Id);

                if (oldCoverage == null)
                {
                    _context.Coverages.Add(newCoverage);
                }
                else
                {
                    var oldCoverageData = JsonConvert.DeserializeObject<CoverageData>(oldCoverage.Data, _jsonSerializerSettings);
                    var newCoverageData = JsonConvert.DeserializeObject<CoverageData>(newCoverage.Data, _jsonSerializerSettings);

                    var mergedCoverageData = new CoverageData();

                    mergedCoverageData.LineData = oldCoverageData.LineData.Select((n, i) => n + newCoverageData.LineData.ElementAt(i));
                    mergedCoverageData.FunctionData = oldCoverageData.FunctionData.Select((n, i) => n + newCoverageData.FunctionData.ElementAt(i));

                    mergedCoverageData.BranchData = new Dictionary<string, IEnumerable<BranchData>>();
                    foreach (KeyValuePair<string, IEnumerable<BranchData>> entry in oldCoverageData.BranchData)
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

                    oldCoverage.Data = JsonConvert.SerializeObject(mergedCoverageData, _jsonSerializerSettings);
                }
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            finally
            {
                _mergeSemaphore.Release();
            }
        }

        public async Task<Coverage> GetCoverageAsync(string id)
        {
            return await Task.FromResult(_context.Coverages.FirstOrDefault(c => c.Id == id)).ConfigureAwait(false);
        }

    }

}
