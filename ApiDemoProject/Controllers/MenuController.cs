using ApiDemoProject.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiDemoProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MenuController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public MenuController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var menus = await ReadMenuFromJson();

            if (menus.Result.Count < 0)
            {
                return NotFound();
            }
            return Ok(menus);
        }

        private async Task<MenuListResponse> ReadMenuFromJson()
        {
            if (!_cache.TryGetValue("Menus", out MenuListResponse menus))
            {
                menus =
                    JsonConvert.DeserializeObject<MenuListResponse>(
                        await System.IO.File.ReadAllTextAsync("menu.json"));

                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(25), // cache will expire in 25 seconds
                    SlidingExpiration = TimeSpan.FromSeconds(5) // caceh will expire if inactive for 5 seconds
                };

                options.RegisterPostEvictionCallback(Callback, "Some state info");

                _cache.Set("Menus", menus, options);
            }

            return menus;
        }

        private void Callback(object key, object value, EvictionReason reason, object state)
        {
            Console.WriteLine("Evicted");
        }
    }
}