﻿using AutoMapper;
using BookStore.Books;

namespace BookStore.Blazor;

public class BookStoreBlazorAutoMapperProfile : Profile
{
    public BookStoreBlazorAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Blazor project.
        CreateMap<BookDto, CreateUpdateBookDto>();

    }
}
