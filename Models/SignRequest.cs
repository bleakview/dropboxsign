using Dropbox.Sign.Model;

namespace dropbox_sign.Models
{
	public class SignRequest
	{		
		public string ClientId { get; set; }
		public string UserName { get; set; }
		public string Title { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }
		public List<SubSignatureRequestSigner> SubSignatureRequestSigners = new List<SubSignatureRequestSigner>();
		public List<Stream> Files = new List<Stream>();
	}
}
