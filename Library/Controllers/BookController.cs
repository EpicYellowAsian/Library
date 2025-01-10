using Library.Models;
using Library.LibraryData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly LibraryContext _context; 

        public BookController(LibraryContext context)
        {
            _context = context;
        }

        //Hämta alla böcker som finns
        [HttpGet("GetAllBooksInLibrary")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync(); 
        }

        //Hämta specifik bok med hjälp av id
        [HttpGet("{id}/GetBookById")] 
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound("Error, no book found. Please try again...");
            }

            else
            {
                return Ok(book);
            }
        }

        //Lägga till ny bok i biblioteket
        [HttpPost("{id}/AddNewBook")]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new Book { Id = book.Id }, book);
        }

        //Ta bort bok från biblioteket
        [HttpDelete("RemoveBook")]
        public async Task<ActionResult<Book>> RemoveBook(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId); 

            if (book == null)
            {
                return NotFound("Error, book not found...");
            }

            //Kollar om en bok är utlånad eller ej innan det går att ta bort den
            bool isBookLoaned = await _context.Lendings.AnyAsync(l => l.BookId == bookId && l.DateOfReturn == null); 

            //Om boken inte finns tillgänglig eller är utlånad kommer det ej gå att ta bort
            if (isBookLoaned || !book.IsAvailable)
            {
                return BadRequest("This book can't be removed since it's currently on loan. Try again when the book is available."); 
            }

            else
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return Ok("The book " + book.Title + " has been successfully removed.");  
        }

        //Låna ut bok
        [HttpPut("LendBook")] 
        public async Task<IActionResult> LendBook(int bookId, int userId)
        {
            var book = await _context.Books.FindAsync(bookId);
            var user = await _context.Users.FindAsync(userId);

            if (book == null || user == null) //Om det inte går att hitta en specifik bok
            {
                return NotFound("Error, no book or user was found...");
            }

            if (!book.IsAvailable) //Om boken inte är tillgänglig för utlåning
            {
                return BadRequest("This book is currently not available for lending.");
            }

            var lending = new Lending //Skapa ny utlåning
            {
                BookId = bookId,
                UserId = userId,
                DateOfLending = DateTime.Now,
                DateOfReturn = DateTime.Today
            };

            _context.Lendings.Add(lending); //Lägg till utlåning
            book.IsAvailable = false; //Falskt då boken är ej tillgänglig pga utlåning
            await _context.SaveChangesAsync();

            return Ok("The book " + book.Title + " has been lended to" + " UserId: " + user.UserId + "."); 
        }

        //Lämna tillbaka bok
        [HttpPut("ReturnBook")]
        public async Task<IActionResult> ReturnBook(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);

            if (book == null || book.IsAvailable) //Om det inte går att hitta boken
            {
                return NotFound("Error, no book found. Please try again...");
            }

            book.IsAvailable = true; //Ändras tillbaka till sant då boken är tillgänglig nu
            await _context.SaveChangesAsync();

            return Ok("The book " + book.Title + " has been returned. Hope you enjoyed reading it!");
        }

        //Se alla utlåningar för en specifik användare
        [HttpGet("UserLendingProfile")]
        public async Task<ActionResult<IEnumerable<Lending>>> GetLendingByUser(int userId)
        {
            var lendings = await _context.Lendings
                .Where(l => l.UserId == userId)
                .Include(l => l.Book)
                .ToListAsync();

            return Ok(lendings);
        }
    }
}

