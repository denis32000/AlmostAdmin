using System;
using System.Linq;

namespace DataProcessor
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        public static ProcessorProcessedData HardProcessData(string data)
        {
            try
            {
                var length = data.Length;
                var symbols = data.ToLower().Count(c => 'a' <= c && c <= 'z');
                var digits = data.Count(c => '0' <= c && c <= '9');

                var processingResult = new ProcessorProcessedData
                {
                    JsonResult = $"DataLength:{length};SymbolsCount:{symbols};Digits:{digits}",
                    Success = true
                };

                return processingResult;
            }
            catch(Exception ex)
            {
                var processingResult = new ProcessorProcessedData
                {
                    JsonResult = ex.ToString(),
                    Success = false
                };
                return processingResult;
            }
        }
    }
}
