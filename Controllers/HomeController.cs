using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Xml.XPath;

namespace FeederRSS.Controllers
{
    public class ChannelClass
    {
        public string title;
        public string description;
        public string link;
        public List<Items> items;

        public ChannelClass()
        {
            title = "";
            description = "";
            link = "";
            items = new List<Items>();
        }
    }

    public class Items
    {
        public string title;
        public string description;
        public string pubDate;

        public Items()
        {
            title = "";
            description = "";
            pubDate = "";
        }
    }

    public class HomeController : Controller
    {
        string generateHtml(List<ChannelClass> channels)
        {
            string message = "";

            foreach (ChannelClass channel in channels)
            {
                message += "<hr />";
                message += $"<h1>{channel.title}<br></h1>";
                message += "<br>";
                foreach (Items article in channel.items)
                {
                    message += "<br>";
                    message += "<hr />";
                    message += "<br>";
                    message += $"<h2>{article.title}<br></h2>";
                    message += $"<h3>({article.pubDate})<br></h3>";
                    message += "<br>";
                    message += $"{article.description}<br>";
                    message += "<br>";
                }
                message += "<hr />";
                message += "<br>";
            }
            return message;
        }

        public void AddLink(string link)
        {
            XmlReader reader = XmlReader.Create(link);
            XmlDocument sett = new XmlDocument();
            sett.Load("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
            XPathNavigator nav = sett.CreateNavigator();
            nav.MoveToFirstChild();
            using (XmlWriter writer = nav.AppendChild())
            {
                writer.WriteStartElement("channel");
                writer.WriteStartElement("link");
                writer.WriteString(link);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            sett.Save("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
        }

        public void DeleteLink(string link)
        {
            XmlDocument sett = new XmlDocument();
            sett.Load("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
            XmlNode node = sett.SelectSingleNode($"/setting/channel[link='{link}']");
            node.ParentNode.RemoveChild(node);
            sett.Save("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
        }

        public void SettingsLoad(out List<string> urlList, out int freq, out string isFormed)
        {
            XmlDocument sett = new XmlDocument();
            sett.Load("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
            XmlNodeList elemList = sett.GetElementsByTagName("link");
            urlList = new List<string>();
            foreach (XmlElement element in elemList)
            {
                urlList.Add(element.InnerXml);
            }
            isFormed = sett.GetElementsByTagName("isFormed")[0].InnerXml;
            freq = Convert.ToInt32(sett.GetElementsByTagName("freq")[0].InnerXml);
        }

        public void SettingsLoad(out List<string> urlList)
        {
            XmlDocument sett = new XmlDocument();
            sett.Load("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
            XmlNodeList elemList = sett.GetElementsByTagName("link");
            urlList = new List<string>();
            foreach (XmlElement element in elemList)
            {
                urlList.Add(element.InnerXml);
            }
        }

        public void SettingsLoad(out string isFormed)
        {
            XmlDocument sett = new XmlDocument();
            sett.Load("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
            isFormed = sett.GetElementsByTagName("isFormed")[0].InnerXml;
        }

        public ActionResult Index()
        {
            List<string> urlList = new List<string>();
            int freq;
            string isFormed;
            SettingsLoad(out urlList, out freq, out isFormed);

            ViewBag.Sec = freq;

            ViewBag.Da = "";
            List<ChannelClass> channels = new List<ChannelClass>();
            foreach (string url in urlList)
            {    
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();
                ChannelClass channel = new ChannelClass();
                channel.title = feed.Title.Text;
                channel.description = feed.Description.Text;
                channel.link = url;

                foreach (SyndicationItem chanel_item in feed.Items)
                {
                    Items item = new Items();
                    item.title = chanel_item.Title.Text;
                    item.pubDate = chanel_item.PublishDate.ToString();
                    item.description = chanel_item.Summary.Text;
                    channel.items.Add(item);
                }

                channels.Add(channel);
            }
            if (isFormed == "formed")
            {
                ViewBag.formed = generateHtml(channels);
                ViewBag.notFormed = "";
            }
            else
            {
                ViewBag.notFormed = generateHtml(channels);
                ViewBag.formed = "";
            }
            return View();
        }
        
        public void ChangeFreq(string freq)
        {
            XmlDocument sett = new XmlDocument();
            sett.Load("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
            XmlNode node = sett.SelectSingleNode($"/setting/freq");
            node.ParentNode.RemoveChild(node);
            XPathNavigator nav = sett.CreateNavigator();
            nav.MoveToFirstChild();
            using (XmlWriter writer = nav.AppendChild())
            {
                writer.WriteStartElement("freq");
                writer.WriteString(freq);
                writer.WriteEndElement();
            }
            sett.Save("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
        }

        public void ChangeIsFormed()
        {
            XmlDocument sett = new XmlDocument();
            sett.Load("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
            string isFormed = sett.GetElementsByTagName("isFormed")[0].InnerXml;
            XmlNode node = sett.SelectSingleNode($"/setting/isFormed");
            node.ParentNode.RemoveChild(node);
            XPathNavigator nav = sett.CreateNavigator();
            nav.MoveToFirstChild();
            using (XmlWriter writer = nav.AppendChild())
            {
                writer.WriteStartElement("isFormed");
                if (isFormed == "formed")
                {
                    writer.WriteString("notFormed");
                }
                else
                {
                    writer.WriteString("formed");
                }
                writer.WriteEndElement();
            }
            sett.Save("E:/User/Desktop/parser/FeederRSS/FeederRSS/App_Data/Settings.xml");
        }

        public ActionResult Settings()
        {
            ViewBag.Error = "";
            List<string> urlList = new List<string>();
            int freq = 100;
            string isFormed = "formed";
            try
            {
                SettingsLoad(out urlList, out freq, out isFormed);
            }
            catch
            {
                ViewBag.Error += "Ошибка чтения: Невозможно прочитать файл настроек. <br>";
            }
            ViewBag.Links = "";
            foreach (string link in urlList)
            {
                ViewBag.Links += $"удалить <input type='checkbox' name='{link}'> {link}<br>";
            }
            ViewBag.freq = freq;
            if (isFormed == "formed")
            {
                ViewBag.isFormed = "checked='checked'";
                ViewBag.isNotFormed = "";
            }
            else
            {
                ViewBag.isNotFormed = "checked='checked'";
                ViewBag.isFormed = "";
            }
            return View();
        }

        [HttpPost]
        public ActionResult Settings(string freq, string linkBox)
        {
            ViewBag.Error = "";
            if (Request.Form["add"] != null)
            {
                if (linkBox != "")
                {
                    try
                    {
                        AddLink(linkBox);
                    }
                    catch (Exception)
                    {
                        ViewBag.Error += "Ошибка ввода ленты: Неверная ссылка. <br>";
                    }
                }
            }
            List<string> urlList = new List<string>();
            int oldfreq = 100;
            string isFormed = "formed";
            try
            {
                SettingsLoad(out urlList, out oldfreq, out isFormed);
            }
            catch
            {
                ViewBag.Error += "Ошибка чтения: Невозможно прочитать файл настроек. <br>";
            }
            if (Request.Form["delete"] != null)
            {
                foreach (string link in urlList)
                {
                    if (Request.Form[$"{link}"] == "on")
                    {
                        try
                        {
                            DeleteLink(link);
                        }
                        catch
                        {
                            ViewBag.Error += "Ошибка удаления. <br>";
                        }
                    }
                }
                SettingsLoad(out urlList);
            }
            ViewBag.freq = oldfreq;
            if (Request.Form["update"] != null)
            {
                if(freq != "")
                {
                    int Num;
                    bool isNum = int.TryParse(freq, out Num);
                    if (isNum)
                    {
                        try
                        {
                            ViewBag.freq = freq;
                            ChangeFreq(freq);
                        }
                        catch
                        {
                            ViewBag.Error += "Ошибка изменения частоты обновления. <br>";
                        }
                    }
                    else
                    {
                        ViewBag.Error += "Ошибка частоты обновления: Введите число. <br>";
                    }                   
                }
                
                if (Request.Form["isFormed"] != isFormed)
                {
                    try
                    {
                        ChangeIsFormed();
                        SettingsLoad(out string isFormedNow);
                        isFormed = isFormedNow;
                    }
                    catch
                    {
                        ViewBag.Error += "Ошибка смены форматирования. <br>";
                    }
                }

            }
            if(isFormed == "formed")
            {
                ViewBag.isFormed = "checked='checked'";
                ViewBag.isNotFormed = "";
            }
            else
            {
                ViewBag.isNotFormed = "checked='checked'";
                ViewBag.isFormed = "";
            }
            ViewBag.Links = "";
            foreach (string link in urlList)
            {
                ViewBag.Links += $"удалить <input type='checkbox' name='{link}'> {link}<br>";
            }
            return View();
        }
    }
}