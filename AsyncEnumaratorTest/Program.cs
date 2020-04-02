using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AsyncEnumaratorTest
{
    class Program
    {
        static async Task Main()
        {
            var dataflow = new ActionBlock<(int, string)>(async ((int uri, string action) input) =>
            {
                Console.WriteLine($"Processing '{input.uri}' to {input.action}");
                switch (input.action)
                {
                    case "add":
                        await Task.Delay(3000);
                        break;
                    case "delete":
                        await Task.Delay(1000);
                        break;
                    case "update":
                        await Task.Delay(3000);
                        break;
                }
                
                Console.WriteLine($"Finish '{input.uri}' ({input.action})");
            }, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 100
            });

            IAsyncEnumerator<int> searchDocuments = RangeAsync(1, 100, 2).GetAsyncEnumerator();
            IAsyncEnumerator<int> dhsDocuments = RangeAsync(1, 100, 3).GetAsyncEnumerator();
            var searchHasValue = await searchDocuments.MoveNextAsync();
            var dhsHasValue = await dhsDocuments.MoveNextAsync();
            while (searchHasValue || dhsHasValue)
            {
                if (!searchHasValue)
                {
                    while (dhsHasValue)
                    {
                        await dataflow.SendAsync((dhsDocuments.Current, "add"));
                        dhsHasValue = await dhsDocuments.MoveNextAsync();
                    }

                    break;
                }

                if (!dhsHasValue)
                {
                    while (searchHasValue)
                    {
                        await dataflow.SendAsync((searchDocuments.Current, "delete"));
                        searchHasValue = await searchDocuments.MoveNextAsync();
                    }

                    break;
                }

                if (searchDocuments.Current == dhsDocuments.Current)
                {
                    if (await HasSameEtag(searchDocuments.Current))
                    {
                        Console.WriteLine($"({searchDocuments.Current}) skip to update");
                    }
                    else
                    {
                        await dataflow.SendAsync((searchDocuments.Current, "update"));
                    }

                    searchHasValue = await searchDocuments.MoveNextAsync();
                    dhsHasValue = await dhsDocuments.MoveNextAsync();
                }
                else if (searchDocuments.Current > dhsDocuments.Current)
                {
                    await dataflow.SendAsync((dhsDocuments.Current, "add"));
                    dhsHasValue = await dhsDocuments.MoveNextAsync();
                }
                else
                {
                    await dataflow.SendAsync((searchDocuments.Current, "delete"));
                    searchHasValue = await searchDocuments.MoveNextAsync();
                }
            }

            dataflow.Complete();
            await dataflow.Completion;
        }

        static async IAsyncEnumerable<int> RangeAsync(int start, int max, int margin = 1)
        {
            for (int i = 0; i < max; i += margin)
            {
                if (i / margin % 1000 == 0)
                {
                    await Task.Delay(1000);
                }
                
                yield return start + i;
            }
        }

        static async Task<bool> HasSameEtag(int n)
        {
            return await Task.FromResult(n % 4 == 0);
        }
    }
}
