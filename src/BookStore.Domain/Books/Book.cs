using System;
using Volo.Abp.Domain.Entities;

namespace BookStore.Domain.Books
{
    public class Book :  Entity<Guid>
    {
        public string Name { get; set; }
        public DateTime PublisDate { get; set; }
        public float Price { get; set; }    
        

      
    }
}
