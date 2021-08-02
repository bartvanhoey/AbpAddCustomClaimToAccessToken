using AutoMapper;
using BookStore.Application.Contracts.Books;

namespace BookStore.Blazor
{
    public class BookStoreBlazorAutoMapperProfile : Profile
    {
        public BookStoreBlazorAutoMapperProfile()
        {
            CreateMap<BookDto, UpdateBookDto>();
            CreateMap<BookDto, CreateBookDto>();
        }
    }
}
