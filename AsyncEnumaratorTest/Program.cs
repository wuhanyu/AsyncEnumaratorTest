using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncEnumaratorTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IAsyncEnumerator<int> searchDocuments = RangeAsync(2, 30, 2).GetAsyncEnumerator();
            IAsyncEnumerator<int> dhsDocuments = RangeAsync(2, 30, 3).GetAsyncEnumerator();
            var searchHasValue = await searchDocuments.MoveNextAsync();
            var dhsHasValue = await dhsDocuments.MoveNextAsync();
            while (searchHasValue || dhsHasValue)
            {
                if (!searchHasValue)
                {
                    while (dhsHasValue)
                    {
                        Console.WriteLine($"({dhsDocuments.Current}) to add");
                        dhsHasValue = await dhsDocuments.MoveNextAsync();
                    }

                    break;
                }

                if (!dhsHasValue)
                {
                    while (searchHasValue)
                    {
                        Console.WriteLine($"({searchDocuments.Current}) to add");
                        searchHasValue = await searchDocuments.MoveNextAsync();
                    }

                    break;
                }

                if (searchDocuments.Current == dhsDocuments.Current)
                {
                    Console.WriteLine($"({searchDocuments.Current}) to update");
                    searchHasValue = await searchDocuments.MoveNextAsync();
                    dhsHasValue = await dhsDocuments.MoveNextAsync();
                }
                else if (searchDocuments.Current > dhsDocuments.Current)
                {
                    Console.WriteLine($"({dhsDocuments.Current}) to add");
                    dhsHasValue = await dhsDocuments.MoveNextAsync();
                }
                else
                {
                    Console.WriteLine($"({searchDocuments.Current}) to delete");
                    searchHasValue = await searchDocuments.MoveNextAsync();
                }
            }
        }

        static async IAsyncEnumerable<int> RangeAsync(int start, int count, int margin = 1)
        {
            for (int i = 0; i < count; i += margin)
            {
                await Task.Delay(10);
                yield return start + i;
            }
        }
    }
}
