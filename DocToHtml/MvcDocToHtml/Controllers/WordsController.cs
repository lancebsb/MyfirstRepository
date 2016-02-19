using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Aspose.Words;
using Model;
using Webdiyer.WebControls.Mvc;
using HtmlAgilityPack;
using System.Text;

namespace MvcDocToHtml.Controllers
{
    public class WordsController : Controller
    {
        Model.DataClasses1DataContext db = new DataClasses1DataContext();
        #region Get
        public ActionResult Index(int id = 1)
        {
            var list = (from c in db.UploadFile.Where(x => x.Type == 0)
                        select new word
                       {
                           ID = c.ID,
                           Name = c.Name,
                           CreateTime = c.CreateTime,
                           Path = c.Path,
                           WordTep = c.WordTep == 1 ? "搜才" : "智联",
                           tep = c.WordTep
                       }).OrderByDescending(x => x.ID).ToPagedList(id, 10);
            return View(list);



        }

        public ActionResult Info()
        {
            int _int = 0;
            List<model> li = new List<model>();
            int ID = 0;
            ID = Convert.ToInt32(Request.QueryString["ID"]);
            int tep = 1;
            if (int.TryParse(Request.QueryString["Tep"], out _int))
            {
                tep = _int;
            }
            UploadFile file = db.UploadFile.Where(x => x.ID == ID).FirstOrDefault();
            if (file != null)
            {
                string str = file.DocContent;
                if (tep == 1)
                {
                    AddList(li, "姓名", GetStr(str, "姓\\s*名", "性\\s*别"));
                    AddList(li, "性别", GetStr(str, "性\\s*别", "出\\s*生\\s*年\\s*月"));
                    AddList(li, "出生年月", GetStr(str, "出\\s*生\\s*年\\s*月", "现\\s*居\\s*住\\s*地"));
                    AddList(li, "现居住地", GetStr(str, "现\\s*居\\s*住\\s*地", "参\\s*加\\s*工\\s*作\\s*时\\s*间"));
                    AddList(li, "参加工时间", GetStr(str, "参\\s*加\\s*工\\s*作\\s*时\\s*间", "最\\s*高\\s*学\\s*历"));
                    AddList(li, "最高学历", GetStr(str, "最\\s*高\\s*学\\s*历", "求\\s*职\\s*状\\s*态"));
                    AddList(li, "求职状态", GetStr(str, "求\\s*职\\s*状\\s*态", "婚\\s*姻\\s*状\\s*况"));
                    AddList(li, "婚姻状况", GetStr(str, "婚\\s*姻\\s*状\\s*况", "E\\s*-\\s*mail"));
                    AddList(li, "E-mail", GetStr(str, "E\\s*-\\s*mail", "手\\s*机"));
                    AddList(li, "手机", GetStr(str, "手\\s*机", "求\\s*职\\s*意\\s*向"));
                    AddList(li, "求职意向", GetStr(str, "求\\s*职\\s*意\\s*向", "应\\s*聘\\s*职\\s*位"));
                    AddList(li, "应聘职位", GetStr(str, "应\\s*聘\\s*职\\s*位", "应\\s*聘\\s*行\\s*业"));
                    AddList(li, "应聘行业", GetStr(str, "应\\s*聘\\s*行\\s*业", "工\\s*作\\s*地\\s*区"));
                    AddList(li, "工作地区", GetStr(str, "工\\s*作\\s*地\\s*区", "应\\s*聘\\s*类\\s*型"));
                    AddList(li, "应聘类型", GetStr(str, "应\\s*聘\\s*类\\s*型", "期\\s*望\\s*月\\s*薪"));
                    AddList(li, "期望月薪", GetStr(str, "期\\s*望\\s*月\\s*薪", "到\\s*岗\\s*时\\s*间"));
                    AddList(li, "到岗时间", GetStr(str, "到\\s*岗\\s*时\\s*间", "自\\s*我\\s*评\\s*价"));
                    AddList(li, "自我评价", GetStr(str, "自\\s*我\\s*评\\s*价", "工\\s*作\\s*经\\s*历"));
                    AddList(li, "工作经历", GetStr(str, "工\\s*作\\s*经\\s*历", "教\\s*育\\s*背\\s*景"));
                    AddList(li, "教育背景", GetStr(str, "教\\s*育\\s*背\\s*景", "在\\s*校\\s*实\\s*践"));
                    AddList(li, "在校实践", GetStr(str, "在\\s*校\\s*实\\s*践", "IT\\s*技\\s*能"));
                    AddList(li, "IT技能", GetStr(str, "IT\\s*技\\s*能", "语\\s*言\\s*技\\s*能"));
                    AddList(li, "语言技能", GetStr(str, "语\\s*言\\s*技\\s*能", "培\\s*训\\s*记\\s*录"));
                    AddList(li, "培训记录", GetStr(str, "培\\s*训\\s*记\\s*录", "附\\s*加\\s*信\\s*息"));
                    AddList(li, "附加信息", GetStr(str, "附\\s*加\\s*信\\s*息", string.Empty));
                }
                if (tep == 2)
                {
                    string id = file.Path.Substring(0, file.Path.IndexOf('.')+1);
                    ParseIndexPage(Server.MapPath("~/file/html/"+id+"html"), li);

                }
            }

            return View(li);

        }
        #endregion

