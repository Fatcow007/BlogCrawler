using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BlogCrawler
{
    internal class TistoryCrawler : TypeOneCrawler
    {


        private TistoryCrawler()
        {
        }

        private static readonly Lazy<TistoryCrawler> lazy = new Lazy<TistoryCrawler>(() => new TistoryCrawler());
        public static TistoryCrawler Instance
        {
            get
            {
                return lazy.Value;
            }
        }


        //EDIT BELOW ONLY -> Turn it into class->subclass(TODO)

        protected override string GetTitle()
        {
            var titleNode = MyHtmlDocument.DocumentNode.SelectSingleNode("//*[@property='og:title']");
            var title = titleNode.Attributes["content"].Value.Trim();
            return title;
        }

        protected override List<string> GetTexts()
        {
            var postAreaNode = MyHtmlDocument.DocumentNode.SelectSingleNode("//div[@class='area_view']");
            var textNodes = postAreaNode.SelectNodes("div/p//node()[not(node())]");
            // and not(normalize-space(text()) = '')
            var texts = textNodes.Select(o => WebUtility.HtmlDecode(o.InnerText).Trim()).ToList();
            return texts;
        }

        protected override string GetNextLink(string Url)
        {

            var postAreaNode = MyHtmlDocument.DocumentNode.SelectSingleNode("//div[@class='area_view']");
            var nextLinkNodes = postAreaNode.SelectNodes(".//div[contains(@class, 'another_category')]/table//a");
            var index = 0;
            string nextLink = null;
            foreach (HtmlNode node in nextLinkNodes)
            {
                var possibleCurrentNode = node.Attributes["class"]?.Value;
                if (possibleCurrentNode == "current") break;
                else
                {
                    index += 1;
                }
            }

            if (index > 0)
            {
                nextLink = WebUtility.HtmlDecode(nextLinkNodes[index - 1].Attributes["href"].Value);
            }
            var fullNextLink = String.Join('/', Url.Split("/")[0..3]) + nextLink;
            if (OpenedWebList.Contains(fullNextLink) || nextLink == null)
            {
                fullNextLink = null;
            }
            return fullNextLink;

        }
        protected override void LoadUrl(ChromeDriver mDriver, string Url)
        {

            mDriver.Navigate().GoToUrl(Url);
            var timeout = 30;//seconds
            new WebDriverWait(mDriver, TimeSpan.FromSeconds(timeout))
                .Until(ExpectedConditions.ElementExists(By.XPath("//div[@class='area_view']//div[contains(@class, 'another_category')]/table//a[@class='current']")));
            var html = mDriver.PageSource;
            MyHtmlDocument.LoadHtml(html);
            OpenedWebList.Add(Url);
        }
    }
}