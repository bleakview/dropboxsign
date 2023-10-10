using Dropbox.Sign.Api;
using Dropbox.Sign.Client;
using Dropbox.Sign.Model;
using dropbox_sign.Models;

namespace dropbox_sign.helper
{
	public class Sign
	{
		public async Task<List<string>> CreateRequest(SignRequest signRequest)
		{
			try
			{
				var config = new Configuration();
				config.Username = SettingsHelper.UserName;

				var apiInstance = new SignatureRequestApi(config);

				var signingOptions = new SubSigningOptions(
					draw: true,
					type: true,
					upload: true,
					phone: true,
					defaultType: SubSigningOptions.DefaultTypeEnum.Draw
				);
				var timeSpan = DateTime.UtcNow.AddDays(7) - new DateTime(1970, 1, 1);
				var secondsSinceEpoch = (int)timeSpan.TotalSeconds;
				var data = new SignatureRequestCreateEmbeddedRequest(
					clientId: signRequest.ClientId,
					title: signRequest.Title,
					subject: signRequest.Subject,
					message: signRequest.Message,
					signers: signRequest.SubSignatureRequestSigners,					
					//sample file
					//files:signRequest.Files,

					fileUrls: new List<string>() { "https://gentle-crepe-e0ad13.netlify.app/example_signature_request.pdf" },
					signingOptions: signingOptions,
					testMode: true,
					expiresAt: secondsSinceEpoch
				);


				var embeddedApiInstance = new EmbeddedApi(config);
				var signatureResponse = await apiInstance.SignatureRequestCreateEmbeddedAsync(data);
				var responseUrls = new List<string>();
				foreach (var signature in signatureResponse.SignatureRequest.Signatures)
				{
					var signatureId = signature.SignatureId;
					var embeddedSignUrlResponse = await embeddedApiInstance.EmbeddedSignUrlAsync(signatureId);
					responseUrls.Add(embeddedSignUrlResponse.Embedded.SignUrl);
					return responseUrls;
				}
			}
			catch (ApiException e)
			{
				Console.WriteLine("Exception when calling HelloSign API: " + e.Message);
				Console.WriteLine("Status Code: " + e.ErrorCode);
				Console.WriteLine(e.StackTrace);
				throw e;
			}
			return null;
		}
	}
}
