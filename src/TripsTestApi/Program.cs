using Microsoft.AspNetCore.Mvc;
using SemanticKernelFunctionsPoc.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

Dictionary<string, string> userNamesCustomerNamesMap = new()
{
    { "user_pepsi", "Pepsi" },
    { "user_cocacola", "Coca Cola" },
    { "user_fanta", "Fanta" }
};

Dictionary<string, string> activeSessions = [];
app.MapPost("/simple_login",([FromBody] LoginRequest loginRequest) =>
{
    // Basic validation for the incoming username
    if (string.IsNullOrEmpty(loginRequest.UserName))
    {
        return Results.BadRequest("UserName is required.");
    }

    // Check if the username exists in our map
    if (!userNamesCustomerNamesMap.TryGetValue(loginRequest.UserName, out var customerName))
    {
        // If username is not found, return Forbidden (or Unauthorized)
        return Results.Forbid(); // Or Results.Unauthorized() depending on desired behavior
    }

    // Generate a new unique session ID
    var sessionId = Guid.NewGuid().ToString();
    // Store the session ID and the associated customer name
    activeSessions[sessionId] = customerName;

    // Return the new session ID to the client
    return Results.Ok(new LoginResponse { SessionId = sessionId });
}).WithName("SimpleLogin");

List<Trip> Trips = [
    new Trip(1, "Sao Paulo", "Rio de Janeiro", 1, 1, 1000, "Pepsi"),
    new Trip(2, "Buenos Aires", "Córdoba", 2, 5, 2000, "Coca Cola"),
    new Trip(3, "Valparaiso", "Santiago", 3, 6, 3000, "Fanta")
];

List<Driver> Drivers = [
    new Driver(1, "John", "Doe", 30, 4.5, "Pepsi"),
    new Driver(2, "Jane", "Doe", 25, 4.0, "Coca Cola"),
    new Driver(3, "John", "Perez", 35, 4.8, "Fanta"),
    new Driver(4, "Lucky", "Luke", 30, 4.5, "Pepsi"),
    new Driver(5, "Homer", "Simpson", 30, 4.5, "Coca Cola"),
    new Driver(6, "Lara", "Croft", 30, 4.5, "Fanta"),
 ];

List<Vehicle> Vehicles = [
    new Vehicle(1, "ABC123", "Toyota", "Camry", 2022, "Pepsi"),
    new Vehicle(2, "DEF456", "Honda", "Civic", 2021, "Coca Cola"),
    new Vehicle(3, "GHI789", "Ford", "Mustang", 2020, "Fanta"),
    new Vehicle(4, "JKL012", "Chevrolet", "Camaro", 2019, "Pepsi"),
    new Vehicle(5, "MNO345", "Nissan", "Altima", 2018, "Coca Cola"),
    new Vehicle(6, "PQR678", "Subaru", "Impreza", 2017, "Fanta"),
];

bool ValidateSession(HttpContext httpContext, out string? customerName)
{
    customerName = null;

    if(false == httpContext.Request.Headers.TryGetValue("SessionId", out var sessionId))
        return false;

    var strSessionId = sessionId.ToString();
    if(string.IsNullOrEmpty(strSessionId))
        return false;

    return activeSessions.TryGetValue(strSessionId, out customerName);
}

// Endpoint to get trips by customer name (requires valid session)
app.MapGet("/trips", (HttpContext httpContext) =>
{
    if (!ValidateSession(httpContext, out var customerName))
    {
        return Results.Unauthorized(); // Session invalid or missing
    }

    var customerTrips = Trips.Where(t => t.CustomerName == customerName).ToList();
    return Results.Ok(customerTrips);
}).WithName("GetTrips");

// Endpoint to get vehicles by customer name (requires valid session)
app.MapGet("/vehicles", (HttpContext httpContext) =>
{
    if (!ValidateSession(httpContext, out var customerName))
    {
        return Results.Unauthorized(); // Session invalid or missing
    }

    var customerVehicles = Vehicles.Where(v => v.CustomerName == customerName).ToList();
    return Results.Ok(customerVehicles);
}).WithName("GetVehicles");

// Endpoint to get drivers by customer name (requires valid session)
app.MapGet("/drivers", (HttpContext httpContext) =>
{
    if (!ValidateSession(httpContext, out var customerName))
    {
        return Results.Unauthorized(); // Session invalid or missing
    }

    var customerDrivers = Drivers.Where(d => d.CustomerName == customerName).ToList();
    return Results.Ok(customerDrivers);
}).WithName("GetDrivers");

app.Run();