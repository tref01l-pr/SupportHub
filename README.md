# Support Hub
# ASP.NET Framework + PostgreSQL Backend

## 🚀 Getting Started
### 1. Prerequisites
Ensure you have the following installed:
Docker Descktop
PostgreSQL 16-version
.net 8.0 sdk

### 2. Clone the Repository
```
git clone https://github.com/tref01l-pr/SupportHub.git
```

### 3. Create .env file
It should look like this:
```
DATABASE_URL=
JWT_SECRET=

SMTP_USER=
SMTP_PASSWORD=
SMTP_HOST=
SMTP_PORT=

IMAP_USER=
IMAP_PASSWORD=
IMAP_HOST=
IMAP_PORT=
```

### 4. Configure PostgreSQL
Go to your .env file and there enter your DATABASE_URL= it should look like this:
```
Host=your-host;Database=your-database-name;Username=your-username;Password=your-password;Port=your-port;
```

### 4. Configure your JWT_SECRET
Go to your .env file and there enter your JWT_SECRET= your secret must not contain spaces or other specific characters. Also, your secret must not be too long or too short. For example string with 72 chars

### 5. Configure your SMTP and IMAP
Go to your .env file and there enter your options for SMTP and IMAP
SMTP_USER= it is your email for your SMTP
SMTP_PASSWORD= 16 chars password for SMTP
SMTP_HOST= host of your SMTP. For example smtp.gmail.com for gmail
SMTP_PORT= port of your SMTP. For example 587 for gmail

IMAP_USER= it is your email for your IMAP
IMAP_PASSWORD= 16 chars password for IMAP
IMAP_HOST= host of your IMAP. For example imap.gmail.com for gmail
IMAP_PORT= port of your IMAP. For example 993 for gmail

### 6. Update migrations and do data seeding
```
cd ./SupportHubBackEnd
dotnet ef database update --project .\SupportHub.DataAccess.SqlServer\ --startup-project .\SupportHub.API\
cd ./SupportHub.API
dotnet run --seeddata
```
After running all the commands, you will add all the migrations to your database and do data seeding.
You can run your application and everything will work.

### 7. Using docker to run application
If you want to run your application local on docker you should change some routes in your Dockerfile
```
COPY ["SupportHubBackEnd/SupportHub.API/SupportHub.API.csproj", "SupportHub.Api/"]
COPY ["SupportHubBackEnd/SupportHub.Application/SupportHub.Application.csproj", "SupportHub.Application/"]
COPY ["SupportHubBackEnd/SupportHub.DataAccess.SqlServer/SupportHub.DataAccess.SqlServer.csproj", "SupportHub.DataAccess.SqlServer/"]
COPY ["SupportHubBackEnd/SupportHub.Domain/SupportHub.Domain.csproj", "SupportHub.Domain/"]
COPY ["SupportHubBackEnd/SupportHub.Infrastructure/SupportHub.Infrastructure.csproj", "SupportHub.Infrastructure/"]
```
Here you have to remove SupportHubBackEnd/ to run it locally and it should look like this:
```
COPY ["SupportHub.API/SupportHub.API.csproj", "SupportHub.Api/"]
COPY ["SupportHub.Application/SupportHub.Application.csproj", "SupportHub.Application/"]
COPY ["SupportHub.DataAccess.SqlServer/SupportHub.DataAccess.SqlServer.csproj", "SupportHub.DataAccess.SqlServer/"]
COPY ["SupportHub.Domain/SupportHub.Domain.csproj", "SupportHub.Domain/"]
COPY ["SupportHub.Infrastructure/SupportHub.Infrastructure.csproj", "SupportHub.Infrastructure/"]
```

If you want to deploy this application you should check where your cloud service will take a root for your path. 
If it takes your root file as repository from GitHub, you won't change anything.
If it takes your root file as a Dockerfile, then you will have to change the routes as in the second example

to run it locally use this commands:
```
docker build -t support-hub .
docker container ls
docker run -p 8080:8080 support-hub
```

## 🔬 Testing
You can check some tests in SupportHub.UnitTests