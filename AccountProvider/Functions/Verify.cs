using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AccountProvider.Functions;

public class Verify(ILogger<Verify> logger, UserManager<UserAccount> userManager)
{
	private readonly ILogger<Verify> _logger = logger;
    private readonly UserManager<UserAccount> _userManager = userManager;

	[Function("Verify")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
		string body = null!;

		try
		{
			body = await new StreamReader(req.Body).ReadToEndAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError($"ERROR : Run.StreamReader :: {ex.Message}");
		}

		if (body != null)
		{
			VerifactionRequest vr = null!;

			try
			{
				vr = JsonConvert.DeserializeObject<VerifactionRequest>(body)!;
			}
			catch (Exception ex)
			{
				_logger.LogError($"ERROR : Run.DeserializeObject<VerificationRequest> :: {ex.Message}");
			}

			if (vr != null && !string.IsNullOrEmpty(vr.Email) && !string.IsNullOrEmpty(vr.VerifactionCode))
			{
				// Verify code using verificationprovider

				var isVerified  = true;
				if (isVerified)
				{
					var userAccount = await _userManager.FindByEmailAsync(vr.Email);
					if (userAccount != null)
					{
						userAccount.EmailConfirmed = true;
						await _userManager.UpdateAsync(userAccount);

						if (await _userManager.IsEmailConfirmedAsync(userAccount))
						{
							return new OkResult();
						}
					}
				}
			}
		}

		return new UnauthorizedResult();
	}
}