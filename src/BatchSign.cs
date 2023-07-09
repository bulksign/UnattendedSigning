using Bulksign.Api;
using Bulksign.Api.SignSdk;
using Bulksign.Context;
using Bulksign.Pal;
using Bulksign.SigningSdk;

namespace Bulksign.Sample;

public class BatchSign
{
    private Dictionary<int, BatchSignatureInformation> toBeSigned = new Dictionary<int, BatchSignatureInformation>();

    public void Sign(SignContext context, SignApiClient client)
    {
        //we know batch sign is enabled, so first build a dictionary with each signature type/indentifier  
       
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
                if (!Constants.SignatureTypesWithoutUserInteraction.Contains(signatureType))
                {
                    Console.WriteLine($"Signature {signature.Id} is not a support signature type for unattended signing, skipping it ");
                    return;
                }

                if (!toBeSigned.ContainsKey(signatureType))
                {
                    toBeSigned.Add(signatureType, new BatchSignatureInformation()
                    {
                        SignatureId = signature.Id,
                        DocumentId = document.Id
                    });
                }
            }

            SignatureAuthentication auth = null;
            
            //now batch sign all unique signature types
            foreach (KeyValuePair<int,BatchSignatureInformation> pair in toBeSigned)
            {
                Console.WriteLine($"Starting batch sign for signature type : {pair.Key}");

                if (pair.Key == (int)SignatureTypeApi.OTPSign)
                {
                    SigningSdk.BulksignResult<string> otpResult = client.SendOTPForSignature(new SendSignatureOTPApiModel()
                    {
                        DocumentId = pair.Value.DocumentId,
                        SignatureId = pair.Value.SignatureId,
                        SignStepId = context.PublicId
    
                    }, ApiKeys.SIGN_KEY);

                    if (!otpResult.IsSuccessful)
                    {
                        Console.WriteLine("OTP could not be sent : " + otpResult.ErrorMessage);
                    }
                        
                    //now ask the user for the OTP 
                    string userOtp = "......";

                    auth = new OTPSignatureAuthentication()
                    {
                        Otp = userOtp
                    };
                }
                
                SignApiModel model = new SignApiModel()
                {
                    SignStepId            = context.PublicId,
                    DocumentId            = pair.Value.DocumentId,
                    SignatureId           = pair.Value.SignatureId,
                    SignatureImageContent = string.Empty,
                    ClientDate            = string.Empty,
                    Configuration = auth
                };

                SigningSdk.BulksignResult<string> sigResult = client.Sign(model, ApiKeys.SIGN_KEY);
                
                if (sigResult.IsSuccessful == false)
                {
                    Console.WriteLine($"Batch signing failed for signatureType {pair.Key}, fieldId {pair.Value.SignatureId}");
                }
                else
                {
                    Console.WriteLine($"Batch signing was successful for signatureType {pair.Key}, fieldId {pair.Value.SignatureId}");
                }
            }
            
        }
    }
}


public class BatchSignatureInformation
{
    public string SignatureId
    {
        get; 
        set;
    }

    public int DocumentId
    {
        get;
        set;
    }
}
