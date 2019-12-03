using SimpleWebScraper.Builders;
using SimpleWebScraper.Data;
using SimpleWebScraper.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleWebScraper
{
    
    class Program
    {
        private const string Method = "search";
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("Please enter which city you would like to scrape information from: ");
                var craigsListCity = Console.ReadLine() ?? string.Empty;

                Console.WriteLine("Please enter the CraigsList category that you would like to scrape: ");
                var craigsListCategoryName = Console.ReadLine() ?? string.Empty;

                using (WebClient client = new WebClient())
                {
                    //.Replace will remove all the spaces in user input i.e. new york to newyork. 
                    //DownloadString method from System.Net retrieves information from URL as a string. i.e. <!DOCTYPE html>\n<html class=...
                    string content = client.DownloadString($"http://{craigsListCity.Replace(" ", string.Empty)}.craigslist.org/{Method}/{craigsListCategoryName}");

                    ScrapeCriteria scrapeCriteria = new ScrapeCriteriaBuilder()
                        //Retrieves source code from the website
                        .WithData(content)
                        //.WithRegex is looking for the FULL MATCH
                        .WithRegex(@"<a href=\""(.*?)\"" data-id=\""(.*?)\"" class=\""result-title hdrlnk\"">(.*?)</a>")
                        .WithRegexOptions(RegexOptions.ExplicitCapture)
                        //.ScrapeCriteriaBuilder.WithPart(ScrapeCriteriaPartBuilder) holds the (.*?).Which is title of listing and adds to ScrapeCriteriaParts List
                        .WithPart(new ScrapeCriteriaPartBuilder()
                            .WithRegex(@">(.*?)</a>")
                            .WithRegexOption(RegexOptions.Singleline)
                            .Build())
                        //.ScrapeCriteriaBuilder.WithPart(ScrapeCriteriaPartBuilder) holds the (.*?).Which is link to the listing and adds to ScrapeCriteriaParts List
                        .WithPart(new ScrapeCriteriaPartBuilder()  
                            .WithRegex(@"href=\""(.*?)\""")
                            .WithRegexOption(RegexOptions.Singleline)
                            .Build())
                        .Build();

                    Scraper scraper = new Scraper();
                    //Takes in the DATA (source code), Regex and scrapes 
                    var scrapedElements = scraper.Scrape(scrapeCriteria);

                    if (scrapedElements.Any())
                    {
                        foreach (var scrapedElement in scrapedElements)
                        {
                            Console.WriteLine(scrapedElement);
                        }
                    }
                    else
                    {
                        Console.WriteLine("There were no matches for the specified scrape criteria");
                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}