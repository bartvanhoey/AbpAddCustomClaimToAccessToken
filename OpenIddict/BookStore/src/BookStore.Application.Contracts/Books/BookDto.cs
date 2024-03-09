using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace BookStore.Books
{
    public class BookDto :  AuditedEntityDto<Guid>
    {
        public string? Name { get; set; }
        public BookType Type { get; set; }
        public DateTime PublishDate { get; set;  }
        public float Price { get; set; }
    }
}