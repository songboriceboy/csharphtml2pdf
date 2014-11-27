using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Fizzler;
using Fizzler.Systems.HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Web;
namespace GetWebContent
{
    public partial class Frm_Main : Form
    {
        private WebDownloader m_wd = new WebDownloader();
        private TaskDelegate deles;
        private BloomFilter m_bf = new BloomFilter(10485760);
        public Frm_Main()
        {
            InitializeComponent();
        }
        private void GetTitle()
        {
            string strContent
                = m_wd.GetPageByHttpWebRequest(this.textBoxUrl.Text, Encoding.UTF8);
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument
            {
                OptionAddDebuggingAttributes = false,
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
                OptionReadEncoding = true
            };

            htmlDoc.LoadHtml(strContent);
            string strTitle = "";
            HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//title");
            // Extract Title
            if (!Equals(nodes, null))
            {
                strTitle = string.Join(";", nodes.
                    Select(n => n.InnerText).
                    ToArray()).Trim();
            }
            strTitle = strTitle.Replace("博客园", "");
            strTitle = Regex.Replace(strTitle, @"[|/\;:*?<>&#-]", "").ToString();
            strTitle = Regex.Replace(strTitle, "[\"]", "").ToString();
            this.textBoxTitle.Text = strTitle.TrimEnd();
        }
        private void GetMainContent()
        {
            string strContent
                = m_wd.GetPageByHttpWebRequest(this.textBoxUrl.Text, Encoding.UTF8);
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument
            {
                OptionAddDebuggingAttributes = false,
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
                OptionReadEncoding = true
            };

            htmlDoc.LoadHtml(strContent);

            IEnumerable<HtmlNode> NodesMainContent = htmlDoc.DocumentNode.QuerySelectorAll(this.textBoxCssPath.Text);

            if (NodesMainContent.Count() > 0)
            {
                this.richTextBox1.Text = NodesMainContent.ToArray()[0].OuterHtml;
                this.webBrowser1.DocumentText = this.richTextBox1.Text;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            GetTitle();
            GetMainContent();
        }

        /*
  站点		--->  CSS路径
"Cnblogs"	---> "div#cnblogs_post_body"
"Csdn"		---> "div#article_content.article_content"
"51CTO"		---> "div.showContent"
"Iteye"		---> "div#blog_content.blog_content"
"ItPub"		---> "div.Blog_wz1"
"ChinaUnix" ---> "div.Blog_wz1"
          */
      

        private void Frm_Main_Load(object sender, EventArgs e)
        {
            this.textBoxUrl.Text = "http://www.cnblogs.com/ice-river/p/4112323.html";
            this.textBoxCssPath.Text = "div#cnblogs_post_body";
            deles = new TaskDelegate(new ccTaskDelegate(RefreshTask));
        }

        protected string NormalizeLink(string baseUrl, string link)
        {
            return link.NormalizeUrl(baseUrl);
        }

        protected string GetNormalizedLink(string baseUrl, string decodedLink)
        {
            string normalizedLink = NormalizeLink(baseUrl, decodedLink);

            return normalizedLink;
        }

        public string DownloadPic(string strLink, string strNewPage
            , string strPageTitle)
        {



            WebClient wc = new WebClient();
 
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument
            {
                OptionAddDebuggingAttributes = false,
                OptionAutoCloseOnEnd = true,
                OptionFixNestedTags = true,
                OptionReadEncoding = true
            };

            htmlDoc.LoadHtml(strNewPage);
            DocumentWithLinks links = htmlDoc.GetSrcLinks();
            int i = 1;
            string baseUrl = new Uri(strLink).GetLeftPart(UriPartial.Authority);

            foreach (string strPicLink in links.Links)
            {
                if (string.IsNullOrEmpty(strPicLink))
                {
                    continue;
                }
          
                try
                {
                    string strExtension = System.IO.Path.GetExtension(strPicLink);

                    if (strExtension == ".js" || strExtension == ".swf")
                        continue;
                  
                    if (strExtension == "")
                    {
                        strExtension = ".jpg";
                    }

                    string normalizedPicLink = GetNormalizedLink(baseUrl, strPicLink);
                    strNewPage = DownLoadPicInternal(wc, strNewPage, strPageTitle, strPicLink, normalizedPicLink, strExtension, ref i);
                }
                catch (Exception ex)
                {
                } //end try
            }
            strPageTitle = strPageTitle.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "")
             .Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
            strPageTitle = Regex.Replace(strPageTitle, @"[|•/\;.':*?<>-]", "").ToString();
            strPageTitle = Regex.Replace(strPageTitle, "[\"]", "").ToString();
            strPageTitle = Regex.Replace(strPageTitle, @"\s", "");


            File.WriteAllText(Path.Combine(strPageTitle, "index.html"), strNewPage, Encoding.UTF8);

            PrintLog(" 文章 [" + strPageTitle + "] 的全部图片下载完成!!!\n");
            return strPageTitle;
        }
        protected string DownLoadPicInternal(WebClient wc, string strNewPage, string strPageTitle, string strPicLink
                               , string strTureLink, string strExtension, ref int i)
        {
            strPageTitle = strPageTitle.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("?", "")
            .Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
            strPageTitle = Regex.Replace(strPageTitle, @"[|•/\;.':*?<>-]", "").ToString();
            strPageTitle = Regex.Replace(strPageTitle, "[\"]", "").ToString();
            strPageTitle = Regex.Replace(strPageTitle, @"\s", "");

            if (!Directory.Exists(Application.StartupPath + "\\" + strPageTitle))//判断是否存在
            {
                Directory.CreateDirectory(Application.StartupPath + "\\" + strPageTitle);//创建新路径
            }
           
            int[] nArrayOffset = new int[2];
            nArrayOffset = m_bf.getOffset(strPicLink);
            strNewPage = strNewPage.Replace(strPicLink, nArrayOffset[0].ToString() + nArrayOffset[1].ToString() + strExtension);
            string strSavedPicPath = Path.Combine(strPageTitle, nArrayOffset[0].ToString() + nArrayOffset[1].ToString() + strExtension);
        

            PrintLog(" 开始下载文章 [" + strPageTitle + "] 的第" + i.ToString() + "张图片\n");
            strTureLink = HttpUtility.UrlDecode(strTureLink);
            wc.DownloadFile(strTureLink, Application.StartupPath + "\\" + strSavedPicPath);
            PrintLog(" 下载完成文章 [" + strPageTitle + "] 的第" + i.ToString() + "张图片\n");
            System.Threading.Thread.Sleep(300);
            i++;
            return strNewPage;

        }
    

