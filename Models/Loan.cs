namespace DigitalLibraryApi.Models
{
    public class Loan
    {
        public int Id { get; set; }           
        public int UserId { get; set; }           
        public int BookId { get; set; }           
        public DateTime BorrowDate { get; set; }  
        public DateTime? ReturnDate { get; set; }  
    }
}
