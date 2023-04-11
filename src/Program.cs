using Bulksign.Api;
using Bulksign.Api.SignSdk;
using Bulksign.Context;
using Bulksign.Pal;

namespace Bulksign.Sample;

public class Program
{
	private static string envelopeId = string.Empty;

	private static string signStepId = string.Empty;

	private static int[] allowedSignatureTypes = new int[]
		{ (int)SignatureTypeApi.DrawTypeToSign, (int)SignatureTypeApi.ClickToSign, (int)SignatureTypeApi.Stamp };
	
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
		envelope.EmailMessage = "Please sign this document";
		envelope.EmailSubject = "Please Bulksign this document";
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

		envelopeId = result.Response.EnvelopeId;

		signStepId = result.Response.RecipientAccess.FirstOrDefault().SignStepIdentifier;

		Console.WriteLine($" {nameof(SendEnvelope)} request was successful, envelopeId is {result.Response.EnvelopeId} was created");

		return true;
	}

	public static void Sign()
	{
		SignApiClient client = new SignApiClient();

		SigningSdk.BulksignResult<SignContext> context = client.GetSignContext(signStepId, ApiKeys.SIGN_KEY);

		//go through each document and eac signature field and sign them 

		foreach (SignContextDocument document in context.Response.Documents)
		{
			List<PdfFormField> signatures = document.FormElements.Where(f => f.PdfFormFieldType == FormFieldType.Signature).ToList();

			foreach (PdfFormField signature in signatures)
			{
				int signatureType = (signature as PdfFormSignature).SignatureType;

				//can we sign this type of signature ?
				if (!allowedSignatureTypes.Contains(signatureType))
				{
					Console.WriteLine($"Signature {signature.Id} is not a support signature type for unttended signing, skipping it ");
					return;
				}
				
				Console.WriteLine($"Started unattended signing for field {signature.Id}, signature type is {signatureType} ");
				
				SigningSdk.BulksignResult<string> sigResult = client.Sign(signStepId, document.Id, signature.Id, string.Empty, ApiKeys.SIGN_KEY);

				if (sigResult.IsSuccessful == false)
				{
					Console.WriteLine($"Signing failed for field {signature.Id}");
				}
				else
				{
					Console.WriteLine($"Signing successful for field {signature.Id}");
				}
			}
		}

		client.Finish(signStepId, ApiKeys.SIGN_KEY);

		Console.WriteLine("DONE");
	}
}