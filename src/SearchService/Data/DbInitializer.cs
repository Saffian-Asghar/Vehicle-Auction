﻿using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDB", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(a => a.Make, KeyType.Text)
            .Key(a => a.Model, KeyType.Text)
            .Key(a => a.Color, KeyType.Text)
            .CreateAsync();

        // var count = await DB.CountAsync<Item>();
        // if (count == 0)
        // {
        //     Console.WriteLine("No data - will attempt to seed");
        //     var itemData = await File.ReadAllTextAsync("Data/auctions.json");

        //     var options = new JsonSerializerOptions
        //     {
        //         PropertyNameCaseInsensitive = true,
        //     };

        //     var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

        //     await DB.InsertAsync(items);
        // }
    }

}
