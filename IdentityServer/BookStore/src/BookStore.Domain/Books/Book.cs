using System;
using BookStore.Domain.Shared.Books;
using Volo.Abp.Domain.Entities;

namespace BookStore.Domain.Books
{
    public class Book :  Entity<Guid>
    {
        public string Name { get; set; }
        public DateTime PublishDate { get; set; }

        public BookType Type { get; set; }
        public float Price { get; set; }    
    }
}
