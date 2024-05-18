namespace AccountProvider.Models;

public class VerifactionRequest
{
	public string Email { get; set; } = null!;
	public string VerifactionCode { get; set; } = null!;
}