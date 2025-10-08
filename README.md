# 📔 CollabStories
Web platform to write collaborative stories in real time using ASP.NET Core Web API and JavaScript, HTML, CSS for the frontend.

---

## ✨ Features
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

---

## 📸 Screenshots
Building ...
### Backend
### Frontend

---

## 🚀 Future Features
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

---

## 🛠 Setup
### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started/)
- [Node.js](https://nodejs.org/en/download)

### Steps
1. Clone repository
2. 
#### User Secrets
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

---

## ⚙ Technologies
### Backend
- **Language**: C#  
- **Framework**: ASP.NET Core Web API 8  
- **Database**: SQL Server 2022  
- **ORM**: Entity Framework Core 8  
- **Authentication**: JWT with ASP.NET Identity  
- **Real-time Communication**: SignalR  
- **Containerization**: Docker  
- **Testing**: xUnit  
- **Fake Data**: Bogus  
- **API Documentation**: Swagger  

### Frontend
- **Languages**: JavaScript, HTML, CSS  
- **Frameworks**: React, Vue  
- **Real-time Communication**: SignalR Client  

---

## 🙋‍♂️ Authors
- **Rafael Silva** - API & Database ([LinkedIn](https://www.linkedin.com/in/rafa-silva-v/))
- **Pedro Sousa** - Frontend ([LinkedIn](https://www.linkedin.com/in/dsousr/))
