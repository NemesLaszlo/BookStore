using Blazored.LocalStorage;
using BookStore_UI.WebAssembly.Contracts;
using BookStore_UI.WebAssembly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookStore_UI.WebAssembly.Service
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {

        public BookRepository(HttpClient client, ILocalStorageService localStorage) : base(client, localStorage)
        {

        }
    }
}
