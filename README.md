# CollabStories
Web platform to write collaborative stories in real time using ASP.NET Core Web API and JavaScript, HTML, CSS for the frontend.

## Features
### Stories
- Actors
  - Owner Author
  - Collaborative Authors  
- Real-time collaborative stories
### Authentication
- JWT authentication
  - Login and register
### Testing
- Unit tests
- Integration tests

## Future Features
### Stories
- Turns
  - Every author has a limited amout of time per turn
  - The other authors continue where the story left
- Owner defines the session rules
  - Duration
  - Maximum players, turns and rounds
  - Optional:
    - Start and End are pre-defined, players need to reach the goal in a concise way  
### Homepage
- Latest stories and most popular
- Infinite scrolling navigation

## Setup
### User Secrets
Change `DefaultConnection`, `DbTestConnection` and `JwtConfig Secret` passwords.
```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost,1434; Database=CollabStoriesDB; MultipleActiveResultSets=True;User ID=sa;Password='example-password'; Encrypt=False;",
        "DbTestConnection": "Server=localhost,1434; Database=CollabStoriesDBTest; MultipleActiveResultSets=True;User ID=sa;Password='example-password'; Encrypt=False;"
    },
    "JwtConfig": {
        "Secret": "a-string-secret-at-least-256-bits-long",
        "ValidIssuer": "http://localhost:5014/",
        "ValidAudiences": "http://localhost:5014/"
    }
}
```
Building ...

## Technologies
- APS.NET Core Web API: C# Web Framework
- SQL Server: Relational Database
- HTML, CSS and JavaScript: Frontend
