namespace SemanticKernelFunctionsPoc.Shared;

public class LoginRequest
{
    public string? UserName { get; set; }
}

public class LoginResponse
{
    public string? SessionId { get; set; }
}

record Trip(int Id, string Origin, string Destination, int DriverId, int VehicleId, double InformedCargoValue, string CustomerName);
record Driver(int Id, string Name, string LastName, int Age, double Rating, string CustomerName);
record Vehicle(int Id, string LicensePlate, string Brand, string Model, int Year, string CustomerName);