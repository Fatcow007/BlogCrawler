
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace BlogCrawler
{
    class Crawler
    {

        public readonly static bool DEBUG_MODE = false;
        private readonly static string READ_ME_TEXT =
            "#[현재 사용가능한 블로그 사이트]\n" +
            "#[네이버 블로그    : blog.naver.com]\n" +
            "#[티스토리 블로그  : tistory.com]\n" +
            "#[Blogspot         : blogspot.com]\n" +
            "#\n" +
            "#[사용법]\n" +
            "#아래 빈 공간에 소설 제목, 블로그 URL을  각각 한 줄씩 적으면 됩니다.\n" +
            "#블로그 URL의 경우, 소설의 1화 URL을 넣으면 자동으로 마지막 화까지 다운로드 됩니다.\n" +
            "#동시에 하나 이상의 소설을 다운로드 받으려면, 제목 -> URL 순서만 지켜서 적어주세요.\n" +
            "#*앞에 샾(#)이 붙은 문장은 프로그램에서 무시됩니다.*\n" +
            "#예시:\n" +
            "#소설1 파일이름\n" +
            "#https://blog.naver.com/example/123456789\n" +
            "#소설2 파일이름\n" +
            "#https://example.tistory.com/123\n" +
            "#\n" +
            "#이 줄 다음부터 작성하세요.\n";

        static void Main(string[] args)
        {
            Console.WriteLine("웹크롤러 by Fatcow");
            //Open Chrome Browser #Selenium

            Console.WriteLine("크롬 드라이버 로딩중...");
            new DriverManager().SetUpDriver(new ChromeConfig());
            var chromeOptions = new ChromeOptions();
            var titleList = new List<string>();
            var pageCountList = new List<int>();
            var elapsedTimeList = new List<long>();
            var stopwatch = new Stopwatch();
            chromeOptions.AddArguments("headless", "--log-level=3");
            using (ChromeDriver driver = new ChromeDriver(chromeOptions))
            {
                foreach (Tuple<String, String> element in ReadUrlFile())
                {

                    titleList.Add(element.Item1);
                    var page = 0;
                    stopwatch.Start();
                    if (element.Item2.ToLower().Contains("blog.naver.com"))
                    {
                        page = NaverBlogCrawler.Instance.Run(driver, element.Item1, element.Item2);
                    }else if (element.Item2.ToLower().Contains("blogspot.com"))
                    {
                        page = BlogspotCrawler.Instance.Run(driver, element.Item1, element.Item2);
                    }else if (element.Item2.ToLower().Contains("tistory.com"))
                    {
                        page = TistoryCrawler.Instance.Run(driver, element.Item1, element.Item2);
                    }
                    stopwatch.Stop();
                    elapsedTimeList.Add(stopwatch.ElapsedMilliseconds);
                    stopwatch.Reset();
                    pageCountList.Add(page);

                }
            }
            Console.Clear();
            Console.WriteLine("-----------------------------------------------", titleList.Count);
            Console.WriteLine("[크롤링 완료. 총 {0}개의 txt파일 추출]", titleList.Count);
            for (int i = 0; i < titleList.Count; i++)
            {
                Console.WriteLine("-----------------------------------------------", titleList.Count);
                Console.WriteLine("[{0}]", titleList[i]);
                Console.WriteLine("[{0} 페이지]", pageCountList[i]);
                Console.WriteLine("[소요시간 : {0} 초]", elapsedTimeList[i]/ 1000L);
            }
            Console.WriteLine("-----------------------------------------------\n", titleList.Count);
            if(titleList.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("[블로그리스트.txt를 읽어주세요.]");
            }
            Console.WriteLine("[아무 키나 누르면 종료됩니다]");
            Console.ReadKey();
        }


        private static List<Tuple<String, String>> ReadUrlFile()
        {
            string title;
            string url;
            var fileDir = "블로그리스트.txt";
            var UrlList = new List<Tuple<String, String>>();


            Console.WriteLine("블로그리스트.txt 파일 로딩 중...");
            // Create empty file if it does not exist.
            try
            {

                if (!File.Exists(fileDir))
                {
                    File.WriteAllText("블로그리스트.txt", READ_ME_TEXT);
                }
                else
                {
                    // Read the file and display it line by line.  
                    System.IO.StreamReader file =
                        new System.IO.StreamReader(fileDir);
                    while ((title = file.ReadLine()) != null)
                    {
                        title = title.Trim();
                        if (title.Equals("") || title.StartsWith("#"))
                        {
                            continue;
                        }
                        if ((url = file.ReadLine()) != null)
                        {
                            url = url.Trim();
                            Uri uriResult;
                            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                            if (result)
                            {
                                UrlList.Add(new Tuple<String, String>(title + ".txt", url));
                            }
                            else
                            {
                                Console.WriteLine("{0}의 주소 형식이 잘못되었습니다.", title);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    file.Close();
                }
            }
            catch (Exception e){Console.WriteLine(e);}

            return UrlList;
        }

    }
}
