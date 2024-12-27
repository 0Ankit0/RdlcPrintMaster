using AspNetCore.Reporting;
using RdlcPrintMaster.Models;
using System.Reflection;
using System.Text;

namespace RdlcPrintMaster.Services
{
	public interface IReportService
	{
		Task<byte[]> GenerateReportAsync(string reportName, string reportType, int length);
	}

	public class ReportService : IReportService
	{
		private static readonly string FileDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

		public async Task<byte[]> GenerateReportAsync(string reportName, string reportType, int length)
		{
			// Attempt 1: Try using FileDirPath
			var rdlcFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Design", $"{reportName}.rdlc");

			// Attempt 2: Fallback to AppDomain.CurrentDomain.BaseDirectory if the file is not found
			if (!File.Exists(rdlcFilePath))
			{
				string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				string projectDirectory = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));
				rdlcFilePath = Path.Combine(projectDirectory, "Design", $"{reportName}.rdlc");
			}

			if (!File.Exists(rdlcFilePath))
			{
				throw new FileNotFoundException($"The RDLC file '{reportName}.rdlc' could not be found in either path.");
			}

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			Encoding.GetEncoding("utf-8");

			LocalReport report = new LocalReport(rdlcFilePath);
			string dataSourceName = string.Empty;
			object dataSource = null;

			switch (reportName)
			{
				case "TestReport":
					var testData = new List<TestData>
					{
						new TestData { Name = "John Doe", Address = "123 Main St", Phone = "555-1234" },
						new TestData { Name = "Jane Smith", Address = "456 Elm St", Phone = "555-5678" }
					}; 
					dataSourceName = "TestDataset";
					dataSource = testData;
					break;
				default:
					throw new ArgumentException($"Unsupported report name: {reportName}");
			}

			report.AddDataSource(dataSourceName, dataSource);

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			var result = report.Execute(GetRenderType(reportType), 1, parameters);

			return result.MainStream;
		}

		private RenderType GetRenderType(string reportType)
		{
			return reportType.ToUpper() switch
			{
				"PDF" => RenderType.Pdf,
				"XLS" => RenderType.Excel,
				"WORD" => RenderType.Word,
				_ => RenderType.Pdf,
			};
		}
	}
}