using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TensorSharpStresser
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] model = File.ReadAllBytes(Settings.Default.ModelFile);
            var procUnits = Settings.Default.Processors.Split(',');

            var imgProcessors = procUnits.Select(p => new WhiteNoiceImageTensorProcessor(model, Settings.Default.ImgSize, p));

            //var tasks = imgProcessors.Select(s => new Task(() => s.RunDetectionAsync(Settings.Default.StressCycles)));
            //Task.WaitAll(tasks.ToArray());
        }
    }
}
