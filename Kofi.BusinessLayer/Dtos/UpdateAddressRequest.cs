namespace Kofi.BusinessLayer.Dtos;

public class UpdateAddressRequest
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
}
