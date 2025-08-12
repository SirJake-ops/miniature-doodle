# BackendTracker - Internal Ticket Management System

A comprehensive ticket management system designed for internal team use, featuring real-time communication capabilities between ticket submitters and assignees.

## 🎯 Overview

BackendTracker is a CRUD-based ticket management system that allows users to create, manage, and discuss tickets through multiple channels. The system supports both traditional form-based ticket creation and automated ticket generation from email submissions. What sets it apart is the integrated real-time messaging system that enables direct communication between ticket submitters and assignees. Previous iteration of the project is here [https://github.com/SirJake-ops/bookish-chainsaw](found here).

## ✨ Key Features

### 📋 Ticket Management
- **Multi-Channel Ticket Creation**: Create tickets via frontend forms or automated email processing
- **Complete CRUD Operations**: Create, read, update, and delete tickets
- **Ticket Assignment**: Assign tickets to team members for resolution
- **Status Tracking**: Monitor ticket progress and resolution status
- **File Attachments**: Support for file uploads and attachments

### 💬 Real-Time Communication
- **GraphQL-Powered Messaging**: Real-time chat system built on GraphQL subscriptions
- **Ticket-Specific Discussions**: Contextual conversations tied to individual tickets
- **Submitter-Assignee Communication**: Direct communication channel between relevant parties
- **Live Updates**: Instant message delivery and status updates

### 🔐 Authentication & Security
- **JWT-Based Authentication**: Secure token-based user authentication
- **Role-Based Access**: Controlled access based on user roles and permissions
- **Internal Tool Focus**: Designed for internal team use with appropriate security measures

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
├── BackendTracker.Domain/          # Core business entities and interfaces
├── BackendTracker.Application/     # Business logic and use cases
├── BackendTracker.Infrastructure/  # Data access and external services
└── BackendTracker.WebAPI/         # Controllers, GraphQL, and presentation layer
```

### Technology Stack

- **Backend**: ASP.NET Core 8.0
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Real-Time**: GraphQL with Hot Chocolate
- **Authentication**: JWT Bearer Tokens
- **Containerization**: Docker & Docker Compose
- **Architecture**: Clean Architecture / Onion Architecture

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/SirJake-ops/miniature-doodle.git
   cd miniature-doodle
   ```

2. **Start the database**
   ```bash
   docker-compose up -d postgres
   ```

3. **Update database connection string**
   
   Update `appsettings.json` with your PostgreSQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=backendtracker;Username=your_user;Password=your_password"
     }
   }
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Start the application**
   ```bash
   dotnet run --project BackendTracker.WebAPI
   ```

6. **Access the application**
   - API: `https://localhost:5001`
   - GraphQL Playground: `https://localhost:5001/graphql`
   - Swagger UI: `https://localhost:5001/swagger`

## 📡 API Endpoints

### REST API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/tickets` | Get tickets for a user |
| `POST` | `/api/tickets` | Create a new ticket |
| `PUT` | `/api/tickets/{id}` | Update a ticket |
| `PATCH` | `/api/tickets/{id}` | Partially update ticket properties |
| `DELETE` | `/api/tickets/{id}` | Delete a ticket |
| `POST` | `/api/tickets/{id}/assign` | Assign ticket to user |
| `POST` | `/api/auth/login` | User authentication |

### GraphQL

Access the GraphQL playground at `/graphql` to explore:

- **Queries**: Fetch tickets, users, and messages
- **Mutations**: Create tickets, send messages, update statuses
- **Subscriptions**: Real-time message updates and ticket changes

Example GraphQL subscription for real-time messaging:
```graphql
subscription {
  messageAdded(ticketId: "your-ticket-id") {
    id
    content
    userId
    timestamp
    user {
      userName
    }
  }
}
```

## 🐳 Docker Support

The project includes Docker support for easy deployment:

```bash
# Start PostgreSQL database
docker-compose up -d postgres

# Or start the entire application stack
docker-compose up -d
```

### Docker Configuration

- **PostgreSQL**: Runs on port `5432`
- **Application**: Configured to connect to containerized database
- **Volumes**: Database data persisted in Docker volumes

## 📊 Database Schema

### Core Entities

- **Users** (`ApplicationUsers`): System users with authentication
- **Tickets**: Main ticket entities with status, priority, and metadata
- **Messages**: Real-time chat messages linked to tickets
- **Files**: File attachments associated with tickets

### Key Relationships

- Users can submit multiple tickets (one-to-many)
- Users can be assigned multiple tickets (one-to-many)
- Tickets can have multiple messages (one-to-many)
- Tickets can have multiple file attachments (one-to-many)

## 🔧 Configuration

### Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Database=backendtracker;Username=postgres;Password=password"

# JWT Authentication
JWT__SecretKey="your-secret-key"
JWT__Issuer="BackendTracker"
JWT__Audience="BackendTrackerUsers"

# Email Processing (for automated ticket creation)
Email__SmtpServer="your-smtp-server"
Email__Port=587
Email__Username="your-email"
Email__Password="your-password"
```

## 🧪 Testing

The project includes comprehensive integration tests:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test BackendTrackerTest.IntegrationTests
```

Test coverage includes:
- ✅ Ticket CRUD operations
- ✅ User authentication flows
- ✅ GraphQL queries and mutations
- ✅ Real-time messaging functionality

## 🚧 Roadmap

### Planned Features

- [ ] **Email Integration**: Automated ticket creation from email submissions
- [ ] **File Upload Improvements**: Enhanced file handling and storage
- [ ] **Notification System**: Email and in-app notifications
- [ ] **Advanced Filtering**: Complex ticket search and filtering
- [ ] **Reporting Dashboard**: Analytics and reporting capabilities
- [ ] **Mobile API**: Mobile-optimized endpoints

### Future Enhancements

- [ ] **External Client Portal**: Customer-facing ticket submission
- [ ] **SLA Management**: Service level agreement tracking
- [ ] **Integration APIs**: Third-party service integrations
- [ ] **Advanced Permissions**: Granular role-based access control

## 🤝 Contributing

This is currently an internal tool, but contributions are welcome:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For internal support and questions:

- **Issues**: Create an issue in this repository
- **Documentation**: Check the `/docs` folder for detailed documentation
- **GraphQL Schema**: Available at `/graphql` endpoint

---

**Built with ❤️ for internal team productivity**


