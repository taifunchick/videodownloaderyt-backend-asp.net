# 🎬 Video Downloader API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens)

**REST API for downloading videos from YouTube**

</div>

---

## 📖 About

A REST API service for downloading YouTube videos. Features user authentication, download queue, and video information retrieval.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🔐 JWT Authentication | Register, login, token-based auth |
| 📺 YouTube Support | Download videos in various qualities |
| 📋 Download List | View your download history |
| 🗑️ Delete Downloads | Remove downloaded videos |
| 🎯 Quality Selection | Choose video quality (144p - 4K) |
| 👤 User Profiles | Each user has private downloads |

---

## 🛠️ Tech Stack

- **.NET 8.0** - Framework  
- **ASP.NET Core** - Web API  
- **Entity Framework Core** - ORM  
- **SQLite** - Database  
- **JWT** - Authentication  
- **YoutubeExplode** - YouTube downloading  
- **BCrypt** - Password hashing  

---

## 🚀 Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Step 1: Clone or create project

```bash
mkdir VideoDownloader.API
cd VideoDownloader.API
```

### Step 2: Create project

```bash
dotnet new webapi -n VideoDownloader.API
cd VideoDownloader.API
```

### Step 3: Install packages

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package YoutubeExplode --version 6.3.16
dotnet add package BCrypt.Net-Next
dotnet add package Swashbuckle.AspNetCore
```

### Step 4: Create database

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 5: Run the API

```bash
dotnet run
```

---

## 📡 API Endpoints

Base URL: `http://localhost:5094/api/Video`

### Authentication

| Method | Endpoint   | Description       |
|--------|------------|-------------------|
| POST   | `/register`| Register new user |
| POST   | `/login`   | Login and get JWT token |

### Video Operations (require Bearer token)

| Method | Endpoint         | Description          |
|--------|------------------|----------------------|
| POST   | `/info`          | Get video metadata   |
| POST   | `/download`      | Download video       |
| GET    | `/downloads`     | Get user's downloads |
| DELETE | `/download/{id}` | Delete download      |

---

## 🔧 Database Schema

### Users Table

| Column      | Type    | Description                 |
|------------|---------|-----------------------------|
| Id         | INTEGER | Primary Key                 |
| Email      | TEXT    | Unique, email address       |
| Username   | TEXT    | User's name                 |
| PasswordHash | TEXT  | BCrypt hash                 |
| CreatedAt  | DATETIME| Registration date           |

### VideoDownloads Table

| Column      | Type    | Description              |
|------------|---------|--------------------------|
| Id         | INTEGER | Primary Key              |
| VideoUrl   | TEXT    | Original URL             |
| Title      | TEXT    | Video title              |
| Quality    | TEXT    | Selected quality         |
| Status     | TEXT    | Completed/Pending/Failed |
| FileSize   | BIGINT  | Size in bytes            |
| UserId     | INTEGER | Foreign key to Users     |
| CreatedAt  | DATETIME| Download date            |
| CompletedAt| DATETIME| Completion date          |

---

## 🐛 Troubleshooting

| Problem               | Solution                            |
|-----------------------|-------------------------------------|
| Database not found    | Run `dotnet ef database update`     |
| JWT token invalid     | Check `Jwt:Key` in appsettings.json |
| Port 5094 in use      | Change port in `launchSettings.json`|
| YouTube download fails| Check video URL and internet connection |

---

## 🙏 Acknowledgments

[YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) - YouTube downloading library  
[BCrypt.Net-Next](https://github.com/benrugg/BCrypt.Net-Next) - Password hashing  
