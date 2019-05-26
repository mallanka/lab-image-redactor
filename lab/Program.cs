using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace lab
{
    class Program
    {

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("чего тебе надо? жми цифру" +
                                  "\n1.переименовать изображения по времени создания" +
                                  "\n2.добавить на изображение время создания" +
                                  "\n3.сортировать изображения по годам их создания" +
                                  "\n4.сортировать по местам создания");
                Byte.TryParse(Console.ReadLine(), out byte num);
                Console.WriteLine("укажи путь");
                string path = Console.ReadLine();
                switch (num)
                {
                    case 1:
                        Directory.CreateDirectory(path + "Rename");                         //создаю новую папку
                        try
                        {
                            string[] filesInFolder = Directory.GetFiles(path);              //беру список файлов
                            foreach (string x in filesInFolder)                             //пробегаю по каждому файлу  
                            {
                                Image i = Image.FromFile(x);                                //создаю объект каждого файла
                                if (i.PropertyItems.Any(p => p.Id == 0x0132))               //если есть свойство "дата съемки"
                                {
                                    PropertyItem pi = i.GetPropertyItem(0x0132);            //беру свойство "дата съемки"
                                    string dateText = Encoding.ASCII.GetString(pi.Value);   //конвертирую значние в стрингу
                                    dateText = dateText.Replace(":", "-").Replace(".", "-");    //заменяю недопустимые символы для пути
                                    FileInfo pic = new FileInfo(x);
                                    string newPath = x.Insert(path.Length + 1, dateText).Insert(path.Length, "Rename");
                                    pic.CopyTo(newPath);                                    //копирую файл с новым именем в новую папку
                                }
                                else                                                        //если нет свойства "дата съемки"
                                {
                                    FileInfo pic = new FileInfo(x);
                                    string newPath = x.Insert(path.Length, "Rename").
                                                       Insert(path.Length + 7, pic.CreationTime.ToString().
                                                       Replace(":", "-").Replace(".", "-") + " ");
                                    pic.CopyTo(newPath);
                                }
                            }
                            Console.WriteLine("Сделано, проверяй");
                            break;
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            Console.WriteLine("путь не существует, давай всё заново");
                            break;
                        }
                    case 2:
                        Directory.CreateDirectory(path + "DateMark");
                        try
                        {
                            string[] filesInFolder = Directory.GetFiles(path);
                            foreach (string x in filesInFolder)
                            {
                                Image i = Image.FromFile(x);
                                if (i.PropertyItems.Any(p => p.Id == 0x0132))
                                {
                                    PropertyItem pi = i.GetPropertyItem(0x0132);
                                    string dateText = Encoding.ASCII.GetString(pi.Value);
                                    Graphics graphics = Graphics.FromImage(i);
                                    graphics.DrawString(dateText,
                                    new System.Drawing.Font("Arial", 14, FontStyle.Bold),
                                    new SolidBrush(Color.Red), new Rectangle(i.Width - i.Width / 30, 
                                                                             i.Height - i.Height / 30, 
                                                                             i.Width / 30, 
                                                                             i.Height / 30),
                                    new StringFormat(StringFormatFlags.NoWrap));
                                    i.Save(x.Insert(path.Length, "DateMark"));
                                }
                                else
                                {
                                    FileInfo pic = new FileInfo(x);
                                    string dateText = pic.CreationTime.ToString();
                                    Graphics graphics = Graphics.FromImage(i);
                                    graphics.DrawString(dateText,
                                    new System.Drawing.Font("Arial", 14, FontStyle.Bold),
                                    new SolidBrush(Color.Red), new Rectangle(i.Width - i.Width / 30,
                                                                             i.Height - i.Height / 30,
                                                                             i.Width / 30,
                                                                             i.Height / 30),
                                    new StringFormat(StringFormatFlags.NoWrap));
                                    i.Save(x.Insert(path.Length, "DateMark"));
                                }
                            }
                            Console.WriteLine("Сделано, проверяй");
                            break;
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            Console.WriteLine("путь не существует, давай всё заново");
                            break;
                        }

                    case 3:
                        Directory.CreateDirectory(path + "YearSort");
                        try
                        {
                            string[] filesInFolder = Directory.GetFiles(path);
                            foreach (string x in filesInFolder)
                            {
                                Image i = Image.FromFile(x);
                                if (i.PropertyItems.Any(p => p.Id == 0x0132))
                                {
                                    PropertyItem pi = i.GetPropertyItem(0x0132);
                                    string dateText = Encoding.ASCII.GetString(pi.Value);
                                    dateText = dateText.Remove(4, dateText.Length - 4);
                                    FileInfo pic = new FileInfo(x);
                                    Directory.CreateDirectory(path + "YearSort\\" + dateText);
                                    pic.CopyTo(x.Insert(path.Length, "YearSort\\" + dateText));
                                }
                                else
                                {
                                    FileInfo pic = new FileInfo(x);
                                    Directory.CreateDirectory(path+ "YearSort\\"+ pic.CreationTime.Year+"\\");
                                    pic.CopyTo(x.Insert(path.Length, "YearSort\\" + pic.CreationTime.Year));
                                }
                            }
                            Console.WriteLine("Сделано, проверяй");
                            break;
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            Console.WriteLine("путь не существует, давай всё заново");
                            break;
                        }
                    case 4:
                        Directory.CreateDirectory(path + "SiteSort");
                        try
                        {
                            string[] filesInFolder = Directory.GetFiles(path);
                            foreach (string x in filesInFolder)
                            {
                                Image i = Image.FromFile(x);
                                if (i.PropertyItems.Any(p => p.Id == 0x0002) && i.PropertyItems.Any(p => p.Id == 0x0004))
                                {
                                    PropertyItem pi = i.GetPropertyItem(0x0002);
                                    string latit = Encoding.ASCII.GetString(pi.Value);
                                    pi = i.GetPropertyItem(0x0004);
                                    string longit = Encoding.ASCII.GetString(pi.Value);
                                    string site;
                                    WebRequest webRequest = WebRequest.Create($"https://geocode-maps.yandex.ru/1.x/?geocode={longit},{latit}");
                                    WebResponse webResponse = webRequest.GetResponse();
                                    using (Stream stream = webResponse.GetResponseStream())
                                    {
                                        using (StreamReader sr = new StreamReader(stream))
                                        {
                                            XmlReader xmlReader = XmlReader.Create(stream);
                                            XDocument xDocument = XDocument.Load(xmlReader);
                                            XElement xElement = xDocument.Root;
                                            string sityPic = xElement.ToString();
                                            sityPic = sityPic.Remove(0, sityPic.IndexOf("LocalityName") + 13);
                                            sityPic = sityPic.Remove(sityPic.IndexOf("<"));                     //вычленил название города
                                            string countryPic = xElement.ToString();
                                            countryPic = countryPic.Remove(0, countryPic.IndexOf("CountryName>") + 12);
                                            countryPic = countryPic.Remove(countryPic.IndexOf("<"));            //вычленил название страны
                                            site = countryPic + ", " + sityPic;
                                        }
                                        webResponse.Close();
                                    }

                                    Directory.CreateDirectory(path + "SiteSort\\" + site);
                                    FileInfo pic = new FileInfo(x);
                                    pic.CopyTo(x.Insert(path.Length, "SiteSort\\").Insert(path.Length + 9, site));
                                }
                            }
                            Console.WriteLine("Сделано, проверяй");
                            break;
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            throw ex;
                            Console.WriteLine("путь не существует, давай всё заново");
                            break;
                        }
                    default:
                        Console.WriteLine("ты дурак? тебе же ясно сказали - ЦИФРУ одну из четырех! а ты что нажал?");
                        break;
                }
                Console.ReadLine();
                Console.Clear();
            }
        }
    }
}
