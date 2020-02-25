using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace gvaduha.Common
{
    public struct ServiceProcessingResult
    {
        public Guid ServiceId { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<ImageProcessorResult> ImageProcessorResults { get; set; }
    }

    public class ExternalTensorImageProcessor
    {
        private Uri[] _processorUris;
        public ExternalTensorImageProcessor(Uri[] processorUris)
        {
            _processorUris = processorUris;
        }

        static async Task<ServiceProcessingResult> GetExternalProcessorResultAsync(Uri uri)
        {
            var client = new HttpClient();
            var resp = await client.GetAsync(uri);
            resp.EnsureSuccessStatusCode();
            using var respStream = await resp.Content.ReadAsStreamAsync();
            using var sr = new StreamReader(respStream);
            var data = JsonConvert.DeserializeObject<ServiceProcessingResult>(sr.ReadToEnd());
            return data;
        }

        public async Task<IEnumerable<ServiceProcessingResult>> GetExternalProcessorsResultAsync()
        {
            var tasks = _processorUris.ToList().Select(async uri => await GetExternalProcessorResultAsync(uri));
            var results = await Task.WhenAll(tasks.ToArray());
            return results;
        }

        public static IEnumerable<ImageProcessorResult> MergeUnconditionally(IEnumerable<ServiceProcessingResult> serviceResults)
        {
            return serviceResults.SelectMany(x => x.ImageProcessorResults);
        }
    }
}