        public void RefreshTask(DelegatePara dp)
        {
            //如果需要在安全的线程上下文中执行
            if (this.InvokeRequired)
            {
                this.Invoke(new ccTaskDelegate(RefreshTask), dp);
                return;
            }

            //转换参数
            string strLog = (string)dp.strLog;
            WriteLog(strLog);

        }
        protected void PrintLog(string strLog)
        {
            DelegatePara dp = new DelegatePara();

            dp.strLog = strLog;
            deles.Refresh(dp);
        }
        public void WriteLog(string strLog)
        {
            try
            {
                strLog = System.DateTime.Now.ToLongTimeString() + " : " + strLog;

                this.richTextBoxLog.AppendText(strLog);
                this.richTextBoxLog.SelectionStart = int.MaxValue;
                this.richTextBoxLog.ScrollToCaret();
            }
            catch
            {
            }


        }
        private void buttonGetPics_Click(object sender, EventArgs e)
        {
            GetTitle();
            GetMainContent();
            this.tabControl1.SelectTab(2);
            DownloadPic(this.textBoxUrl.Text, this.richTextBox1.Text, this.textBoxTitle.Text);
        }

        private void buttonPDF_Click(object sender, EventArgs e)
        {
            GetTitle();
            GetMainContent();
            this.tabControl1.SelectTab(2);
            string strPageTitle = DownloadPic(this.textBoxUrl.Text, this.richTextBox1.Text, this.textBoxTitle.Text);

            HTML2PDF html2pdf = new HTML2PDF(Application.StartupPath + "\\" + strPageTitle + "\\", deles);
            html2pdf._html2pdf(strPageTitle);
        }
    }
}
