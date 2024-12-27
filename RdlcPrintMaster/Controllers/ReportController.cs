using Microsoft.AspNetCore.Mvc;
using RdlcPrintMaster.Services;
using System.Net.Mime;

namespace RdlcPrintMaster.Controllers
{
	[ApiController]
	[Route("")]
	public class ReportController : Controller
	{
		private readonly IReportService _reportService;
		public ReportController(IReportService reportService)
		{
			_reportService = reportService;
		}

		[HttpGet("{reportName}/{length?}/{reportType?}")]
		public async Task<ActionResult> Get(string reportName, int length = 100000, string reportType = "PDF")
		{
			reportType = reportType?.ToUpper() ?? "PDF";
			var reportNameWithLang = reportName;
			var reportFileByteString = await _reportService.GenerateReportAsync(reportNameWithLang, reportType, length);

			if (reportType == "PDF")
			{
				return File(reportFileByteString, MediaTypeNames.Application.Pdf);
			}
			return File(reportFileByteString, MediaTypeNames.Application.Octet, getReportName(reportNameWithLang, reportType));
		}

		private string getReportName(string reportName, string reportType)
		{
			var outputFileName = reportName + ".pdf";

			switch (reportType.ToUpper())
			{
				default:
				case "PDF":
					outputFileName = reportName + ".pdf";
					break;
				case "XLS":
					outputFileName = reportName + ".xls";
					break;
				case "WORD":
					outputFileName = reportName + ".doc";
					break;
			}

			return outputFileName;
		}
	}
}
