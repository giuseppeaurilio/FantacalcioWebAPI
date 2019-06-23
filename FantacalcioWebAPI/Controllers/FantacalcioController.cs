using FantacalcioWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantacalcioWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Administration")]
    public class FantacalcioController : Controller
    {
        private readonly ApplicationContext _context;

        public FantacalcioController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<Fantacalcio> GetAll()
        {
            return _context.Fantacalcio;
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutSquadraStorico([FromRoute] int id, [FromBody] Book book)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != book.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(book).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!BookExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}
    }
}
