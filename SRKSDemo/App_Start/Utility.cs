using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using context = System.Web.HttpContext;

namespace SRKSDemo.App_Start
{
    public class Utility
    {
        public string GUIDGenerator()
        {
            return System.Guid.NewGuid().ToString();

        }
        public bool SaveImage(HttpPostedFileBase ImageFile, string renamedfile)
        {
            string path = "";
            try
            {
                string filepath = context.Current.Server.MapPath("~/AndonImages/");  

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                path = Path.Combine(context.Current.Server.MapPath("~/AndonImages/"), renamedfile);
                ImageFile.SaveAs(path);
            }
            catch (Exception e)
            {

            }
            return true;
        }

        public void deleteOldImage(string imageName)
        {
            try
            {
                var filePath = context.Current.Server.MapPath("~/AndonImages/" + imageName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception e)
            {

            }
        }

        public TimeSpan timeConerterToCCSSETime(TimeSpan time)
        {
            TimeSpan tSpan = DateTime.Now.TimeOfDay;

            string sTime = time.ToString();
            string[] arry = sTime.Split(':');
            string hr = arry[0];
            string min = arry[1];
            string sec = arry[2];
            try
            {
                int hour = Convert.ToInt32(hr);
                switch (hour)
                {
                    case 7:
                        hour = 0;
                        break;
                    case 8:
                        hour = 1;
                        break;
                    case 9:
                        hour = 2;
                        break;
                    case 10:
                        hour = 3;
                        break;
                    case 11:
                        hour = 4;
                        break;
                    case 12:
                        hour = 5;
                        break;
                    case 13:
                        hour = 6;
                        break;
                    case 14:
                        hour = 7;
                        break;
                    case 15:
                        hour = 8;
                        break;
                    case 16:
                        hour = 9;
                        break;
                    case 17:
                        hour = 10;
                        break;
                    case 18:
                        hour = 11;
                        break;
                    case 19:
                        hour = 12;
                        break;
                    case 20:
                        hour = 13;
                        break;
                    case 21:
                        hour = 14;
                        break;
                    case 22:
                        hour = 15;
                        break;
                    case 23:
                        hour = 16;
                        break;
                    case 0:
                        hour = 17;
                        break;
                    case 1:
                        hour = 18;
                        break;
                    case 2:
                        hour = 19;
                        break;
                    case 3:
                        hour = 20;
                        break;
                    case 4:
                        hour = 21;
                        break;
                    case 5:
                        hour = 22;
                        break;
                    case 6:
                        hour = 23;
                        break;
                    default:
                        hour = 0;
                        break;
                }

                string newTime = hour + ":" + min + ":" + sec;
                tSpan = TimeSpan.Parse(newTime);
            }
            catch (Exception e)
            {

            }
            return tSpan;
        }
    }
}