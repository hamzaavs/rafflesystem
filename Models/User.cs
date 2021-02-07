using MongoDB.Bson;

namespace rafflesystem.Models
{
    public class User
    {
            public ObjectId _id { get; set; }
            public string email { get; set; }
            public double price { get; set; }
            public bool winner { get; set; }
            public string name { get; set; }
    }
}