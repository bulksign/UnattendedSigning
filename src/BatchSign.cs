using Bulksign.Api;
using Bulksign.Api.SignSdk;
using Bulksign.Context;
using Bulksign.Pal;

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
            
            //now batch sign all unique signature types
            foreach (KeyValuePair<int,BatchSignatureInformation> pair in toBeSigned)
            {
                Console.WriteLine($"Starting batch sign for signature type : {pair.Key}");                
                
                SigningSdk.BulksignResult<string> sigResult = client.Sign(context.PublicId , pair.Value.DocumentId, pair.Value.SignatureId, string.Empty, ApiKeys.SIGN_KEY);
                
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
