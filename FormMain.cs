using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;

using AngleSharp;
using System;

namespace SiteParser
{
    public partial class FormMain : Form
    {
        private List<string> _urls;
        private List<Address> _address;

        public FormMain()
        {
            _urls = new List<string>();
            _address = new List<Address>();
            InitializeComponent();
        }

        private async void DataButton_Click(object sender, System.EventArgs e)
        {
            _urls.Clear();
            _address.Clear();
            string url = "https://bestmaps.ru/city/orenburg/street";

            var config = Configuration.Default.WithDefaultLoader();
            using (var context = BrowsingContext.New(config))
            using (var doc = await context.OpenAsync(url))
            {
                var urlsFromSite = doc.QuerySelectorAll("a");
                foreach (var ufs in urlsFromSite)
                {
                    if (ufs.GetAttribute("href").Contains("/city/orenburg"))
                        if (!ufs.GetAttribute("href").Contains("/city/orenburg/street/letter"))
                        {
                            _urls.Add(ufs.GetAttribute("href"));
                        }
                }
            }

            //for a specific amount of elements
                        //for (int i = 0; i < 3; i++)
            foreach (var _url in _urls)
            {
                //for a specific amount of elements
                //string streetUrl = "https://bestmaps.ru" + _urls[i];
                string streetUrl = "https://bestmaps.ru" + _url;

                using (var context = BrowsingContext.New(config))
                using (var doc = await context.OpenAsync(streetUrl))
                {
                    var streetName = doc.GetElementsByClassName("pageTitle")[0];
                    var houses = doc.GetElementsByClassName("house_bg");

                    foreach (var house in houses)
                    {
                        //add all addresses in _address List
                        _address.Add(new Address()
                        {
                            Street = streetName.InnerHtml,
                            House = house.InnerHtml
                        }
                        );
                    }
                }
            }

            DataTextBox.Text += $"Found {_address.Count} houses";
        }


        private void SaveButton_Click(object sender, System.EventArgs e)
        {
            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string json = JsonSerializer.Serialize(_address, options);
                File.WriteAllText(SaveDialog.FileName, json);
                MessageBox.Show("File was successfully saved");
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(SaveDialog.FileName);

                foreach (var adr in _address)
                {
                    sw.Write(adr.Street + " | ");
                    sw.WriteLine(adr.House);
                }

                sw.Close();
                MessageBox.Show("File was successfully saved");
            }
        }
    }
}
