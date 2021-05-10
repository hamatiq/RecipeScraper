using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Net.Http;
using Google.Cloud.Firestore;
using System.Diagnostics;


//REQUIREMENT CLASSES
namespace RecipeScraper.Model
{
    [FirestoreData]
    public class Recepi
    {
        private string id;
        [FirestoreProperty]
        public string Id
        {
            set { id = value; }
            get { return id; }
        }
        private string name;
        [FirestoreProperty]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string pic;
        [FirestoreProperty]
        public string Pic
        {
            get { return pic; }
            set { pic = value; }
        }
        private string diet;
        [FirestoreProperty]
        public string Diet
        {
            get { return diet; }
        }
        private List<string> fingredients;
        [FirestoreProperty]
        public List<string> Fingredients
        {
            set { fingredients = value; }
            get { return fingredients; }
        }
        private List<string> ingredients;
        [FirestoreProperty]
        public List<string> Ingredients
        {
            get { return ingredients; }
        }
        private List<string> directions;
        [FirestoreProperty]
        public List<string> Directions
        {
            set { directions = value; }
            get { return directions; }
        }


        //constructors
        public Recepi(string id, string name, string pic, List<string> ingredients, List<string> fingredients, List<string> directions)
        {
            this.id = id;
            this.name = name;
            this.pic = pic;
            this.ingredients = ingredients;
            this.fingredients = fingredients;
            this.directions = directions;

        }
        public Recepi()
        {
            this.id = "";
            this.name = "";
            this.pic = "";
            this.diet = "vegan";
            this.ingredients = new List<string>();
            this.fingredients = new List<string>();
            this.directions = new List<string>();

        }

        //REQUIREMENT METHODS
        // add bulk ingredients
        public void addFingredient(string str)
        {
            this.fingredients.Add(str);
        }

        // add bulk directions
        public void adddirections(string str)
        {
            this.directions.Add(str);
        }

        //add isolated ingredients
        //and updates the diet field
        public void addIngreds(string ingreds)
        {
            this.ingredients.Add(ingreds);
            if ((diet == "vegan") && (ingreds.Contains("cheese") || ingreds.Contains("milk") || ingreds.Contains("butter")))
            {
                diet = "vegetarian";
            }
            else if ((diet == "vegan" || diet == "vegetarian") && (ingreds.Contains("beef") || ingreds.Contains("pork") || ingreds.Contains("chicken") || ingreds.Contains("fish")))
            {
                diet = "";
            }
        }

        // assesses the diettype
        private void findDiet()
        {
            diet = "vegan";
            foreach (var i in ingredients)
            {
                if ((diet == "vegan") && (i.Contains("cheese") || i.Contains("milk") || i.Contains("butter")))
                {
                    diet = "vegetarian";
                }
                else if ((diet == "vegan" || diet == "vegetarian") && (i.Contains("beef") || i.Contains("Pork") || i.Contains("chicken") || i.Contains("fish")))
                {
                    diet = "";
                }
            }
        }


        //print sirialized json
        public void printserial()
        {
            Console.WriteLine(this.getjson());
        }

        //returns a json obj
        public string getjson()
        {
            return (JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        

        //better json serialisor  
        public Dictionary<string, Object> getDict()
        {
            return (JsonConvert.DeserializeObject<Dictionary<string, Object>>(JsonConvert.SerializeObject(this)));
        }
    }
}
