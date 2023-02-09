using Bulksign.Api;

namespace Bulksign.Sample;

public class ApiKeys
{
	//to authenticate with a user key, just set UserEmail to a empty string
	//to authenticate with an organization key, please set both Key and UserEmail
	//see also the API docs on https://bulksign.com/docs/api.htm

	public const string SIGN_KEY = "";

	public const string API_KEY = "";
	public const string EMAIL = "";

	public AuthenticationApiModel GetAuthentication()
	{
		return new AuthenticationApiModel()
		{
			UserEmail = EMAIL,
			Key = API_KEY
		};
	}


	
}
