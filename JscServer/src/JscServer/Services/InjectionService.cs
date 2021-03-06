﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using JscServer.Models;
using Microsoft.AspNetCore.Hosting;
using System.Threading;

namespace JscServer.Services
{
    public interface IInjectionService
    {
        Task<Injection> InjectAsync(string url);
    }

    public class JsCoverInjectionService : IInjectionService
    {
        private static readonly SemaphoreSlim _injectSemaphore = new SemaphoreSlim(1, 1);

        private readonly JscDbContext _context;

        private readonly string _jsCoverExecutablePath;
        private readonly string _originalJsPath;
        private readonly string _injectedJsPath;

        public JsCoverInjectionService(JscDbContext context, IHostingEnvironment env)
        {
            _context = context;

            _jsCoverExecutablePath = Path.Combine(env.ContentRootPath, "jscover.jar");
            _originalJsPath = Path.Combine(env.ContentRootPath, "wwwroot", "jsc", "original");
            _injectedJsPath = Path.Combine(env.ContentRootPath, "wwwroot", "jsc", "injected");
        }

        public async Task<Injection> InjectAsync(string url)
        {
            var content = await DownloadAsync(url).ConfigureAwait(false);
            // use url and content to identity a file
            var id = Identify(url + content);
            var fileName = id + ".js";

            var existingInjection = _context.Injections.FirstOrDefault(i => i.Id == id);

            if (existingInjection != null)
            {
                return existingInjection;
            }

            await _injectSemaphore.WaitAsync().ConfigureAwait(false);
            try
            { 
                existingInjection = _context.Injections.FirstOrDefault(i => i.Id == id);

                if (existingInjection != null)
                {
                    return existingInjection;
                }

                File.WriteAllText(Path.Combine(_originalJsPath, fileName), content);
                await InjectFileAsync(fileName).ConfigureAwait(false);

                var newInjection = new Injection
                {
                    Id = id,
                    OriginalUrl = url,
                    InjectedPath = Path.Combine("jsc", "injected", fileName)
                };

                _context.Injections.Add(newInjection);
                await _context.SaveChangesAsync();

                return newInjection;
            }
            finally
            {
                _injectSemaphore.Release();
            }

        }

        private async Task<string> DownloadAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private string Identify(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }

        private async Task InjectFileAsync(string fileName)
        {
            string originalFile = Path.Combine(_originalJsPath, fileName);
            string outputFile = Path.Combine(_injectedJsPath, fileName);

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = string.Format("/C java -Dfile.encoding=UTF-8 -jar {0} -io {1} > {2}", _jsCoverExecutablePath, originalFile, outputFile);
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.WaitForExit();
            string error = await cmd.StandardError.ReadToEndAsync().ConfigureAwait(false);

            if (error.Contains("Exception"))
            {
                throw new IOException(error);
            }
        }
    }
}