        #region Post
        [HttpPost]
        public ActionResult AddFile(HttpPostedFileBase file, string wtep)
        {
            int _int = 0;
            int.TryParse(wtep, out _int);

            //长传前判断
            //得到文件的大小
            int fileSize = file.ContentLength;
            //文件名称
            string fileOldName = file.FileName;
            if (fileOldName.Contains("\\"))
            {
                fileOldName = fileOldName.Substring(fileOldName.LastIndexOf('\\') + 1);
            }
            if (fileOldName == string.Empty)
            {
                return Content("0");
            }
            //得到扩展名
            string fileExt = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            if (fileExt != "doc" && fileExt != "docx")
            {
                return Content("1");
            }
            if (fileSize > 1048576 * 5)
            {
                return Content("2");
            }
            //文件路径 
            string uploaddocpath = Server.MapPath("~/file/doc/");
            string uploadhtmlpath = Server.MapPath("~/file/html/");

            string fileNewName = Common.PulicClass.UpLoadfile(file, uploaddocpath);
            Document doc = new Document(uploaddocpath + fileNewName);
            string htmlnma = "";
            if (fileNewName.Contains("docx"))
            {
                htmlnma = fileNewName.Replace("docx", "html");
            }
            else
            {
                htmlnma = fileNewName.Replace("doc", "html");
            }

            doc.Save(uploadhtmlpath + htmlnma, Aspose.Words.SaveFormat.Html);
            string fileText = doc.GetText();
            //保存

            using (Model.DataClasses1DataContext db = new DataClasses1DataContext())
            {
                Model.UploadFile File = new UploadFile();
                File.Name = fileOldName;
                File.Path = fileNewName;
                File.ImgCount = 0;
                File.Type = 0;
                File.DocContent = fileText;
                File.CreateTime = DateTime.Now;
                File.WordTep = _int;
                db.UploadFile.InsertOnSubmit(File);
                db.SubmitChanges();

            }
            return Content("3");
        }
        #endregion


        #region 删除
        [HttpPost]
        public ActionResult Delete()
        {
            try
            {
                int ID = Convert.ToInt32(Request.QueryString["ID"]);
                UploadFile file = db.UploadFile.Where(x => x.ID == ID).FirstOrDefault();
                db.UploadFile.DeleteOnSubmit(file);
                db.SubmitChanges();
                return Content("1");
            }
            catch (Exception)
            {

                return Content("0");
            }

        }

        #endregion

        #region 内置方法

