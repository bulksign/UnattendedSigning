using Bulksign.Api;

namespace Bulksign.Sample;

public static class Constants
{
    public static int[] SignatureTypesWithoutUserInteraction = new int[] { (int)SignatureTypeApi.DrawTypeToSign, (int)SignatureTypeApi.ClickToSign, (int)SignatureTypeApi.Stamp };

}