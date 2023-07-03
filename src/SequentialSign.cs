using Bulksign.Api;
using Bulksign.Api.SignSdk;
using Bulksign.Context;
using Bulksign.Pal;
using Bulksign.SigningSdk;

namespace Bulksign.Sample;

public class SequentialSign
{   

    public  void Sign(SignContext context, SignApiClient client)
    {
        //go through each document and each signature field and sign them 
        foreach (SignContextDocument document in context.Documents)
        {
            List<PdfFormField> signatures = document.FormElements.Where(f => f.PdfFormFieldType == FormFieldType.Signature).ToList();

            foreach (PdfFormField signature in signatures)
            {
                int signatureType = (signature as PdfFormSignature).SignatureType;

                //unattended signature with a USB/smart card certificate is not supported
                if (signatureType == (int)SignatureTypeApi.LocalCertificate)
                {
                    Console.WriteLine($"Signature {signature.Id} is a LocalCertificate signature which is not a supported signature type for unattended signing, skipping it ");
                    continue;
                }
                
                if (signatureType == (int)SignatureTypeApi.OTPSign)
                {
                    Console.WriteLine($"Signature {signature.Id} is a OTP signature which is not a supported signature type for unattended signing, skipping it ");
                    continue;
                }
                
                //can we sign this type of signature in unattended mode?
                if (! Constants.SignatureTypesWithoutUserInteraction.Contains(signatureType))
                {
                    Console.WriteLine($"Signature {signature.Id} is not a support signature type for unattended signing, skipping it ");
                    return;
                }
				
                Console.WriteLine($"Started unattended signing for field {signature.Id}, signature type is {signatureType} ");

                SignApiModel model = new SignApiModel()
                {
                    SignStepId            = context.PublicId,
                    DocumentId            = document.Id,
                    SignatureId           = signature.Id,
                    SignatureImageContent = string.Empty,
                    ClientDate            = string.Empty
                };

                SigningSdk.BulksignResult<string> sigResult = client.Sign(model, ApiKeys.SIGN_KEY);

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

    }
     
}