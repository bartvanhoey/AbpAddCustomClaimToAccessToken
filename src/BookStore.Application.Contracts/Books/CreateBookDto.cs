using System;
using System.ComponentModel.DataAnnotations;
using BookStore.Domain.Shared.Books;

namespace BookStore.Application.Contracts.Books
{
    public class CreateBookDto
    {

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PublishDate { get; set; } = DateTime.Now;

        [Required]
        public BookType Type { get; set; } = BookType.Undefined;

        [Required]
        public float Price { get; set; }
    }
}
