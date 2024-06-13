using Bulksign.Api;
using Bulksign.Api.SignSdk;
using Bulksign.Context;

namespace Bulksign.Sample;

public class Program
{
	private static string envelopeId = string.Empty;

	private static string signStepId = string.Empty;

	
	public static void Main()
	{
		if (SendEnvelope())
		{
			Sign();
		}
	}

	public static bool SendEnvelope()
	{
		AuthenticationApiModel token = new ApiKeys().GetAuthentication();

		if (string.IsNullOrEmpty(token.Key))
		{
			Console.WriteLine("Please edit APiKeys.cs and put your own token/email");
			return false;
		}

		BulksignApiClient api = new BulksignApiClient();

		EnvelopeApiModel envelope = new EnvelopeApiModel();
		envelope.EnvelopeType = EnvelopeTypeApi.Serial;
		envelope.DaysUntilExpire = 10;
		//we will sign unattended so it makes no sense to send emails to the signer
		envelope.DisableSignerEmailNotifications = true;
		envelope.Messages = new MessageApiModel[]
		{
			new MessageApiModel()
			{
				Language = "en-us",
				Message = "Please sign this document",
				Subject = "Please sign this document"
			}

		};
		envelope.Name = "Test envelope";

		envelope.Recipients = new[]
		{
			//the Index property will determine the order in which the recipients will sign the document(s). 
			new RecipientApiModel
			{
				Name          = "Bulksign Test",
				Email         = "contact@bulksign.com",
				Index         = 1,
				RecipientType = RecipientTypeApi.Signer
			}
		};

		envelope.Documents = new[]
		{
			new DocumentApiModel
			{
				Index    = 1,
				FileName = "test.pdf",
				FileContentByteArray = new FileContentByteArray
				{
					ContentBytes = File.ReadAllBytes(Environment.CurrentDirectory + @"\Files\bulksign_test_Sample.pdf")
				}
			}
		};

		BulksignResult<SendEnvelopeResultApiModel> result = api.SendEnvelope(token, envelope);

		if (result.IsSuccessful == false)
		{
			Console.WriteLine($"Request failed : ErrorCode '{result.ErrorCode}' , Message {result.ErrorMessage}");
			return false;
		}

		envelopeId = result.Result.EnvelopeId;

		signStepId = result.Result.RecipientAccess.FirstOrDefault().SignStepIdentifier;

		Console.WriteLine($" {nameof(SendEnvelope)} request was successful, envelopeId is {result.Result.EnvelopeId} was created");

		return true;
	}

	public static void Sign()
	{
		SignApiClient client = new SignApiClient();

		SigningSdk.BulksignResult<SignContext> context = client.GetSignContext(signStepId, ApiKeys.SIGN_KEY);
		
		bool isBatchSigningEnabled = context.Result.EnableBatchSign;

		if (isBatchSigningEnabled)
		{
			//batch signing is enabled for this SignStep so switch to batch signing mode
			new BatchSign().Sign(context.Result, client);
		}
		else
		{
			//no batch signing, so sign each form field individually
			new SequentialSign().Sign(context.Result, client);
		}
	
		//automatic form filling is also supported if you need it
		//new FormFilling(client).FillFormFields(context.Result);
		
		//finish the entire signing process here
		SigningSdk.BulksignResult<string> finishResult = client.Finish(signStepId, ApiKeys.SIGN_KEY);
		
		if (finishResult.IsSuccessful == false)
		{
			Console.WriteLine($"Finishing the signing step failed : {finishResult.ErrorMessage}");
			return;
		}
		
		Console.WriteLine("DONE");
	}
}