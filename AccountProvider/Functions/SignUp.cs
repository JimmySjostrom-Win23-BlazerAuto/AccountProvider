using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AccountProvider.Functions
{
    public class SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)
	{
        private readonly ILogger<SignUp> _logger = logger;
        private readonly UserManager<UserAccount> _userManager = userManager;

		[Function("SignUp")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            string body = null!;

			try
			{
				body = await new StreamReader(req.Body).ReadToEndAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError($"ERROR : SignUp.Run.StreamReader :: {ex.Message}");
			}

			if (body != null)
            {
                UserRegistrationRequest urr = null!;

				try
				{
					urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body)!;
				}
				catch (Exception ex)
				{
					_logger.LogError($"ERROR : SignUp.Run.DeserializeObject<UserRegistrationRequest> :: {ex.Message}");
				}
				
                if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password))
                {
                    if (! await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                    {
                        var userAccount = new UserAccount
                        {
                            FirstName = urr.FirstName,
                            LastName = urr.LastName,
                            Email = urr.Email,
                            UserName = urr.Email
                        };

						try
						{
							var result = await _userManager.CreateAsync(userAccount, urr.Password);
							if (result.Succeeded)
							{
								try
								{
									using var http = new HttpClient();
									StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email = userAccount.Email }), Encoding.UTF8, "application/json");
									var response = await http.PostAsync("https://verificationprovider-silicon-js.azurewebsites.net/api/generate", content);
								}									 								
								catch (Exception ex)
								{
									_logger.LogError($"ERROR : Run.Verification :: {ex.Message}");
								}
							
								return new OkResult();
							}
						}
						catch (Exception ex)
						{
							_logger.LogError($"ERROR : SignUp.Run._userManager.CreateAsync :: {ex.Message}");
						}						
                    }
					else
					{
                        return new ConflictResult();
					}
				}
            }

            return new BadRequestResult();
        }
    }
}