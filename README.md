# BookStore

Book Store application with book and author management with Customer and Administrator users. With .NET Core and Blazor.

- .NET Core
- AutoMapper
- NLog
- Swagger UI
- Entity Framework Core
- Repository Pattern
- Identity and Authentication
- Security with JSON Web Token

#### Endpoints for the Backend

| Entity | Type   | URL                | Description                                               | Success              | Failure                                                              | Authorize                |
| ------ | ------ | ------------------ | --------------------------------------------------------- | -------------------- | -------------------------------------------------------------------- | ------------------------ |
| User   | POST   | api/users/login    | User Login. You are able to login with username or email. | 200 OK and the Token | 500 InternalServerError with a message                               | No                       |
|        | POST   | api/users/register | User Registration (as a Costumer)                         | 201 Created          | 500 InternalServerError with a message                               | No                       |
| Author | GET    | api/authors        | Get all authors.                                          | 200 OK               | 500 InternalServerError with a message                               | Yes                      |
|        | GET    | api/authors/:id    | Get an author by id.                                      | 200 OK               | 404 NotFound; 500 InternalServerError with a message                 | Yes                      |
|        | POST   | api/authors        | Creates an author.                                        | 201 Created          | 400 BadRequest; 500 InternalServerError with a message               | Yes (Only Administrator) |
|        | PUT    | api/authors/:id    | Update an author by id.                                   | 204 NoContent        | 400 BadRequest; 404 NotFound; 500 InternalServerError with a message | Yes (Only Administrator) |
|        | DELETE | api/authors/:id    | Delete an author by id.                                   | 204 NoContent        | 400 BadRequest; 404 NotFound; 500 InternalServerError with a message | Yes (Only Administrator) |
| Book   | GET    | api/books          | Get all books.                                            | 200 OK               | 500 InternalServerError with a message                               | Yes                      |
|        | GET    | api/books/:id      | Get a Book by id.                                         | 200 OK               | 404 NotFound; 500 InternalServerError with a message                 | Yes                      |
|        | POST   | api/books          | Creates a new book.                                       | 201 Created          | 400 BadRequest; 500 InternalServerError with a message               | Yes (Only Administrator) |
|        | PUT    | api/books/:id      | Update a book by id.                                      | 204 NoContent        | 400 BadRequest; 404 NotFound; 500 InternalServerError with a message | Yes (Only Administrator) |
|        | DELETE | api/books/:id      | Delete a book by id.                                      | 204 NoContent        | 500 InternalServerError with a message                               | Yes (Only Administrator) |
