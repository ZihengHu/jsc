using System;
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
        private static object injectLock = new object();

        private JscDbContext _context;
        private IHostingEnvironment _env;

        private string _jsCoverExecutablePath;
        private string _originalJsPath;
        private string _injectedJsPath;

        public JsCoverInjectionService(JscDbContext context, IHostingEnvironment env)
        {
            _context = context;
            _env = env;

            _jsCoverExecutablePath = Path.Combine(_env.ContentRootPath, "jscover.jar");
            _originalJsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "jsc", "original");
            _injectedJsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "jsc", "injected");
        }

        public async Task<Injection> InjectAsync(string url)
        {
            var content = await DownloadAsync(url).ConfigureAwait(false);
            // use url and content to identity a file
            var id = Identify(url + content);
            var fileName = id + ".js";

            var existingInjection = _context.Injections.Where(i => i.Id == id).FirstOrDefault();

            if (existingInjection != null)
            {
                return existingInjection;
            }
            
            var lockTaken = false;
            try
            {
                Monitor.Enter(injectLock, ref lockTaken);

                existingInjection = _context.Injections.Where(i => i.Id == id).FirstOrDefault();

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
                if (lockTaken) Monitor.Exit(injectLock);
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
            cmd.StartInfo.Arguments = string.Format("/C java -jar {0} -io {1} > {2}", _jsCoverExecutablePath, originalFile, outputFile);
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
