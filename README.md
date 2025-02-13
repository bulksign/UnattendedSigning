# Unattended Signing Sample

Sample code which demonstrates unattended signing (signing multiple signature fields in multiple documents without the user's input) with Bulksign platform.

Brief documentation about this feature is available <a href="https://bulksign.com/docs/unattended.htm">here</a>


#High level overview of the logic:

- we send a new envelope for signing using SendEnvelope API and we obtain the SignStep identifier .

- using the SignSDK, we use GetSignContext to obtain information about this specific SignStep.

- we check if EnableBatchSign is enabled for this SignStep.

a) if its enabled we sign with batch mode :

```
new BatchSign().Sign
```

b) if BatchMode is not enabled, we sign using sequential mode (which loops through all available signature fields and signs them one by one).


```
new SequentialSign().Sign
```

- after ALL signature fields are signed, we call "Finish"  to complete the SignStep.



#Notes :

1) If you plan to implement unattended signing, consider enabling BatchSigning when creating the envelope :
```
    envelope.EnableBatchSign = true;
```

It has the following advantages :

- it's simpler : all signatures fields of a certain type will be signed using a single request.

- for OTP signatures, you will be sending a single OTP for authentication for ALL OTP signatures.   

- it's faster.

2) Unattended form filling is also supported if you need it :

```
new FormFilling().FillFormFields()
```