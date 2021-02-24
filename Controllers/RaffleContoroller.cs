using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Driver;
using rafflesystem.DTO;
using rafflesystem.Models;

namespace rafflesystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RaffleController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IDistributedCache _cache;


        public RaffleController(IDistributedCache cache)
        {
            var client = new MongoClient("mongodb+srv://testuser:jHesCERjSVaknTCQ@cluster0.emi7e.mongodb.net?retryWrites=true&w=majority");
            var database = client.GetDatabase("RaffleDatabase");
            _userCollection = database.GetCollection<User>("UserCollection");

            _cache = cache;

        }


        [HttpGet("all")]
        public async Task<IActionResult> GetUsers(int id)
        {

            string cache_key = "all_list";
            var allList = await _cache.GetStringAsync(cache_key); // null

            if (allList != null)
            {
                var cachedData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserDTO>>(allList);
                return Ok(cachedData);
            }

            var users = _userCollection.Find(x => true).ToList();
            List<UserDTO> _dto = new List<UserDTO> { };


            foreach (var item in users)
            {
                _dto.Add(new UserDTO { name = item.name, _id = item._id.ToString(), winner = item.winner });
            }


            var cacheOption = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddSeconds(5));
            var serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(_dto);

            await _cache.SetStringAsync(cache_key, serializedData, cacheOption);


            return Ok(_dto);
        }
        [HttpPost("save")]
        public IActionResult SaveUser([FromBody] User user)
        {
            if (user.name == null || user.price == 0)
            {
                return BadRequest(new { error = "boş bırakmayın" });
            }
            _userCollection.InsertOne(user);


            return Ok(user);
        }
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {

            string cache_key = "get_" + id;
            var allList = await _cache.GetStringAsync(cache_key); // null

            if (allList != null)
            {
                var cachedData = Newtonsoft.Json.JsonConvert.DeserializeObject<UserDTO>(allList);
                return Ok(cachedData);
            }


            var _id = new ObjectId(id);
            var user = _userCollection.Find(x => x._id == _id).FirstOrDefault();

            UserDTO userDTO = new UserDTO { };
            userDTO.name = user.name;
            userDTO._id = user._id.ToString();
            userDTO.winner = user.winner;


            var cacheOption = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(1));
            var serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(userDTO);

            await _cache.SetStringAsync(cache_key, serializedData, cacheOption);

            return Ok(userDTO);


        }
        [HttpGet("update/{id}")]
        public IActionResult UpdateUser(string id)
        {
            var _id = new ObjectId(id);
            var user = _userCollection.Find(x => x._id == _id).FirstOrDefault();


            user.winner = true;

            _userCollection.ReplaceOne(x => x._id == _id, user);

            return Ok(user);


        }
        [HttpGet("getwinner")]
        public IActionResult GetWinner()
        {
            var winner = _userCollection.Find(x => x.winner == true).FirstOrDefault();
            return Ok(winner);
        }
    }
}
