using Bulksign.Api;

namespace Bulksign.Sample;

public class ApiKeys
{
	//this is the authentication key used for SignSDK.
	//This key is unique for each Bulksign instance and can be obtained for	instance administrator.
	public const string SIGN_KEY = "";

	
	//to authenticate with a user key, just set UserEmail to a empty string
	//to authenticate with an organization key, please set both Key and UserEmail
	//see also the API docs on https://bulksign.com/docs/api.htm
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
