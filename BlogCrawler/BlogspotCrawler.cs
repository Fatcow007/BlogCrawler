using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace BlogCrawler
{
    internal class BlogspotCrawler
    {
        public HtmlDocument MyHtmlDocument { get; }

        private BlogspotCrawler()
        {
            MyHtmlDocument = new HtmlDocument();
        }

        private static readonly Lazy<BlogspotCrawler> lazy = new Lazy<BlogspotCrawler>(() => new BlogspotCrawler());
        public static BlogspotCrawler Instance
        {
            get
            {
                return lazy.Value;
            }
        }



        internal int Run(ChromeDriver mDriver, string fileName, string Url)
        {

            Console.Clear();


            Console.WriteLine("[Blogspot 크롤러 작동중] URL : {0}", Url);

            var label = GetLabelForXmlFeed(mDriver, Url);

            Console.Write("Blogspot RSS Feed 가져오는 중 ...");
            var doc = GetXmlDocument(Url, label);
            Console.WriteLine("성공!");
            var titles = new List<string>();
            var texts = new List<string>();

            // Creating namespace object    
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://www.w3.org/2005/Atom");

            var entryNodes = doc.SelectNodes("ns:feed/ns:entry", nsmgr);

            Console.Write("XML 파일 파싱 중 ...");
            foreach (XmlNode node in entryNodes)
            {
                var title = node.SelectSingleNode("ns:title", nsmgr).InnerText;
                titles.Add(title);
                var html = node.SelectSingleNode("ns:content", nsmgr).InnerText;
                MyHtmlDocument.LoadHtml(html);
                var elements = MyHtmlDocument.DocumentNode.SelectNodes("child::*");
                texts.Add(string.Join('\n', elements.Select(o => WebUtility.HtmlDecode(o.InnerText)).ToList()));
            }
            titles.Reverse();
            texts.Reverse();
            Console.WriteLine("성공!");


            Console.Write("텍스트 파일 작성 중 ...");
            using (StreamWriter w = new StreamWriter(fileName, false, System.Text.Encoding.Default))
            {
                foreach ((string title, string text) in titles.Zip(texts, Tuple.Create))
                {
                    string finalText = title + "\n\n" + text;
                    w.WriteLine(finalText);
                    w.Flush();
                }
            }
            Console.WriteLine("성공!");


            Console.WriteLine("모든 작업 완료!");

            return texts.Count;
        }
        private string GetLabelForXmlFeed(ChromeDriver mDriver, string Url)
        {
            Console.Write("RSS Feed를 위한 Label 추출중...");
            mDriver.Url = Url;
            new WebDriverWait(mDriver, TimeSpan.FromSeconds(3)).Until(ExpectedConditions.ElementExists(By.XPath("//span[@class='post-info-categorynbt']/a")));
            
            var label = mDriver.FindElementByXPath("//span[@class='post-info-categorynbt']/a").Text;

            Console.WriteLine("성공!");
            return label;
        }
        private XmlDocument GetXmlDocument(String Url, String label)
        {
            string feedUrl = string.Join('/', Url.Split("/")[0..3]) + @"/feeds/posts/default/-/" + label + "?redirect=false&max-results=500";

            XmlDocument doc = new XmlDocument();
            doc.Load(feedUrl);
            return doc;

        }


    }
}