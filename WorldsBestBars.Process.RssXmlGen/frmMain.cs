using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;


namespace WorldsBestBars.Process.RssXmlGen
{
    public partial class frmMain : Form
    {

         private bool autostart;

        public frmMain()
        {
            InitializeComponent();
        }

        public frmMain(bool Autostart)
        {
            InitializeComponent();
            autostart = Autostart;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

            /// <summary>
        /// Create the image url
        /// </summary>
        /// <param name="RelativeUrl"></param>
        /// <returns></returns>
        private string GetImageUrl(string RelativeUrl)
        {
          
            try
            {
                string sServerPathBase = "http://www.worldsbestbars.com/content/media/" + RelativeUrl;// +"/245x165.orig.01.jpg";
                string sPath = @"C:\websites\com.worldsbestbars\#shared\media\" + RelativeUrl.Replace("/", "\\");

                DirectoryInfo dir = new DirectoryInfo(sPath);

                List<string>fileList = Directory.GetFiles(sPath,"original.*", System.IO.SearchOption.TopDirectoryOnly).ToList<string>();

                if (fileList.Count > 0)
                    return "http://www.worldsbestbars.com/content/media/" + RelativeUrl + "/"+ Path.GetFileName(fileList[0]);
               
 
            }
            catch (Exception ex)
            {
                File.AppendAllText("Error.log", DateTime.Now.ToString() + " : "  + ex.Message);
            
            }
            return "";
        }
     
    

        /// <summary>
        /// Generate the RSS XML
        /// </summary>
        private void CreateRSS()
        {
            // Create XML
            XNamespace nsGeoRSS = "http://www.georss.org/georss";
            XNamespace nsFieldTrip = "http://www.fieldtripper.com/fieldtrip_rss";
        
            List<Bar> list = Bar.GetAllBar();
            XDocument doc =
         new XDocument(
         new XElement("rss", new XAttribute("version", "2.0"),
         new XAttribute(XNamespace.Xmlns + "georss", "http://www.georss.org/georss"),
           new XAttribute(XNamespace.Xmlns + "fieldtrip", "http://www.fieldtripper.com/fieldtrip_rss"),
         new XElement("feed",
         new XElement("title", "Worlds Best Bars"),
         new XElement("link", "http://www.worldsbestbars.com"),
         new XElement("Description", "WorldsBestBars.com is much more than a bar directory, it's a site with an ongoing mission to locate the finest bars the planet has to offer and boasts reviews of over 2,000 of the world's most prestigious places to drink and be seen in."),
         new XElement("image",
         new XElement("url", "http://www.worldsbestbars.com/content/img/logos/wbb_logo.png")),
         new XElement("pubDate", DateTime.Today.ToString("R")),
         new XElement("lastBuildDate", "Worlds Best Bars"),
         new XElement("managingEditor", "noreply@worldsbestbars.com"),
         new XElement("language", "en"),

         list.Select(x => new XElement("item",
             new XElement("title", x.Name),
             new XElement(nsGeoRSS + "point", ReformatCoordString(x.GeocoordinateString)),
             new XElement(nsFieldTrip + "address", CreateAddressString(x.Address)),
             new XElement("guid", x.Id),
             new XElement("description", x.Description),
             new XElement("link", "http://www.worldsbestbars.com/" + x.Url),
             new XElement("image",
             new XElement("url", GetImageUrl(x.Url)),
             new XElement("title", "Image of " + x.Name),
             new XElement("attribution", "Source www.worldsbestbars.com")),
             new XElement("pubDate", DateTime.Today.ToString("R")),
             new XElement("startDate", DateTime.Today.ToString("R"))
          )
         )
                //  new XElement("endDate",DateTime.Today.ToString("R")),
             ))

        );
            doc.Save(Properties.Settings.Default.OutputPath);
        }

        private string CreateAddressString(string p)
        {
           // Parse xml and return string
            XDocument doc = XDocument.Parse(p);
            string sAddress = "";
       
            foreach (XElement x in doc.Root.Descendants())
            {
                if(!String.IsNullOrEmpty(x.Value))
                sAddress += x.Value + ", ";
            }
            return sAddress.TrimEnd(',',' ');
        }

        private string ReformatCoordString(string p)
        {
            string sNew = p.Substring(7);
            sNew = sNew.Substring(0,sNew.IndexOf(")"));

            return sNew;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            btnStart.Enabled = false;
            CreateRSS();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.Enabled = true;
            if (autostart)
                Application.Exit();
        }

        private void txtOutputPath_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.OutputPath = txtOutputPath.Text;
            Properties.Settings.Default.Save();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            txtOutputPath.Text = Properties.Settings.Default.OutputPath;
            this.Text = "Autostart";
            if (autostart)
                backgroundWorker1.RunWorkerAsync();
        }

 
    }
}
