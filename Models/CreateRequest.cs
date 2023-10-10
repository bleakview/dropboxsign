namespace dropbox_sign.Models
{
	public class CreateRequest
	{
		public string Email { get; set; }
		public string Name { get; set; }
		public string Title { get; set; }
		public string Subject { get; set; }
		public string Message { get; set; }		
		public IFormFile Document { set; get; }
	}
}
