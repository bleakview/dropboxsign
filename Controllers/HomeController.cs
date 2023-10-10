using Dropbox.Sign.Api;
using Dropbox.Sign.Client;
using Dropbox.Sign.Model;
using dropbox_sign.helper;
using dropbox_sign.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using System.Diagnostics;

namespace dropbox_sign.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public HomeController(ILogger<HomeController> logger, 
			IWebHostEnvironment webHostEnvironment)
		{
			_logger = logger;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index(string pass= "a234134")
		{
			if(pass == "a234134")
			{
				return View();
			}
			return StatusCode(500);
		}

		public IActionResult Create()
		{
			return View(new CreateRequest());
		}
		[HttpPost]
		public IActionResult Create(CreateRequest model)
		{
			try
			{
				var config = new Configuration();
				config.Username = SettingsHelper.UserName;

				var apiInstance = new SignatureRequestApi(config);

				var signer = new SubSignatureRequestSigner(
					emailAddress: "jack@example.com",
					name: "Jack",
					order: 0
				);

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
					clientId: SettingsHelper.ClientId,
					title: "NDA with Acme Co.",
					subject: "The NDA we talked about",
					message: "Please sign this NDA and then we can discuss more. Let me know if you have any questions.",
					signers: new List<SubSignatureRequestSigner>() { signer },
					ccEmailAddresses: new List<string>() { "joe@example.com" },
					fileUrls: new List<string>() { "https://gentle-crepe-e0ad13.netlify.app/example_signature_request.pdf" },
					signingOptions: signingOptions,
					testMode: true,
					expiresAt: secondsSinceEpoch
				);


				var embeddedApiInstance = new EmbeddedApi(config);
				var signatureResponse = apiInstance.SignatureRequestCreateEmbedded(data);
				var responseUrls = new List<string>();
				foreach (var signature in signatureResponse.SignatureRequest.Signatures)
				{
					var signatureId = signature.SignatureId;
					var embeddedSignUrlResponse = embeddedApiInstance.EmbeddedSignUrl(signatureId);
					responseUrls.Add(embeddedSignUrlResponse.Embedded.SignUrl);					
				}
				var pdfModel = new PdfModel
				{
					SignLink = responseUrls.First(),
				};
				return RedirectToAction("Document", "Home", pdfModel);
			}
			catch (ApiException e)
			{
				Console.WriteLine("Exception when calling HelloSign API: " + e.Message);
				Console.WriteLine("Status Code: " + e.ErrorCode);
				Console.WriteLine(e.StackTrace);
			}
			return RedirectToAction("Index", "Home");
		}
		public IActionResult Privacy()
		{
			return View();
		}

		[DisableCors]
		public IActionResult Document(PdfModel pdfModel)
		{
			if(pdfModel.SignLink == null)
			{
				pdfModel = new PdfModel { 
					SignLink= "https://app.hellosign.com/editor/embeddedSign?signature_id=5807d38dfc27dc50b4ec14c7f9f9eb1a&token=480794129ddf7d4e31f78d028d633eb5"
				};
			}
			
			return View(pdfModel);
		}

		public IActionResult ViewPdf(string file)
		{
			var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
			var filePath = Path.Combine(uploads, file);
			var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return new FileStreamResult(stream, "application/pdf");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		private string GetUniqueFileName(string fileName)
		{
			fileName = Path.GetFileName(fileName);
			return Path.GetFileNameWithoutExtension(fileName)
					  + "_"
					  + Guid.NewGuid().ToString().Substring(0, 4)
					  + Path.GetExtension(fileName);
		}
	}
}