using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace BikeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // ProcessBikes is declared async, and returns implicit Task, so to make
            // sure that the program doesn't terminate before ProcessBikes is complete,
            // we need to call the Wait() method of the returned Task.
            // Please see https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient
            // for additional background on Tasks and async/await in general.
            ProcessBikes().Wait();
            Console.WriteLine("How did you like them apples?");
        }

        private static async Task ProcessBikes()
        {
            var client = new HttpClient();

            // NOTE: The old non GraphGL-service is undocumented by HSL
            //       If the URL is used by browser (with typical Content-Type
            //       HTTP header), the service outputs XML. With empty Content-Type
            //       or application/json, output will be in JSON. We want JSON.
            // NOTE: The service does not seem to support querying by anything, and
            //       even the GraphGL API (separate from this) only allows doing
            //       queries via station ID (which we don't have at this point).
            //       Silly API, making client do all the work and cause extra network
            //       traffic and extra latency.
            // NOTE: While original assignment used gave an non-https URL, we use the
            //       API over HTTPS since it's not the 90s anymore.
            var stringRetrieverTask = client.GetStringAsync("https://api.digitransit.fi/routing/v1/routers/hsl/bike_rental");
            // continue only after the retriever task completes and store result into
            // msg (we used GetStringAsync, so result will be a string)
            var msg = await stringRetrieverTask;
            // Use Newtonsoft's JSON toolset library to convert the data in JSON
            // there's also an async version of DeserializeObject, but let's use the sync now
            // Please see comments for the Stations class for the structure and additional
            // issues with the HSL provided JSONs.
            // If de-serialization (conversion from JSON to C# objects) succeeds, it will
            // be non-null and of type Stations. That object has exactly one member, which
            // is the list of actual stations that we want to use.
            // NOTE: There is an Async version of DeserializeObject as well, but we gain
            //       only additional complexity if we'll use it, so the DeserializeObject-
            //       call here is "plain old" synchronous one.
            var stations = JsonConvert.DeserializeObject<BikeClient.Stations>(msg);
            // Iterate over the 'stations' list of the result object, so that we can
            // access the 'name' and 'bikesAvailable' members of each station and do
            // whatever we want with them.
            foreach (var station in stations.stations) {
                // Here, string interpolation construct is used (note the $-character
                // before the openining quote) which makes the output string more readable.
                // You may also glue the output string from components if you like.
                Console.WriteLine($"Name={station.name}, bikesAvailable={station.bikesAvailable}");
            }
            // And we're done. Ugh.
        }

    }
}