        //通过正则匹配字符串
        /// <summary>
        /// 通过正则匹配两个字符串之间的字符串
        /// </sum通过正则匹配字符串mary>
        /// <param name="matchstr">要匹配的字符串</param>
        /// <param name="first">开始的字符串</param>
        /// <param name="end">结束的字符串</param>
        /// <returns></returns>
        private string GetStr(string matchstr, string first, string end)
        {
            string rstr = "";
            //if (!matchstr.Contains(first))
            //{
            //    first = string.Empty;
            //}
            //if (!matchstr.Contains(end))
            //{
            //    end = string.Empty;
            //}
            //if (first == string.Empty && end == string.Empty)
            //{
            //    return string.Empty;
            //}
            string s = end == string.Empty ? string.Empty : "?";
            Regex reg1 = new Regex(first + @"([\s\S]*" + s + ")" + end, RegexOptions.IgnoreCase);
            //Regex reg1 = new Regex(@"(?<=" + first + ").*(?=" + end + ")");
            Match m1 = reg1.Match(matchstr);
            if (m1.Success)
            {
                rstr = m1.Result("$1");

            }
            return rstr;
        }
        //添加数据到集合
        private void AddList(List<model> t, string name, string val)
        {

            model a = new model();
            a.name = name.Replace("\a", string.Empty);
            a.val = val.Replace("\a", string.Empty);
            if (a.val.Length > 0)
            {
                t.Add(a);
            }
        }
        //实体模型
        public class model
        {
            public string name { set; get; }
            public string val { set; get; }
        }
        public class word
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public int Type { get; set; }
            public string DocContent { get; set; }
            public Nullable<int> ImgCount { get; set; }
            public Nullable<System.DateTime> CreateTime { get; set; }
            public string WordTep { get; set; }
            public Nullable<int> tep { get; set; }
        }
        public void ParseIndexPage(string url, List<model> li)
        {

            HtmlDocument document = new HtmlDocument();
            document.Load(url);
            //去掉html中的注释
            foreach (var comment in document.DocumentNode.SelectNodes("//comment()").ToArray())
                comment.Remove();
            HtmlNode rootNode = document.DocumentNode;
            string CategoryNameXPath = "//html[1]/body[1]/div[1]/p";
            HtmlNodeCollection categoryNodeList = rootNode.SelectNodes(CategoryNameXPath);
            HtmlNode temp = null;
            HtmlNode hn1 = rootNode.SelectNodes("//html[1]/body[1]/div[1]/table[1]")[0];
            li.Add(new model { name = "简历介绍", val = hn1.InnerText });
            foreach (HtmlNode categoryNode in categoryNodeList)
            {

                temp = HtmlNode.CreateNode(categoryNode.OuterHtml);

                HtmlNodeCollection attrs1 = categoryNode.SelectNodes("./span");
                bool b = false;
                foreach (HtmlNode node in attrs1)
                {
                    if (!node.Attributes[0].Value.Contains("background-color:#d9d9d9;"))
                    {
                        continue;
                    }
                    b = true;
                    break;
                }
                if (b == false)
                {
                    continue;
                }
                model a = new model();
                a.name = temp.InnerText.Replace("&#xa0;", string.Empty);
                if (a.name == string.Empty)
                {
                    continue;
                }
                //获取当前节点的下一个兄弟节点

                bool f = true;
                HtmlNode nextnode = categoryNode.NextSibling;
                StringBuilder str = new StringBuilder(nextnode.OuterHtml);
                if (a.name.Contains("ID："))
                {
                    str = new StringBuilder(nextnode.InnerText);
                }
                while (f)
                {
                    nextnode = nextnode.NextSibling;
                    if (nextnode == null||iscontent(nextnode))
                    {
                        f = false;

                    }
                    else
                    {
                        str.Append(nextnode.OuterHtml);
                    }

                }
                a.val = str.ToString();
                li.Add(a);
            };
        }

        public bool iscontent(HtmlNode hn)
        {
            bool r = false;
            HtmlNodeCollection attrs1 = hn.SelectNodes("./span");
            if (attrs1 == null)
                return false;
            if (attrs1.Count() <= 0)
            {

            }
            else
            {
                if( attrs1[0].Attributes[0].Value.Contains("background-color:#d9d9d9;"))
                {
                    r = true;
                }
            }
            return r;
        }
        #endregion


    }
}
