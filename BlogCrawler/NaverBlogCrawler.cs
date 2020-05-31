using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System.Threading;

namespace BlogCrawler
{
    internal class NaverBlogCrawler : TypeOneCrawler
    {


        private NaverBlogCrawler()
        {
        }

        private static readonly Lazy<NaverBlogCrawler> lazy = new Lazy<NaverBlogCrawler>(() => new NaverBlogCrawler());
        public static NaverBlogCrawler Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        protected override string GetTitle()
        {
            var titleNode = MyHtmlDocument.DocumentNode.SelectSingleNode("//*[@property='og:title']");
            var title = titleNode.Attributes["content"].Value.Trim();
            return title;
        }

        private string GetNaverPostId()
        {
            var ogUrlNode = MyHtmlDocument.DocumentNode.SelectSingleNode("//*[@property='og:url']");
            var ogUrl = ogUrlNode.Attributes["content"].Value;
            var id = ogUrl.Split('/').Last();
            return id;
        }

        protected override List<string> GetTexts()
        {
            var pageId = GetNaverPostId();
            var postViewNode = MyHtmlDocument.DocumentNode.SelectSingleNode("//*[@id='post-view" + pageId + "']");
            var seViewerNode = postViewNode.SelectSingleNode("div/div[@class='se-main-container']");
            var finalTextboxNode = seViewerNode ?? postViewNode;
            var elements = finalTextboxNode.SelectNodes("*[not(contains(@class,'oglink'))]//node()[not(node()) and not(name() = '#comment')]");
            // and not(normalize-space(text()) = '')
            var texts = elements.Select(o => WebUtility.HtmlDecode(o.InnerText).Trim()).ToList();
            return texts;
        }

        protected override string GetNextLink(string Url)
        {
            var elements = MyHtmlDocument.DocumentNode.SelectNodes(
                "//tbody[@id='postBottomTitleListBody']//descendant::tr");
            var index = 0;
            string nextLink = null;
            foreach (var element in elements)
            {
                if (element.Attributes["class"].Value == "on") break;
                else
                {
                    index += 1;
                }
            }

            if (index > 0)
            {
                nextLink = WebUtility.HtmlDecode(elements[index - 1].SelectSingleNode("descendant::a").Attributes["href"].Value);
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
            if (!Url.Contains("PostView.nhn"))
            {
                new WebDriverWait(mDriver, TimeSpan.FromSeconds(timeout))
                    .Until(ExpectedConditions.ElementExists(By.Id("mainFrame")));
                mDriver.SwitchTo().Frame("mainFrame");
            }
            new WebDriverWait(mDriver, TimeSpan.FromSeconds(timeout))
                .Until(ExpectedConditions.ElementExists(By.XPath("//tbody[@id='postBottomTitleListBody']//descendant::a")));
            var html = mDriver.PageSource;
            MyHtmlDocument.LoadHtml(html);
            OpenedWebList.Add(Url);
        }

    }
}
