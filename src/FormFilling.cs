using Bulksign.Api.SignSdk;
using Bulksign.Context;
using Bulksign.Pal;
using Bulksign.SigningSdk;

namespace Bulksign.Sample;

public class FormFilling
{
    private SignApiClient client = null;

   
    public FormFilling(SignApiClient client)
    {
        this.client = client;
    }

    public void FillFormFields(SignContext context)
    {
        foreach (SignContextDocument document in context.Documents.OrderBy(o=>o.OrderIndex))
        {
            List<PdfFormField> fields = document.FormElements.ToList();

            foreach (PdfFormField field in fields)
            {
                if (field.PdfFormFieldType == FormFieldType.Attachment)
                {
                    AddAttachment(context.PublicId, document, field as PdfFormAttachment);
                }

                if (field.PdfFormFieldType == FormFieldType.CheckBox)
                {
                    PdfFormFieldCheckBox checkBox = field as PdfFormFieldCheckBox;
                    
                    //check it only if it's required
                    if (checkBox.IsRequired)
                    {
                        this.UpdateFormFieldValue(context.PublicId, document, checkBox.Id, "true");
                    }
                }

                if (field.PdfFormFieldType == FormFieldType.TextBox)
                {
                    PdfFormFieldTextField textBox = field as PdfFormFieldTextField;

                    if (textBox.IsRequired)
                    {
                        this.UpdateFormFieldValue(context.PublicId, document, textBox.Id, "test");
                    }
                } 
                
                
                if (field.PdfFormFieldType == FormFieldType.ComboBox)
                {
                    PdfFormFieldComboBox comboBox = field as PdfFormFieldComboBox;

                    if (comboBox.IsRequired)
                    {
                        this.UpdateFormFieldValue(context.PublicId, document, comboBox.Id, "_selected_item_text");
                    }
                } 

                
            }
        }

    }

    private void UpdateFormFieldValue(string stepIdentifier, SignContextDocument document, string formFieldId, string value)
    {
        client.UpdateFormFieldValue(new UpdateFormFieldValueApiModel()
        {
            DocumentId = document.Id,
            SignStepId = stepIdentifier,
            Value = value,
            PdfId = formFieldId
            
        },  ApiKeys.SIGN_KEY);
    } 
    
    
    private void AddAttachment(string stepIdentifier, SignContextDocument document, PdfFormAttachment attachment)
    {
        client.AddAttachment(new AddAttachmentApiModel()
        {
            DocumentId = document.Id,
            SignStepId = stepIdentifier,
            FileName = "myfile.jpg",
            FileContentAsBase64 = "..........", //add here the file's content as a base64 encoded string
            AttachmentId = attachment.Id
        }, ApiKeys.SIGN_KEY);
    }

}