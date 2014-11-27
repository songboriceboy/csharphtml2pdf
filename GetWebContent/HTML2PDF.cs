using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;


namespace GetWebContent
{
    public class HTML2PDF
    {
        private TaskDelegate m_delesPDF;
        private string m_strPath;
   
        public HTML2PDF(string strPath, TaskDelegate delesPDF)
        {
            m_strPath = strPath;
            m_delesPDF = delesPDF;
        }
       
    
        //注意:路径中不能有空格
        public bool _html2pdf(string fileName)
        {

            string strPdfSavedPath = m_strPath;
            if (!Directory.Exists(strPdfSavedPath))//判断是否存在
            {
                Directory.CreateDirectory(strPdfSavedPath);//创建新路径
            }

            if (!File.Exists(strPdfSavedPath + fileName + ".pdf"))
            {

                string strHtmlSavedPath = m_strPath;
                string file_flvbind = Application.StartupPath + @"\PDFLIB\wkhtmltopdf.exe";
                //MoveFolderTo(fileName, Application.StartupPath + @"\PDFLIB\");
                //生成ProcessStartInfo
                ProcessStartInfo pinfo = new ProcessStartInfo(file_flvbind);
                //pinfo.WorkingDirectory = Application.StartupPath + @"\PDFLIB\";
                pinfo.WorkingDirectory = strHtmlSavedPath;
                //设置参数
                StringBuilder sb = new StringBuilder();
                sb.Append("--footer-line ");
                sb.Append("--footer-center \"powered by 际为软件事务所(http://www.cnblogs.com/ice-river)\" ");
                sb.Append("\"" + "index.html\"");
              
                sb.Append(" \"" + strPdfSavedPath + fileName + ".pdf" + "\"");

                pinfo.Arguments = sb.ToString();
                //隐藏窗口
                pinfo.WindowStyle = ProcessWindowStyle.Hidden;
                //启动程序
                
                Process p = Process.Start(pinfo);
                p.WaitForExit();
                //DeleteFiles(Application.StartupPath + @"\PDFLIB\");
                if (p.ExitCode == 0)
                {
                    DelegatePara dp = new DelegatePara();
                    dp.strLog = "生成 [" + fileName + ".pdf] 成功！\n";
                    
                    m_delesPDF.Refresh(dp);
                    return true;
                }
                else
                {
                    DelegatePara dp = new DelegatePara();
                    dp.strLog = "生成 [" + fileName + ".pdf] 失败！\n";
                    m_delesPDF.Refresh(dp);
                    return false;
                }
            }
            else
            {
                DelegatePara dp = new DelegatePara();
                dp.strLog = "生成 [" + fileName + ".pdf] 成功！\n";
                m_delesPDF.Refresh(dp);
                return true;
            }
        }
       

    }
}
