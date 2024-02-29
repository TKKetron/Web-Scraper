using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Scraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            path = path.Substring(0, path.IndexOf("bin"));
            path += "Output";
            List<Task> tasks = new List<Task>();

            /*tasks.Add(Task.Run(() => Get5EToolsInfo(path, "https://5e.tools/spells.html", "Spells")));
            Console.WriteLine("starting 5E spells");

            tasks.Add(Task.Run(() => Get5EToolsInfo(path, "https://5e.tools/backgrounds.html", "Backgrounds")));
            Console.WriteLine("starting 5E backgrounds");

            tasks.Add(Task.Run(() => Get5EToolsInfo(path, "https://5e.tools/feats.html", "Feats")));
            Console.WriteLine("starting 5E feats");

            tasks.Add(Task.Run(() => Get5EToolsInfo(path, "https://5e.tools/races.html", "Races")));
            Console.WriteLine("starting 5E Races");


            tasks.Add(Task.Run(() => GetWikiDotSpells(path)));
            Console.WriteLine("starting wikidot");*/

            tasks.Add(Task.Run(() => GetWikiDotClasses(path, "Fighter")));
            //tasks.Add(Task.Run(() => GetWikiDotClasses(path, "Barbarian")));


            await Task.WhenAll(tasks);
            // FixRaces(path);
            //FixSpells(path);
            Console.WriteLine("done");


        }
        static void GetWikiDotSpells(string path)
        {
            List<string> spellsURL = new List<string>();
            List<string> spellsHTML = new List<string>();


            var url = "http://dnd5e.wikidot.com/spells";

            WebClient client = new WebClient();

            string html = client.DownloadString(url);

            url = "http://dnd5e.wikidot.com/spell";

            do
            {
                html = html.Remove(0, html.IndexOf("/spell:"));
                spellsURL.Add(url + html.Substring(html.IndexOf("/spell:") + 6, html.IndexOf(">") - 7));
                html = html.Remove(0, html.IndexOf("\">"));

            } while (html.IndexOf("/spell:") != -1);

            foreach (string s in spellsURL)
                spellsHTML.Add(client.DownloadString(s));


            List<string> spellInfo = new List<string>();
            foreach (string spell in spellsHTML)
            {
                string[] result;
                string temp;
                temp = spell.Remove(0, spell.IndexOf("r\"><span>") + 9);
                spellInfo.Add(temp.Substring(0, temp.IndexOf("<")));
                temp = temp.Remove(0, temp.IndexOf("p>"));
                temp = temp.Substring(temp.IndexOf("p>") + 2, temp.IndexOf("<div class=\"content-separator\" style=\"display: none:\"></div>") - temp.IndexOf("p>") - 2);
                temp = RemoveParagraphBull(temp);
                result = temp.Split("\n");
                foreach (string line in result)
                    if (line != string.Empty)
                        if (line == "+&nbsp;Show&nbsp;HB&nbsp;Suggestion" || line == "-&nbsp;Hide&nbsp;HB&nbsp;Note" || line == "-&nbsp;Hide&nbsp;HB&nbsp;Suggestion")
                            continue;
                        else if (line.Contains("cantrip"))
                        {
                            spellInfo.Add("0");
                            spellInfo.Add(line.Substring(0, line.IndexOf("cantrip") - 1));
                        }
                        else if (line.Contains("-level"))
                        {
                            spellInfo.Add(line.Substring(0, 1));
                            spellInfo.Add(line.Substring(line.IndexOf(" ") + 1, line.Length - line.IndexOf(" ") - 1));
                        }
                        else
                            spellInfo.Add(line);
                spellInfo.Add("~");
            }

            string[] lines = spellInfo.ToArray();
            File.WriteAllLines(path + "SpellsWD.txt", lines);

            //int level, enum school, string castingTime, int range, bool v, bool s, bool m, string material, string suration, 
        }
        static void GetWikiDotClasses(string path, string name)
        {
            string url = "http://dnd5e.wikidot.com/" + name.ToLower();

            List<string> info = new List<string>();

            IWebDriver driver = new ChromeDriver(@"C:\dev\");
            driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 1, 0);
            driver.Url = url;

            string html = driver.FindElement(By.ClassName("feature")).Text;
            /*List<string> subClassesHTML = new List<string>();

            

            WebClient client = new WebClient();

            string html = client.DownloadString(url);


            html = RemoveParagraphBull(html);

            html = html.Substring(html.IndexOf("The " + name), html.Length - html.IndexOf("The " + name));
            html = html.Substring(0, html.IndexOf("\n\n\n\n\n\n\n\n\n"));

            string subclasses = html.Substring(html.IndexOf("grants you features"), html.LastIndexOf("\n\n") - html.IndexOf("grants you features"));
            subclasses = subclasses.Remove(0, subclasses.IndexOf("Source") + 9);

            List<string> subClassList = subclasses.Split('\n').ToList();
            int z = 0;
            do
            {
                int x = 0;
                for (z = 0; z < subClassList.Count; z++)
                {
                    if (subClassList[z] != "")
                        x++;
                    else
                        x = 0;
                    if (x != 2)
                    {
                        if (subClassList.Count == z + 1)
                        {
                            subClassList.RemoveAt(z);
                            x = 0;
                            break;

                        }
                        if (x == 1 && subClassList[z + 1] == "")
                        {
                            Console.WriteLine(subClassList[z]);
                            subClassList.RemoveAt(z);
                            x = 0;
                            break;
                        }
                        else if (x > 2)
                        {
                            subClassList.RemoveAt(z - 2);
                            subClassList.RemoveAt(z - 1);
                            subClassList.RemoveAt(z);
                            x = 0;
                            break;
                        }
                    }

                }
            }
            while (z != subClassList.Count);

            subClassList.RemoveAll(IsBlank);
            for (int i = 1; i < subClassList.Count; i += 2)
                subClassList[i] = "";
            subClassList.RemoveAll(IsBlank);
            for (int i = 0; i < subClassList.Count; i++)
            {
                subClassList[i] = subClassList[i].ToLower().Trim();
                subClassList[i] = subClassList[i].Replace(' ', '-');
            }

            foreach (string s in subClassList)
                try
                {
                    subClassesHTML.Add(client.DownloadString(url + ":" + s));
                }
                catch (Exception e)
                {
                    try
                    {
                        subClassesHTML.Add(client.DownloadString(url + ":" + s + "-ua"));
                    }
                    catch (Exception b)
                    {
                        subClassesHTML.Add(client.DownloadString("http://dnd5e.wikidot.com/barbarian:path-of-the-depths-pc"));

                    }

                }
            for (int i = 0; i < subClassesHTML.Count; i++)
            {
                subClassesHTML[i] = RemoveParagraphBull(subClassesHTML[i]);
                subClassesHTML[i] = subClassesHTML[i].Remove(0, subClassesHTML[i].IndexOf("\"bottom-right") + 42);
                subClassesHTML[i] = subClassesHTML[i].Substring(0, subClassesHTML[i].LastIndexOf("\n\n\n\n\n\n\n\n\n"));
                subClassesHTML[i] = subClassList[i] + "\n" + subClassesHTML[i] + "\n~";
            }*/


            File.WriteAllText(path + name + ".txt", html);

        }


        static void Get5EToolsInfo(string path, string url, string name)
        {
            List<string> info = new List<string>();

            IWebDriver driver = new ChromeDriver(@"C:\dev\");
            driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 1, 0);
            driver.Url = url;

            driver.FindElement(By.CssSelector("button[class='btn btn-default ']")).Click();//opens filer
            driver.FindElement(By.CssSelector("button[class='btn btn-default btn-xs fltr__h-btn--all w-100']")).Click();//selects all content
            driver.FindElement(By.CssSelector("button[class='btn btn-xs btn-primary']")).Click();//confirm

            ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.CssSelector("div[class='lst__row flex-col ']")); //finds each spell element

            info.Add(driver.FindElement(By.CssSelector("span[class='stats-name copyable']")).Text);//gets the name
            ReadOnlyCollection<IWebElement> elementInfo = driver.FindElements(By.CssSelector("td[colspan='6']"));//gets all other info
            foreach (IWebElement e in elementInfo)
                info.Add(e.Text);//adds info to list
            info.Add("~");

            foreach (IWebElement e in elements)
            {
                int x = 0;
                e.Click();
                //string checker = "Antimagic Field";
                string spellName = driver.FindElement(By.CssSelector("span[class='stats-name copyable']")).Text;
                /*if (spellName != checker)//fins a specific spell
                    continue;*/
                info.Add(spellName);//gets the name

                elementInfo = driver.FindElements(By.CssSelector("td[colspan='6']"));//gets all other info
                foreach (IWebElement i in elementInfo)
                {
                    driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
                    if (x != 6)
                    {
                        info.Add(i.Text);
                        x++;
                        continue;
                    }
                    List<string> searches = new List<string>();
                    searches.Add("span[class='entry-title-inner']");
                    searches.Add("table");
                    searches.Add("p");
                    ReadOnlyCollection<IWebElement> stuff = MultiSearch(i, searches);
                    foreach (IWebElement j in stuff)
                    {
                        ReadOnlyCollection<IWebElement> links = j.FindElements(By.CssSelector("a"));
                        foreach (IWebElement k in links)
                            if (k.GetAttribute("href").Contains("spells.html"))
                                info.Add("{SPELL}\n" + k.Text + "\n{/SPELL}");
                            else
                                info.Add("{TIP}\n" + k.Text + "\n{/TIP}");


                        switch (j.TagName)
                        {
                            case "span":
                                info.Add("{BOLD}\n" + j.Text + "\n{/BOLD}");
                                break;
                            case "table":
                                PrintTable(j, ref info);
                                break;
                            default:
                                info.Add(j.Text);
                                break;
                        }
                    }
                    x++;
                }
                /* if (spellName == checker)//fins a specific spell
                     break; ;*/
                info.Add("~");
            }

            string[] lines = info.ToArray();
            File.WriteAllLines(path + name + "5E.txt", lines);
            driver.Close();
        }

        private static ReadOnlyCollection<IWebElement> MultiSearch(IWebElement i, List<string> cssSelectors)
        {
            List<IWebElement> elements = new List<IWebElement>();
            foreach (string s in cssSelectors)
            {
                ReadOnlyCollection<IWebElement> temp = i.FindElements(By.CssSelector(s));
                foreach (IWebElement j in temp)
                    elements.Add(j);
            }
            elements = elements.OrderBy(o => o.Location.Y).ToList();
            return new ReadOnlyCollection<IWebElement>(elements);
        }
        public static void PrintTable(IWebElement table, ref List<string> info)
        {
            info.Add("{TABLE}\n");
            try
            {
                info.Add(table.FindElement(By.CssSelector("caption")).Text); //gets table title if there is one
            }
            catch (Exception error)
            {
                info.Add("{NO TITLE}");
            }
            info.Add("\n\n");
            ReadOnlyCollection<IWebElement> tableRows = table.FindElements(By.CssSelector("tr")); // gets each row
            bool head = true;
            foreach (IWebElement k in tableRows)
            {
                ReadOnlyCollection<IWebElement> tableItem;
                if (head)
                {
                    tableItem = k.FindElements(By.CssSelector("th"));// gets the header row
                    head = false;
                }
                else
                    tableItem = k.FindElements(By.CssSelector("td"));// gets every other row
                foreach (IWebElement l in tableItem)
                    info.Add(l.Text);
                info.Add("\n");
            }
            info.Add("{/TABLE}");
        }

        public static void FixRaces(string path)
        {
            string[] races = File.ReadAllText(path + "Races5E.txt").Split('~');
            List<string> update = new List<string>();
            foreach (string e in races)
            {
                string[] fakeLines = e.Split('\n');
                List<string> lines = fakeLines.ToList();
                lines.Remove("\r");
                lines.RemoveAll(IsBlank);
                string source = lines[lines.Count() - 1].Substring(8, lines[lines.Count() - 1].IndexOf(", ") - 1);
                if (lines.Last().Contains("UA"))
                    lines[0] = lines[0] + "(" + source + ")";
                for (int i = 0; i < lines.Count(); i++)
                    update.Add(lines[i]);
            }
            races = update.ToArray();
            File.WriteAllLines(path + "Races5E.txt", races);

        }

        public static void FixSpells(string path)
        {
            string temp = "temp";
            string[] spells = File.ReadAllText(path + "Spells5E.txt").Split('~');
            List<List<string>> spell = new List<List<string>>();
            foreach (string s in spells)
                spell.Add(s.Split("\r\n").ToList());
            for (int i = 0; i < spell.Count-1; i++)
            {
                if(i!=0)
                    spell[i].RemoveAt(0);
                spell[i].RemoveAt(spell[i].Count - 1);
                if (i > 0)
                {
                    if (spell[i][0] == spell[i - 1][0])
                    {
                        spell[i][0] = spell[i][0] + " " + spell[i][spell[i].Count - 1].Substring(8, spell[i][spell[i].Count() - 1].IndexOf(", ") - 8);
                        temp = spell[i-1][0];
                    }
                    else if(spell[i][0] == temp)
                        spell[i][0] = spell[i][0] + " " + spell[i][spell[i].Count - 1].Substring(8, spell[i][spell[i].Count() - 1].IndexOf(", ") - 8);

                }
            }
            List<string> info = new List<string>();
            foreach (List<string> s in spell)
            {
                info.AddRange(s);
                info.Add("~");
            }
            for(int i = 0; i < info.Count; i++)
            {
                //while(info[i].Contains("\r"))
                  //  info[i] = info[i].Remove(info[i].IndexOf("\r"), 1);
            }
            File.WriteAllLines(path + "Spells5E.txt", info.ToArray());

        }



        public static string RemoveParagraphBull(string bs)
        {
            string temp = bs;
            while (bs.Contains("–&nbsp;"))
            {
                temp = bs.Substring(0, bs.IndexOf("–&nbsp;")) + "- ";
                temp += bs.Substring(bs.IndexOf("–&nbsp;") + 7, bs.Length - bs.IndexOf("–&nbsp;") - 7);
                bs = temp;
            }
            while (bs.Contains(";&nbsp;"))
            {
                temp = bs.Substring(0, bs.IndexOf(";&nbsp;")) + "; ";
                temp += bs.Substring(bs.IndexOf(";&nbsp;") + 7, bs.Length - bs.IndexOf(";&nbsp;") - 7);
                bs = temp;
            }
            while (bs.Contains("&quot;"))
            {
                temp = bs.Substring(0, bs.IndexOf("&quot;")) + "\"";
                temp += bs.Substring(bs.IndexOf("&quot;") + 6, bs.Length - bs.IndexOf("&quot;") - 6);
                bs = temp;
            }
            while (bs.Contains("&amp;"))
            {
                temp = bs.Substring(0, bs.IndexOf("&amp;")) + "&";
                temp += bs.Substring(bs.IndexOf("&amp;") + 5, bs.Length - bs.IndexOf("&amp;") - 5);
                bs = temp;
            }
            while (bs.Contains("&#8212;"))
            {
                temp = bs.Substring(0, bs.IndexOf("&#8212;")) + "—";
                temp += bs.Substring(bs.IndexOf("&#8212;") + 7, bs.Length - bs.IndexOf("&#8212;") - 7);
                bs = temp;
            }
            while (bs.Contains("<"))
            {
                if (bs.Substring(bs.IndexOf("<"), 7).Contains("/table"))
                {
                    temp = bs.Substring(0, bs.IndexOf("<") - 2) + "\n\n\n{/TABLE}";
                    temp += bs.Substring(bs.IndexOf("<"), bs.Length - bs.IndexOf("<"));
                    bs = temp;
                }
                else if (bs.Substring(bs.IndexOf("<"), 6).Contains("table"))
                {
                    temp = bs.Substring(0, bs.IndexOf("<") - 1) + "\n{TABLE}";
                    temp += bs.Substring(bs.IndexOf("<"), bs.Length - bs.IndexOf("<"));
                    bs = temp;
                }
                temp = bs.Substring(0, bs.IndexOf("<"));
                temp += bs.Substring(bs.IndexOf(">") + 1, bs.Length - bs.IndexOf(">") - 1);
                bs = temp;
            }
            return bs;
        }
        private static bool IsBlank(string s)
        {
            if (s.Equals("") || s.Equals("\r") || s.Equals("\n"))
                return true;
            else
                return false;
        }
    }
}