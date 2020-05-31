using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BlogCrawler
{
    internal abstract class TypeOneCrawler
    {

        //For Console Output
        protected string FileName { get; private set; }
        protected string CurrentUrl { get; private set; }
        protected string LastUrl { get; private set; }
        protected int PageCount { get; private set; }
        protected string CurrentTitle { get; private set; }
        protected string LastTitle { get; private set; }
        protected string ProgressString { get; private set; }


        protected int ConsoleScreenWidth { get; private set; }
        protected HtmlDocument MyHtmlDocument { get; }
        protected List<string> OpenedWebList { get; private set; }

        private readonly int DELAY_BETWEEN_PAGES_MIN = 2000; //milliseconds
        private readonly int DELAY_BETWEEN_PAGES_MAX = 3000; //milliseconds


        protected TypeOneCrawler()
        {
            ConsoleScreenWidth = Console.WindowWidth;
            OpenedWebList = new List<string>();
            ResetConsole(NoUpdate: true);
            MyHtmlDocument = new HtmlDocument();
        }


        public int Run(ChromeDriver mDriver, string fileName, string Url, int chapterLimit = -1)
        {
            var currentUrl = Url;
            var pageCount = 0;
            //Clear Console
            Console.Clear();

            using (StreamWriter w = new StreamWriter(fileName, false, System.Text.Encoding.Default))
            {
                while (currentUrl != null)
                {
                    pageCount++;

                    UpdateConsole(
                        currentTitle:"",
                        currentUrl: currentUrl,
                        fileName: fileName,
                        pageCount: pageCount,
                        progressString: "Url 로딩 중...");

                    LoadUrl(mDriver, currentUrl);

                    UpdateConsole(
                        progressString: "크롤링 시작!");



                    CrawlSingleWeb(w);

                    UpdateConsole(
                        progressString: "크롤링 종료!");


                    UpdateConsole(
                        progressString: "다음 링크 가져오는 중...");

                    var lastUrl = currentUrl;
                    currentUrl = GetNextLink(currentUrl);

                    if (currentUrl == null) break;



                    //서버 부하를 줄이기 위해 랜덤 딜레이
                    Random random = new Random();
                    int num = random.Next(DELAY_BETWEEN_PAGES_MIN, DELAY_BETWEEN_PAGES_MAX);
                    double delayInSec = num / 1000D;
                    UpdateConsole(
                        progressString: "서버의 부하를 줄이기 위해 " + delayInSec + "초 대기 중...");
                    Thread.Sleep(num);
                    var lastTitle = CurrentTitle;
                    UpdateConsole(
                        lastUrl: lastUrl,
                        lastTitle: lastTitle);
                }
            }

            return pageCount;

        }

        private void CrawlSingleWeb(StreamWriter w)
        {

            UpdateConsole(
                progressString: "제목 값 가져오는 중...");
            var title = GetTitle();
            UpdateConsole(
                currentTitle: title,
                progressString: "소설 텍스트 가져오는 중...");
            var texts = GetTexts();
            UpdateConsole(
                progressString: "텍스트 결합 중...");
            var rawText = string.Join('\n', texts);
            var finalText = title + "\n\n" + rawText + "\n\n";
            UpdateConsole(
                progressString: "파일에 쓰는 중...");
            w.WriteLine(finalText);
            w.Flush();
        }

        private void WriteToConsole()
        {
            var UrlText = CurrentUrl;
            var LastUrlText = LastUrl;
            var lines = new String('-', ConsoleScreenWidth - 3);
            var emptyLine = new String(' ', ConsoleScreenWidth);
            var lineFirstText = "┌" + lines + "┐";
            var lineText = "│" + lines + "│";
            var lineLastText = "└" + lines + "┘";
            if (UrlText.Length > lineText.Length - 24)
            {
                UrlText = UrlText.Substring(0, lineText.Length - 24);
            }
            if (LastUrlText.Length > lineText.Length - 24)
            {
                LastUrlText = LastUrlText.Substring(0, lineText.Length - 24);
            }
            var fileInfoText = ("[" + FileName + "][" + PageCount.ToString() + "페이지]");
            fileInfoText = fileInfoText.PadRight(lineText.Length - UnicodeWidth.GetWidth(fileInfoText) + fileInfoText.Length - 2);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(lineFirstText);
            WriteLineWithPaddings(fileInfoText);
            Console.WriteLine(lineText);
            WriteLineWithPaddings("[이전 페이지]      : " + LastUrlText);
            WriteLineWithPaddings("[제목]             : " + LastTitle);
            Console.WriteLine(lineText);
            WriteLineWithPaddings("[현재 페이지]      : " + UrlText);
            WriteLineWithPaddings("[제목]             : " + CurrentTitle);
            WriteLineWithPaddings("[진행상태]         : " + ProgressString);
            Console.WriteLine(lineLastText);
            Console.WriteLine(emptyLine);
            Console.WriteLine(emptyLine);
            Console.WriteLine(emptyLine);
            Console.WriteLine(emptyLine);
            Console.WriteLine(emptyLine);
            Console.WriteLine(emptyLine);
        }

        private void WriteLineWithPaddings(string line)
        {
            Console.Write("│");
            Console.Write(line);
            Console.Write(new string(' ', ConsoleScreenWidth - Console.CursorLeft - 2));
            Console.WriteLine("│");
        }

        private void UpdateConsole(
            string fileName = null,
            string currentUrl = null,
            string lastUrl = null,
            int pageCount = -1,
            string currentTitle = null,
            string lastTitle = null,
            string progressString = null,
            bool NoUpdate = false
            )
        {
            FileName = fileName ?? FileName;
            CurrentUrl = currentUrl ?? CurrentUrl;
            LastUrl = lastUrl ?? LastUrl;
            PageCount = pageCount == -1 ? PageCount : pageCount;
            CurrentTitle = currentTitle ?? CurrentTitle;
            LastTitle = lastTitle ?? LastTitle;
            ProgressString = progressString ?? ProgressString;
            if (!NoUpdate) WriteToConsole();
        }

        private void ResetConsole(bool NoUpdate = false)
        {
            FileName = "";
            CurrentUrl = "";
            LastUrl = "";
            PageCount = 0;
            CurrentTitle = "";
            LastTitle = "";
            ProgressString = "";
            if (!NoUpdate) WriteToConsole();
        }

        protected abstract string GetTitle();
        protected abstract List<string> GetTexts();
        protected abstract void LoadUrl(ChromeDriver mDriver, string Url);
        protected abstract string GetNextLink(string Url);
    }
}