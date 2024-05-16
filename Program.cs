using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace scanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ApiClient client = new ApiClient();
                string hash = client.GetHash(args[0]);
                client.HashLookup(hash);
            } 
            catch (System.IndexOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Specify a file to process.");
            }
            Console.ReadLine();
        }
    }
}
