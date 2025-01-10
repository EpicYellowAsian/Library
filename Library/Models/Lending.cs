namespace Library.Models
{
    public class Lending
    {
        public int LendingId { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime DateOfLending { get; set; }
        public DateTime? DateOfReturn { get; set; } //Kan vara null då boken kanske är på utlåning
        public Book Book { get; set; }
        public User User { get; set; }
    }
}
