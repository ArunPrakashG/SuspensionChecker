using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SuspensionChecker
{
	class Program
	{
		static HttpClient Client = new HttpClient();

		static async Task Main(string[] args)
		{
			List<string> goodAccounts = new();
			foreach (var accountLine in await File.ReadAllLinesAsync("accounts.txt"))
			{
				if (string.IsNullOrEmpty(accountLine))
				{
					continue;
				}

				var split = accountLine.Split(new char[] { '\t', ' ' });
				string userName = split[0];
				string password = split[1];
				string profileUrl = split[2];

				var response = await Client.GetAsync(profileUrl);
				if (!response.IsSuccessStatusCode)
				{
					continue;
				}

				Console.WriteLine($"Checking {userName} ...");
				var html = await response.Content.ReadAsStringAsync();
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(html);
				IEnumerable<HtmlNode> nodes = doc.DocumentNode.Descendants(0).Where(n => n.HasClass("sectionText"));
				if (!nodes.Any())
				{
					goodAccounts.Add($"{userName} {password} {profileUrl}");
					Console.WriteLine($"No Suspension on {userName}");
				}

				Console.WriteLine($"Suspension on {userName}");
				await Task.Delay(3000);
			}

			await File.WriteAllLinesAsync("done.txt", goodAccounts);

			Console.WriteLine($"{goodAccounts.Count} Good accounts.");
			Console.ReadKey();
		}
	}
}
