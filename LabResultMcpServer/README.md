# Lab Result MCP Server

This is a Model Context Protocol (MCP) server implemented in .NET Core that fetches laboratory results for a patient from an Oracle database.

## Features

- MCP Tool: `fetch_patient_lab_results` accepts `patientId` and optional `dateRange`, returns JSON with patient info and lab results.
- Database Integration: Uses Oracle.ManagedDataAccess.Core to query lab results.
- Structured Response: Results grouped by categories (e.g., HEMATOLOGIE, BIOCHIMIE).
- Logging: Serilog for console and file logging.
- Dependency Injection: Services registered for testability.

## Prerequisites

- .NET 8 or later
- Oracle Database with appropriate tables
- Oracle.ManagedDataAccess.Core NuGet package

## Setup

1. Clone the repository.
2. Update `appsettings.json` with your Oracle connection string.
3. Run `dotnet restore` to install dependencies.
4. Build: `dotnet build`

## Database Schema Assumptions

- `patients`: patient_id, name, nda, sample_date
- `lab_results`: patient_id, category, subcategory, test_name, value, unit, normal_min, normal_max, status, date_performed, validated_by

Adjust queries in `LabResultService.cs` as needed.

## Running the MCP Server

The server uses HTTP transport for MCP communication. It runs as an ASP.NET Core app.

- Run: `dotnet run` (starts on http://localhost:3001)
- MCP clients connect via HTTP to the server.

The server supports Streamable HTTP for efficient communication.

Example: MCP clients can send POST requests to the MCP endpoints for tool calls.

## Security

- Implement authentication if needed (e.g., API key in MCP client).
- Use secure DB credentials.
- Validate inputs to prevent SQL injection (use parameterized queries).

## Testing

Run unit tests: `dotnet test` in the LabResultMcpServer.Tests directory.

Tests cover service initialization and tool invocation. For full DB integration, set up a test Oracle instance.

## License

Apache 2.0