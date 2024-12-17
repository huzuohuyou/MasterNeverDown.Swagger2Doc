using ApiConventions.CommunityToolKit;
using ApiConventions.CommunityToolKit.Helpers;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenAPI2MD.CommunityToolkit.Example.Controllers
{
    /// <summary>
    /// À²À²À²
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HelloWorldController : CtkControllerBase
    {
        private readonly JwtHelper _jwtHelper;
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };


        public HelloWorldController(ILog logger, JwtHelper jwtHelper):base(logger)
        {
            _jwtHelper = jwtHelper;
        }

        /// <summary>
        /// SayHello
        /// </summary>
        /// <returns></returns>
        [HttpGet("SayHello", Name = nameof(SayHello))]
        [AllowAnonymous]
        public ActionResult<string> SayHello()
        {
            return default;
        }

        
    }
}