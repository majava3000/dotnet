using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq; // .Any
using System.IO; // File and friends

namespace BikeClient
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message)
            : base(message)
        {
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) {
                throw new System.ArgumentException("No command line parameters");
            } else if (args[0].Any(char.IsDigit)) {
                throw new System.ArgumentException("Command line parameter should not have a number in it");
            }

            // ProcessBikes is declared async, and returns implicit Task, so to make
            // sure that the program doesn't terminate before ProcessBikes is complete,
            // we need to call the Wait() method of the returned Task.
            // Please see https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient
            // for additional background on Tasks and async/await in general.
            // try {
            //     ProcessBikes().Wait();
            // // } catch (NotFoundException ex) {
            // //     Console.WriteLine("Failed while doing query: " + ex.Message);
            // } catch (Exception ex) {
            //     Console.WriteLine("Other error: " + ex.Message);
            // }

            try {
                Task<int> bikeReadingProcess = TestFileRead(args[0]);
                Console.WriteLine(bikeReadingProcess.Result);
            } catch (Exception ex) {
                Console.WriteLine("Other error: " + ex.Message);
            }

            Console.WriteLine("How did you like them apples?");
        }

        // On modern .NET vs Unity:
        //   At this writing, Unity might support targetting the .NET 4.6
        //   API (via which support for async/await comes). Please see
        //   https://blogs.unity3d.com/2017/07/11/introducing-unity-2017/
        //   (C#6/.NET 4.6 support is still marked "experimental").
        //   Unity forums indicate that Unity's existing coroutine
        //   implementation is very much more light-weight compared to
        //   Task/async/await, so while the support is there, you'll want
        //   think twice before using the Task/async/await pattern in
        //   Unity. Whether your Unity target will support the features
        //   is another matter (Android, iOS, etc).
        //   More information here:
        //   https://forum.unity3d.com/forums/experimental-scripting-previews.107/
        //
        // The API of the function here is close enough to the interface
        // that will need to be satisfied (must return Task<int>) but the
        // assignment expects a separate class to contain this method,
        // while this code just has one class with two different methods.
        // Some glue and assembly required. There are many possible ways
        // of implementing the data process here (this is just one and
        // not the most efficient).
        private static async Task<int> TestFileRead(string nameQuery)
        {
            // Having hard-coded paths within implementation might lead to
            // less maintanable code in the long run (imagine a large code
            // base with a load of hard-coded data and then the locations
            // of the files need to be changed, or the paths need to be
            // renamed).
            //
            // Use verbatim string literal (@"...") so that if there
            // are windows path separators ('\directory\subdirectory\') in
            // the string, the backslashes will not be treated as string
            // escapes (you'd have to use "\\directory\\subdirectory\\')
            const string path = @"bikes.txt";

            // This is pretty optional, since the StreamReader will throw up
            // the same exception if it open the file (probably), but should
            // you want to use some "default" data instead if a file is
            // missing, this is how to do the check (a high-scores file
            // might not exist when your game starts the first time for example)
            if (!File.Exists(path))
            {
                throw new System.IO.FileNotFoundException(path);
            }

            // StreamReader (like many other resources that interface with the
            // operating system) implement the IDisposable-interface, which means
            // that special care needs to be used when ending the use of such
            // objects (the runtime may not automatically detect when the objects
            // must be collected perhaps). The 'using' keyword in C# is a construct
            // that will make sure that anything in the 'using' statement will
            // get .Dispose() called on them at the end. For more information, please
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement
            // (also for alternatives if using 'using' seems difficult)
            // NOTE: StreamReader supports multiple different constructors. We use
            //       the one that is the simplest for our usage case. By default,
            //       the StreamReader is setup to decode from UTF-8 encoded data
            //       (which is the most common text encoding format utilized in
            //       modern times outside Microsoft environments). This is convenient
            //       since the text file provided is also encoded in UTF-8 (verified
            //       by 'file' command line tool). Internally strings in C# are not
            //       stored as UTF-8 but something else, so such conversions are quite
            //       often necessary when dealing with textual data.
            // NOTE: Normally for small local files like this, we'd prefer to use
            //       the sync interface since it would be faster. However, since
            //       we must conform to the interface that requires us to return
            //       Task<int>, we'll need to use the async version here.
            using (var sr = new StreamReader(path)) {
                // ReadLine will return a new string representing the line contents
                // from the stream. Once the StreamReader reaches end of the file
                // the method will return 'null' (invalid value marker). The line
                // will NOT contain the newline indicator at the end. Since this
                // is an Async interface, it will require a relatively new C#
                // implementation.
                var line = await sr.ReadLineAsync();
                while (line != null) {
                    // we have something in line, so process it
                    Console.WriteLine("Got '"+line+"'");

                    // The string contains "string:number". We next need to
                    // isolate both of the components, and verify that everything
                    // necessary is present and that the number is actually a
                    // number. A more future proof solution would be using
                    // regular expressions (see System.Text.RegularExpressions),
                    // but we'll do this using simple language constructs.
                    var components = line.Split(':');
                    // If all went well, components will be an array of two
                    // strings (but might be anything from 1 to infinity)
                    if (components.Length == 2) {
                        var name = components[0];
                        int bikesAvailable;
                        // TryParse method of the primitive type 'int' can be
                        // used to both convert the string into an int, and
                        // also to verify that the string does not contain any non-digits
                        // However, since the method would need to return both the
                        // converted integer and the status of conversion, the method
                        // uses 'out parameter' to tell it where to place the conversion
                        // result. Think of it as "where to place the result of the operation"
                        bool secondIsANumber = int.TryParse(components[1], out bikesAvailable);

                        // Check that we can do the comparison. We'll assume that
                        // empty place names are not valid, and also require that the
                        // second component converted correctly and is zero or positive
                        // number.
                        if ((name.Length > 0) &&
                            secondIsANumber &&
                            bikesAvailable >= 0) {

                            // Components seem valid. Do the query comparison
                            // To make the life simpler to the user, use case-insensitive
                            // comparison (we cannot use '==' operator for that). Lucky us
                            // there is a static function for this (String.Compare) that
                            // works for us here (this is actually rather difficult problem
                            // to solve in all the different cultures that your program may
                            // run).
                            // String.Compare(a, b, shouldIgnoreCase) == 0 if strings are
                            // equivalent.
                            if (String.Compare(name, nameQuery, true) == 0) {
                                // We have a hit.
                                Console.WriteLine($"Hit({nameQuery}) => ({name},{bikesAvailable})");
                                // We don't need to continue our search, so return with the
                                // result. All opened files in using(...) will be automatically
                                // closed and released.
                                return bikesAvailable;
                            } else {
                                // The input line from the file was valid, but didn't match
                                // our query
                            }
                        } else {
                            // The input line from the file wasn't valid
                            Console.WriteLine($"String '{line}' has invalid content (1)");
                        }
                    } else {
                        // String didn't contain exactly two components
                        Console.WriteLine($"String '{line}' has invalid content (2)");
                        // Note that we don't terminate the loop here, but will
                        // continue with next line.
                    }

                    // attempt to load the next line
                    line = sr.ReadLine();
                }

                // This point is reached once we've processed all of the lines and
                // the stream reader has indicated end-of-stream (via null)

                Console.WriteLine("All of stream processed");
            } // All IDisposables within 'using (xx)' Dispose()'d here

            // We end up here if we processed all of the input, but didn't find the
            // queried name

            throw new NotFoundException("Station not found");
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
            // test exception throwing
            // throw new NotFoundException("Station not found");
        }

    }
}
