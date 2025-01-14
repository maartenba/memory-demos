using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TripDownMemoryLane.Demo03;

public static class BeerLoader
{
    public static Dictionary<string, Dictionary<string, double>> Beers { get; private set; }

    static BeerLoader()
    {
        Beers = new Dictionary<string, Dictionary<string, double>>();
    }

    public static void LoadBeersInsane()
    {
        Beers = new Dictionary<string, Dictionary<string, double>>();

        var json = File.ReadAllText(Path.Combine("Demo03", "beers.json")); // store all data once (string)
        var jsonArray = JArray.Parse(json); // store it a second time (as JArray)
        foreach (var token in jsonArray)
        {
            var beer = token as JObject;
            if (beer != null)
            {
                var breweryName = beer.Value<string>("brewery");
                var beerName = beer.Value<string>("name");
                var rating = beer.Value<double>("rating");

                // Add beers per brewery dictionary if it does not exist
                Dictionary<string, double> beersPerBrewery;
                if (!Beers.TryGetValue(breweryName, out beersPerBrewery))
                {
                    beersPerBrewery = new Dictionary<string, double>();
                    Beers.Add(breweryName, beersPerBrewery);
                }

                // Add beer
                if (!beersPerBrewery.ContainsKey(beerName))
                {
                    beersPerBrewery.Add(beerName, rating);
                }
            }
        }

        // string and JArray out of scope, memory traffic, expect a GC here...
    }

    public static void LoadBeersUnoptimized()
    {
        Beers = new Dictionary<string, Dictionary<string, double>>();

        using (var reader = new JsonTextReader(new StreamReader(File.OpenRead(Path.Combine("Demo03", "beers.json")))))
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // Load object from the stream
                    var beer = JObject.Load(reader);

                    var breweryName = beer.Value<string>("brewery");
                    var beerName = beer.Value<string>("name");
                    var rating = beer.Value<double>("rating");

                    // Add beers per brewery dictionary if it does not exist
                    Dictionary<string, double> beersPerBrewery;
                    if (!Beers.TryGetValue(breweryName, out beersPerBrewery))
                    {
                        beersPerBrewery = new Dictionary<string, double>();
                        Beers.Add(breweryName, beersPerBrewery);
                    }

                    // Add beer
                    if (!beersPerBrewery.ContainsKey(beerName))
                    {
                        beersPerBrewery.Add(beerName, rating);
                    }
                }
            }
        }
    }

    public static void LoadBeersOptimized()
    {
        using (var reader = new JsonTextReader(new StreamReader(File.OpenRead(Path.Combine("Demo03", "beers.json")))))
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject)
                {
                    // Load object from the stream
                    var beer = JObject.Load(reader);

                    var breweryName = beer.Value<string>("brewery");
                    var beerName = beer.Value<string>("name");
                    var rating = beer.Value<double>("rating");

                    // Add beers per brewery dictionary if it does not exist
                    Dictionary<string, double> beersPerBrewery;
                    if (!Beers.TryGetValue(breweryName, out beersPerBrewery))
                    {
                        beersPerBrewery = new Dictionary<string, double>();
                        Beers.Add(breweryName, beersPerBrewery);
                    }

                    // Add beer
                    if (!beersPerBrewery.ContainsKey(beerName))
                    {
                        beersPerBrewery.Add(beerName, rating);
                    }
                    else
                    {
                        beersPerBrewery[beerName] = rating;
                    }
                }
            }
        }
    }
}