using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcDocToHtml.Common
{
    public class PulicClass
    {
        /// <summary>
        /// 上传文件到指定文件夹
        /// </summary>
        /// <param name="inputfile"></param>
        /// <param name="uploadfilepath"></param>
        /// <returns></returns>
        public static string UpLoadfile(HttpPostedFileBase inputfile, string uploadfilepath)
        {
            string orifilename = string.Empty;
            string modifyfilename = string.Empty;
            string fileExt = "";//文件扩展名
            int fileSize = 0;//文件大小
            try
            {
                if (inputfile.FileName != string.Empty)
                {
                    //得到文件的大小
                    fileSize = inputfile.ContentLength;
                    //得到扩展名
                    fileExt = inputfile.FileName.Substring(inputfile.FileName.LastIndexOf(".") + 1);
                    //新文件名
                    string guid = System.Guid.NewGuid().ToString();
                    modifyfilename = guid + "." + fileExt;
                    //判断是否有该目录
                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(uploadfilepath);
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }
                    // 上传文件
                    inputfile.SaveAs(uploadfilepath + modifyfilename);
                    orifilename = modifyfilename;
                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return orifilename;
        }
    }
}