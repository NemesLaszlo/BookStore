﻿using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AuthorRepository(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<bool> Create(Author entity)
        {
            await _db.Authors.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(Author entity)
        {
            _db.Authors.Remove(entity);
            return await Save();
        }

        public async Task<IList<Author>> FindAll()
        {
            var authors = await _db.Authors
                .Include(q => q.Books)
                .ToListAsync();
            return authors;
        }

        public async Task<Author> FindById(int id)
        {
            var author = await _db.Authors
                .Include(q => q.Books)
                .FirstOrDefaultAsync(q => q.Id == id);
            return author;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(Author entity)
        {
            _db.Authors.Update(entity);
            return await Save();
        }

        public async Task<bool> IsExists(int id)
        {
            return await _db.Authors.AnyAsync(q => q.Id == id);
        }

        public async Task<bool> DeleteAllBooks(IEnumerable<Book> entities)
        {
            entities.ToList().ForEach(i => {
                _db.Books.Remove(i);
                if (System.IO.File.Exists($"{_env.ContentRootPath}\\uploads\\{i.Image}"))
                {
                    System.IO.File.Delete($"{_env.ContentRootPath}\\uploads\\{i.Image}");
                }
            });
            return await Save();
        }
    }
}
