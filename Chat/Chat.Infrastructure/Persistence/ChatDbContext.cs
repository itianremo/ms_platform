using Chat.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Chat.Infrastructure.Persistence;

public class ChatDbContext
{
    private readonly IMongoDatabase _database;

    public ChatDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoConnection"));
        _database = client.GetDatabase(configuration["MongoSettings:DatabaseName"]);
    }

    public IMongoCollection<ChatMessage> Messages => _database.GetCollection<ChatMessage>("Messages");
}
