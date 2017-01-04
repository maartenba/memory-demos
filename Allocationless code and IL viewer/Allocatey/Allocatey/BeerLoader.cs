using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Allocatey
{
    public static class BeerLoader
    {
        public static Dictionary<string, Dictionary<string, double>> Beers { get; private set; }

        static BeerLoader()
        {
            Beers = new Dictionary<string, Dictionary<string, double>>();
        }

        public static void LoadBeers()
        {
            Beers = new Dictionary<string, Dictionary<string, double>>();

            using (var reader = new JsonTextReader(
                new StreamReader(
                    File.OpenRead("Demo03\\beers.json"))))
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

        public static void LoadBeers2()
        {
            using (var reader = new JsonTextReader(
                new StreamReader(
                    File.OpenRead("D:\\Google Drive\\Samples\\Allocationless code and IL viewer\\Allocatey\\beers.json"))))
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
}