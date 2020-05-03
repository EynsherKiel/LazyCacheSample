using LazyCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LazyCacheSample
{
    public class Repository
    {
        private static int _locker = 0;

        public async Task<List<string>> GetList()
        {
            await Task.Yield();

            Interlocked.Increment(ref _locker);

            if(_locker > 1)
            {
                throw new Exception("Lazy, what's wrong with you?");
            }

            return new List<string> { Guid.NewGuid().ToString() };
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var cache = new CachingService();

            var repository = new Repository();

            var tasks = Enumerable.Range(0, 50)
                .Select(el => 
                    cache.GetOrAddAsync(
                        /* Unique key */ $"{nameof(Repository)}.{nameof(Repository.GetList)}", 
                        /* method */ repository.GetList, 
                        /* time, by default 20 minutes */ TimeSpan.FromMinutes(30)));

            var lists = await Task.WhenAll(tasks);

            if (lists.SelectMany(el => el).Distinct().Count() > 1)
            {
                throw new Exception("Sample isn't relevant");
            }

            Console.WriteLine("All great!");
        }
    }
}
