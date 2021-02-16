using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using rafflesystem.DTO;
using rafflesystem.Models;

namespace rafflesystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RaffleController: ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;
        

        public RaffleController()
        {
             var client = new MongoClient("mongodb+srv://testUser:vZbAgdspmAWXtxxt@cluster0.emi7e.mongodb.net?retryWrites=true&w=majority");
            var database = client.GetDatabase("RaffleDatabase");
            _userCollection = database.GetCollection<User>("UserCollection");

        }


        [HttpGet("all")]
        public IActionResult GetUsers(int id)
        {
            var users = _userCollection.Find(x => true).ToList();
            List<UserDTO> _dto = new List<UserDTO> {};

        
        foreach (var item in users)
        {
            _dto.Add(new UserDTO { name = item.name, _id= item._id.ToString(),winner=item.winner});      
        }
        return Ok(_dto);
        }
        [HttpPost("save")]
        public IActionResult SaveUser([FromBody] User user)
        {
            if (user.name==null || user.price==0)
            {
                return BadRequest(new {error="boş bırakmayın"});
            }
            _userCollection.InsertOne(user);
            return Ok(user);
        }
        [HttpGet("get/{id}")]
        public IActionResult GetUser(string id)
        {
            var _id =  new ObjectId(id);
            var user= _userCollection.Find(x => x._id ==_id).FirstOrDefault();
            return Ok(user);
        }
        [HttpGet("update/{id}")]
        public IActionResult UpdateUser(string id)
        {
            var _id= new ObjectId(id);
            var user = _userCollection.Find(x=>x._id == _id).FirstOrDefault();


            user.winner = true;

            _userCollection.ReplaceOne(x=>x._id == _id,user);

            return Ok(user);


        }
        [HttpGet("getwinner")]
        public IActionResult GetWinner()
        {
            var winner= _userCollection.Find(x => x.winner == true).FirstOrDefault();
            return Ok(winner);
        }
    }
}
