using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
namespace blazorWords.Data
{
    public class WordService : IWordService
    {
        private IMongoCollection<Words> words;
        IConfiguration _config;
        public WordService(IConfiguration config)
        {
            _config = config;
        }
        public List<Words> GetWords()
        {
            MongoClient client = new MongoClient(_config.GetConnectionString("WordsDb")); //IT is not IDISPOSIBLE
            IMongoDatabase database = client.GetDatabase("words");
            words = database.GetCollection<Words>("wordLibrary");
            
            return words.Find(word => true).ToList();
        }
    }
}