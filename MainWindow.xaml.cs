using HtmlAgilityPack;
using RecipeScraper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Cloud.Firestore;
using System.IO;
using System.Threading;

namespace RecipeScraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Recepi recepi = new Recepi();
        CollectionReference Collection = dbSetup();
        public static Recepi gethtml(string id)
        {
            string url = "https://www.allrecipes.com/recipe/"+id+"/";
            Recepi recepi = new Recepi();
            recepi.Id = id;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmldoc = new HtmlDocument();

            //REQUIREMENT EXCEPTION HANDLING
            //check if the page exists >> error 404
            try
            {
                htmldoc = web.Load(url);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
            //checks if the path is viable
            if (htmldoc.DocumentNode.SelectNodes("//section[contains(@ class,'error-page')]") != null)
            {
                return (null);
            }
            else
            {
                //recipe name check and validation
                try
                {
                    var name = htmldoc.DocumentNode.SelectNodes("//div[contains(@class, 'intro')]");
                    recepi.Name = name[0].InnerText.Trim();
                }
                catch (Exception)
                {
                    Console.WriteLine("no name");
                    recepi.Name = id;
                }
                //recipe picture check and validation
                try
                {
                    var pic = htmldoc.DocumentNode.SelectNodes("//div[contains(@class, 'image-container')]//div//img");
                    recepi.Pic = pic[0].GetAttributeValue("src", "");
                }
                catch (Exception)
                {
                    Console.WriteLine("no pic");
                    recepi.Pic = "Pic unavailable";
                }

                // isolate spicific ingredients validate and fill >> used for diet validation and querry
                try
                {
                    var Ingredients = htmldoc.DocumentNode.SelectNodes("//li[contains(@class, 'ingredients-item')]//input").ToList();
                    foreach (var e in Ingredients)
                    {
                        if (e.GetAttributeValue("value", "").Length >= e.GetAttributeValue("data-ingredient", "").Length)
                        {
                            recepi.addIngreds(e.GetAttributeValue("data-ingredient", "").Trim());
                        }
                        if (e.GetAttributeValue("value", "").Length < e.GetAttributeValue("data-ingredient", "").Length)
                        {
                            recepi.addIngreds(e.GetAttributeValue("value", "").Trim());
                        }

                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("no Ingredients");
                }

                //  populate the (Full-Ingredients) array >> what will be displayed 
                try
                {
                    var s = htmldoc.DocumentNode.SelectNodes("//span[contains(@class, 'ingredients-item-name')]").ToList();
                    foreach (var e in s)
                    {
                        recepi.addFingredient(e.InnerText);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("no Ingredients");
                }

                // directions 
                try
                {
                    var discription = htmldoc.DocumentNode.SelectNodes("//div[contains(@class, 'section-body')]//p").ToList();
                    foreach (var e in discription)
                    {
                        recepi.adddirections(e.InnerText.Trim());
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("no discription");
                }

                return (recepi);

            }
        }

        //database connection logic
        public static CollectionReference dbSetup()
        {
            //database authentication and connection
            string path = AppDomain.CurrentDomain.BaseDirectory + @"spaghettio-auth.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            string projectId = "spaghettio";
            FirestoreDb db = FirestoreDb.Create(projectId);
            CollectionReference collection = db.Collection("recipe");
            return collection;
        }

        //updates the database with a new recipe
        public void writeToDB()
        {
            //MessageBox.Show("thread started");
            Recepi rec = recepi;

            this.Dispatcher.Invoke(() => { rec.Fingredients = convertStrToList(Ingreds.Text.Trim()); });
            this.Dispatcher.Invoke(() => { rec.Directions = convertStrToList(Instruction.Text.Trim()); });

            if (rec != null)
            {
                try
                {
                    var result1 = Collection.Document(rec.Id).SetAsync(rec).GetAwaiter().GetResult();
                    MessageBox.Show(rec.getjson() + "\n\n was added!");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        //pre database serializing to file was an option
        public static void saveToFile(Recepi rec)
        {
            File.AppendAllText("recepies.json", rec.getjson());
        }

        // checks if the string is made of only numbers >> ID string validation
        bool isDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        //REQUIREMENT LOOPS
        // converts List<string> to string.>> to dispaly
        public static string convertListToStr(List<string> list)
        {
            string str = "";
            for(int i = 0; i < list.Count; i++)
            {
                str = str + list[i] + "\n";
            }

            return str;
        }

        //REQUIREMENT LINQ(FOREACH LOOP) AND Strings, Array or Lists
        // converts a string into a list of strings spliting on '\n'
        public static List<string> convertStrToList (string str) 
        {
            string[] strArr = str.Split('\n');
            List<string> ls = new List<string>();
            foreach(string s in strArr)
            {
                ls.Add(s);
            }
            return (ls);
        }
        //image scorce minipulation
        public static BitmapImage imageSorce (string url)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.EndInit();

                return (bitmap);
            }
            catch(Exception e)
            {
                MessageBox.Show("No Picture Available");
                return null;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            CollectionReference collection = dbSetup();
            
        }

        //REQUIREMENT THREADING
        //connects to database and submits an entry.
        private void Submit(object sender, RoutedEventArgs e)
        {
            //start new thread to exectute (writeToDB)
            
            Thread t = new Thread(new ThreadStart(writeToDB));
            t.Start();
        }

        //populates the page.
        private void Search (object sender, RoutedEventArgs e)
        {
            if(ID.Text.Length != 5 || !isDigitsOnly(ID.Text)){
                MessageBox.Show("ID must be (5) numbers ");
                ID.Text = "ID";
            }
            else
            {
                
                recepi = gethtml(ID.Text);
                if (recepi == null)
                {
                    MessageBox.Show("404 No Recipe Found");
                }
                else
                {
                    //fill the ingreds box
                    Ingreds.Text = convertListToStr(recepi.Fingredients);

                    //fill the instruciton box
                    Instruction.Text = convertListToStr(recepi.Directions);
                    //fill the image >> NOT WORKING
                    Img.Source = imageSorce(recepi.Pic);
                }

            }
        }
    }
}
