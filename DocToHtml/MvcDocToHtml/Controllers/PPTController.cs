using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model;
using Aspose.Slides;
using System.Drawing;
using Webdiyer.WebControls.Mvc;
using System.Configuration;

namespace MvcDocToHtml.Controllers
{
    public class PPTController : Controller
    {
        private Model.DataClasses1DataContext db = new DataClasses1DataContext();
        //
        // GET: /PPT/
        private string filePath = ConfigurationManager.AppSettings["SysDataPath"].ToString().Substring(1);
        #region Get
        public ActionResult Index(int id = 1)
        {
           
            var list = from a in db.UploadFile
                       where a.Type == 1
                       orderby a.CreateTime descending
                       select new UploadFile1
                       {
                           ID=a.ID,
                           Name=a.Name,
                           ImgCount=a.ImgCount,
                           Path = filePath + a.Path,
                           CreateTime=a.CreateTime
                       };
            var model = list.ToPagedList(id,10);
            return View(model);
        }
        public ActionResult ShowImg()
        {
            return View();
        }
        #endregion

        #region Post
        [HttpPost]
        public ActionResult UpFile(HttpPostedFileBase txtUpLoad)
        {
            string result = string.Empty;
            try
            {
                var file = txtUpLoad;
                if (file == null)
                {
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = 0 });//文件不存在
                }
                else
                {
                    var fileName = file.FileName;
                    var length = file.ContentLength;
                    if (length > 1024 * 1000 * 4)
                    {
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = 1 });//文件大小不能超过4M
                    }
                    else
                    {
                        //截取图片后缀
                        var supportedTypes = new[] { "ppt", "pptx" };
                        var fileExt = System.IO.Path.GetExtension(fileName).Substring(1);
                        if (supportedTypes.Contains(fileExt))
                        {
                            string myConn = ConfigurationManager.AppSettings["SysDataPath"].ToString();
                            //设置文件上传的文件夹路径
                            var savePath = Server.MapPath(myConn+"/ppt/");
                            //将ppt上传到服务器
                            //file.SaveAs(filePath);
                            string newName = Common.PulicClass.UpLoadfile(file, savePath);
                            string folder = newName.Substring(0, newName.IndexOf('.'));
                            // ppr转image
                            int imgCount = PPTToImg(newName, folder);
                            // 路径保存到表
                            UploadFile model = new UploadFile();
                            model.Name = fileName.Trim();
                            model.Path = ("/img/" + folder + "/").Trim();
                            model.ImgCount = imgCount;
                            model.Type = 1;
                            model.CreateTime = System.DateTime.Now;
                            model.WordTep = 0;
                            db.UploadFile.InsertOnSubmit(model);
                            db.SubmitChanges();
                            result = Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = 2 });  //保存成功
                        }
                        else
                        {
                            result = Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = -1 });//GetJson(false, "上传图片格式不正确！");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return Content(result);
        }

        // 删除
        [HttpPost]
        public ActionResult DelPPT()
        {
            string result = string.Empty;
            string id = Request.Form["id"] ?? "";
            if (!string.IsNullOrEmpty(id))
            {
                UploadFile model = db.UploadFile.SingleOrDefault(f => f.ID == int.Parse(id));     
                try
                {
                    if (model != null)
                    {
                        db.UploadFile.DeleteOnSubmit(model);
                        db.SubmitChanges();
                        // 删除旧PPT
                        var oldPath = filePath + model.Path.TrimEnd(new char[] { '/'});
                        DirectoryInfo dir = new DirectoryInfo(Server.MapPath(oldPath));
                        if(dir.Exists)
                        dir.Delete(true);
                        result = Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = 0 });
                    }
                }
                catch (Exception ex)
                {
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(new { msg = -1 });
                }
            }
            return Content(result);
        }
        #endregion

        #region 内置方法
        /// <summary>
        /// PPT文件转换
        /// </summary>
        /// <param name="pptPath">ppt路径</param>
        /// <param name="imgPath">图片保存的路径</param>
        private int PPTToImg(string pptPath, string imgName)
        {
           string dataPath = ConfigurationManager.AppSettings["SysDataPath"].ToString();
            string paramSource = Server.MapPath(dataPath+"/ppt/") + pptPath;
            string imgPath = dataPath+"/img/" + imgName;
            CheckDirect(Server.MapPath(imgPath));
            string paramTarget = Server.MapPath(imgPath);
           
            var index = 0;
            Aspose.Slides.Pptx.PresentationEx pres = new Aspose.Slides.Pptx.PresentationEx(paramSource);
            foreach (Aspose.Slides.Pptx.SlideEx p in pres.Slides)
            {
                index++;
                //Create a full scale image
                Image bmp = p.GetThumbnail(1f, 1f); ;
                //Save the image to disk in JPEG format
                bmp.Save(Path.Combine(paramTarget, string.Format("{0}.jpg", index)), System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            return index;
        }
      /// <summary>
      /// 调用office的Com组件
      /// </summary>
      /// <param name="pptPath"></param>
      /// <returns></returns>
        private int PPTToImgCom(string pptPath,string imgName)
        {
            var app = new Microsoft.Office.Interop.PowerPoint.Application();
            string paramSource = Server.MapPath("~/file/ppt/") + pptPath;
            string imgPath = "~/file/img/" + imgName;
            CheckDirect(Server.MapPath(imgPath));
            string paramTarget = Server.MapPath(imgPath);
             var ppt = app.Presentations.Open(paramSource, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
            var index = 0;
            var fileName = System.IO.Path.GetFileNameWithoutExtension(pptPath);
            foreach (Microsoft.Office.Interop.PowerPoint.Slide slid in ppt.Slides)
            {
                ++index;
                //设置图片大小
                slid.Export(Path.Combine(paramTarget, string.Format("{0}.jpg", index)), "jpg", 800, 600);

                //根据屏幕尺寸。设置图片大小
                //slid.Export(imgPath+string.Format("page{0}.jpg",index.ToString()), "jpg", Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            }
            //释放资源
            ppt.Close();
            app.Quit();
            GC.Collect();
            return index;
        }
        private void CheckDirect(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }
        }
        #endregion
    }
}