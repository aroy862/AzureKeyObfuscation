using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureKeyObfuscation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var inputFile = "AvevaCodeExcercise.input.json";
            await inputFile.ObfuscateAzureSecrets();
            Console.WriteLine("Please press any key to close this window");
            Console.ReadKey();

        }
    }
}
