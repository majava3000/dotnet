using System;
using System.Collections.Generic;

/**
 * This file contains the minimum required class model to represent the HSL
 * broved bike rental data that interests of us. The model follows exactly
 * the model used in the API returned JSON, of which a small excerpt is below:
 * {
 *   "stations": [
 *     {
 *       "id": "070",
 *       "name": "Sammonpuistikko",
 *       "x": 24.9224112,
 *       "y": 60.1731473,
 *       "bikesAvailable": 4,
 *       "spacesAvailable": 10,
 *       "allowDropoff": true,
 *       "networks": [
 *         "default"
 *       ],
 *       "realTimeData": true
 *     },
 *     {
 *       "id": "071",
 *       "name": "Hietaniemenkatu",
 *       ..
 *     }
 *   ]
 * }
 *
 * Unlike most JSON returning APIs, the HSL API emits a top-level root object
 * with the 'stations' attribute under which the actual response list entries
 * live. So that the JSON to C# de-serialization works correctly without
 * requiring too much extra attributes and other work, we need to replicate
 * the structure of JSON in C# as well. Since we're only interested in a small
 * subset of the JSON information, by declaring only the members that we want
 * the deserialization process will drop everything else. The only thing that
 * you'll need to be careful is choosing the proper data type for each of the
 * member (when to use string, when to use bool and int, etc).
 *
 * Take care also when naming the members in the classes. The names must match
 * the JSON names by default (possible to override, but easier this way)
 */

namespace BikeClient
{
    public class Station
    {
        public string name;
        public int bikesAvailable;

    }

    public class Stations
    {
        public List<Station> stations;
    }
}