using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseController
    {
        private readonly DataContext _context;
        public BuggyController(DataContext context)
        {
            _context = context;
        }


        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "Secret text";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            var result = _context.Users.Find(-1);
            if(result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var result = _context.Users.Find(-1);

            var sReturn = result.ToString();           
            return sReturn;

        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("This was bad requst");

        }



    }
}