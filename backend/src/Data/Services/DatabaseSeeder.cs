using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json; // Requires the NuGet package in the Data project
using Realworlddotnet.Core.Entities;
using Realworlddotnet.Data.Contexts;

namespace Realworlddotnet.Data.Services;

public interface IDatabaseSeeder
{
  Task SeedAsync();
}

public class DatabaseSeeder : IDatabaseSeeder
{
  private readonly ConduitContext _context;
  private readonly ILogger<DatabaseSeeder> _logger;
  private readonly IPasswordHasher<User> _passwordHasher;
  private readonly string _seedDataPath;

  public DatabaseSeeder(
      ConduitContext context,
      ILogger<DatabaseSeeder> logger,
      IPasswordHasher<User> passwordHasher,
      string contentRootPath)
  {
    _context = context;
    _logger = logger;
    _passwordHasher = passwordHasher;
    // Ensure contentRootPath is passed from the WebAPI project correctly
    _seedDataPath = Path.Combine(contentRootPath, "seed-data.json");
  }

  public async Task SeedAsync()
  {
    try
    {
      if (!File.Exists(_seedDataPath))
      {
        _logger.LogWarning("Seed file missing at {Path}", _seedDataPath);
        return;
      }

      if (await _context.Users.AnyAsync()) return;

      var seedDataJson = await File.ReadAllTextAsync(_seedDataPath);
      var seedItems = JsonConvert.DeserializeObject<List<SeedDataItem>>(seedDataJson);

      if (seedItems == null) return;

      await SeedUsersAsync(seedItems);
      await _context.SaveChangesAsync();

      await SeedTagsAsync(seedItems);
      await _context.SaveChangesAsync();

      await SeedArticlesAsync(seedItems);
      await _context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Seeding failed");
      throw;
    }
  }

  private async Task SeedUsersAsync(List<SeedDataItem> seedItems)
  {
    var items = seedItems.Where(x => x.Model == "users.user");
    foreach (var item in items)
    {
      var data = JsonConvert.DeserializeObject<UserSeedData>(item.Fields.ToString()!);
      if (data == null) continue;

      var user = new User
      {
        Username = data.Username,
        Email = data.Email,
        // Using Identity's hasher
        Password = _passwordHasher.HashPassword(null!, data.Password),
        Bio = data.Bio,
        Image = data.Image ?? ""
      };
      _context.Users.Add(user);
    }
  }

  private async Task SeedTagsAsync(List<SeedDataItem> seedItems)
  {
    var tagItems = seedItems.Where(x => x.Model == "articles.tag");
    foreach (var item in tagItems)
    {
      var data = JsonConvert.DeserializeObject<TagSeedData>(item.Fields.ToString()!);
      if (data == null) continue;

      // Use data.Id here to match the updated TagSeedData class below
      var existingTag = await _context.Tags.FindAsync(data.Id);
      var isTracked = _context.Tags.Local.Any(t => t.Id == data.Id);

      if (existingTag == null && !isTracked)
      {
        _context.Tags.Add(new Tag(data.Id));
      }
    }
  }

  private async Task SeedArticlesAsync(List<SeedDataItem> seedItems)
  {
    var items = seedItems.Where(x => x.Model == "articles.article");
    var author = await _context.Users.FirstOrDefaultAsync();

    foreach (var item in items)
    {
      var data = JsonConvert.DeserializeObject<ArticleSeedData>(item.Fields.ToString()!);
      if (data == null || author == null) continue;

      var article = new Article(data.Title, data.Description, data.Body)
      {
        Slug = data.Slug,
        Author = author,
        CreatedAt = data.Created_At,
        UpdatedAt = data.Updated_At
      };
      _context.Articles.Add(article);
    }
  }
}

// These DTOs must remain outside the class or in a separate file
public class SeedDataItem
{

  public string Model { get; set; } = "";
  public int Pk { get; set; }
  public object Fields { get; set; } = new();
}

public class UserSeedData
{
  public string Username { get; set; } = "";
  public string Email { get; set; } = "";
  public string Password { get; set; } = "";
  public string Bio { get; set; } = "";
  public string? Image { get; set; }
}

public class TagSeedData
{
  [JsonProperty("id")]
  public string Id { get; set; } = "";

  [JsonProperty("name")]
  public string Name { get; set; } = "";
}

public class ArticleSeedData
{
  public string Title { get; set; } = "";
  public string Description { get; set; } = "";
  public string Body { get; set; } = "";
  public string Slug { get; set; } = "";
  public DateTime Created_At { get; set; }
  public DateTime Updated_At { get; set; }
}