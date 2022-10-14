using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Services;
using System.Web.Services;
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Aspose.Pdf.Facades;
using System.Threading;
using System.Drawing;
using Aspose.Pdf.Text;
using System.Net;
using System.Text.RegularExpressions;
using Aspose.Pdf.Forms;
using DocuWare.Platform.ServerClient;
using System.Net.Mail;
using System.Text;

namespace AsposePdfEditor
{
    [ScriptService]
    public partial class CanvasSave : System.Web.UI.Page
    {
        private static String guid = null;
        private static SUPPORTS_INTERVENTIONS inter = new SUPPORTS_INTERVENTIONS();
        private static OBJETS client = new OBJETS();
        private static USERS technicien = new USERS();
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Page_Load(object sender, EventArgs e)
        {
            ProcessRequest(HttpContext.Current);
        }

        public override void ProcessRequest(HttpContext context)
        {
            try
            {
                log.Info("Debut ProcessRequest");

                Aspose.Pdf.License license = new Aspose.Pdf.License();

                try
                {
                    license.SetLicense(@"C:\temp\Aspose.Total.lic");

                    log.Info("Licence trouvée");
                }
                catch(Exception e)
                {
                    log.Error(e);
                }

                if (context.Request.QueryString["Download"] != null)
                {
                    context.Response.AddHeader("Content-disposition", "attachment; filename=" + context.Request.QueryString["Download"].Split('/')[1].ToString());
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.TransmitFile(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + context.Request.QueryString["Download"].ToString());
                    context.Response.End();
                }

                else
                {
                    //Capture File From Post                  
                    guid = context.Request.Form["fileToUpload"];

                    log.Info("Recherche document ID " + guid);

                    //HttpPostedFile file = context.Request.Files["fileToUpload"];

                    if (!String.IsNullOrEmpty(guid))
                    { 
                        using(var dbContext = new bureaunetEntities())
                        {
                            Guid idInter = Guid.Parse(guid);

                            Directory.CreateDirectory(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + guid);

                            try
                            {
                                log.Info("Document trouvé : " + dbContext.SUPPORTS_INTERVENTIONS.Where(m => m.id == idInter).Count());

                                inter = dbContext.SUPPORTS_INTERVENTIONS.FirstOrDefault(m => m.id == idInter);

                                log.Info("Client trouvé : " + dbContext.OBJETS.Where(m => m.ID == inter.client).Count());

                                client = dbContext.OBJETS.FirstOrDefault(m => m.ID == inter.client);

                                log.Info("Technicien trouvé : " + dbContext.USERS.Where(m => m.ID == inter.technicien).Count());

                                technicien = dbContext.USERS.FirstOrDefault(m => m.ID == inter.technicien);
                            }
                            catch(Exception e)
                            {
                                log.Error(e);
                            }

                            String pathToFile = System.Configuration.ConfigurationManager.AppSettings["modelPath"];

                            log.Info("Chemin modèle : " + pathToFile);

                            File.Copy(pathToFile, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + guid + "/input-" + guid + ".pdf");
                            File.Copy(pathToFile, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + guid + "/output-" + guid + ".pdf");

                            Startup();

                            context.Response.Write(ImageConverter());
                        }

                    /*ServiceConnection conn = DocuwareProvider.Connect(System.Configuration.ConfigurationManager.AppSettings["URLDocuware"], System.Configuration.ConfigurationManager.AppSettings["loginDW"], System.Configuration.ConfigurationManager.AppSettings["passDW"]);

                        if (conn != null)
                        {
                            Organization org = DocuwareProvider.GetOrganization(conn);

                            ////////Obtaining the organization's cabinets
                            var fileCabinets = org.GetFileCabinetsFromFilecabinetsRelation().FileCabinet;

                            if (fileCabinets.Count() > 0)
                            {
                                defaultFilecabinet = fileCabinets.Where(f => !f.IsBasket && f.Id == System.Configuration.ConfigurationManager.AppSettings["FileCabinetID"]).First();

                                var dialogInfoItems = defaultFilecabinet.GetDialogInfosFromSearchesRelation();

                                var dialog = dialogInfoItems.Dialog.First(m => m.Id == System.Configuration.ConfigurationManager.AppSettings["SearchDialogID"]).GetDialogFromSelfRelation();

                                if (dialog != null)
                                {
                                    ////////Searchfor the document through its internal ID
                                    var qSearch = new DialogExpression()
                                    {
                                        Condition = new List<DialogExpressionCondition>()
                                            {
                                                DialogExpressionCondition.Create("GUID", guid )
                                            },
                                        Count = 1
                                    };

                                    var queryResult = dialog.Query.PostToDialogExpressionRelationForDocumentsQueryResult(qSearch);

                                    if (queryResult.Items.Count > 0)
                                    {
                                        myDocument = queryResult.Items.First().GetDocumentFromSelfRelation();

                                        var downloadedFile = DocuwareProvider.DownloadDocumentContent(myDocument);

                                        if (context.Request.Form["Opp"] == "uploading")
                                        {
                                            if (downloadedFile != null)
                                            {
                                                String pathToFile = System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "input-"+guid+".pdf";

                                                using (var file = System.IO.File.Create(pathToFile))
                                                using (var stream = downloadedFile.Stream)
                                                    stream.CopyTo(file);
                                            }
                                            var downloadedFile2 = DocuwareProvider.DownloadDocumentContent(myDocument);

                                            if (downloadedFile2 != null)
                                            {
                                                String pathToFile2 = System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "output-" + guid + ".pdf";

                                                using (var file2 = System.IO.File.Create(pathToFile2))
                                                using (var stream = downloadedFile2.Stream)
                                                    stream.CopyTo(file2);
                                            }

                                            Startup();

                                            context.Response.Write(ImageConverter());
                                        }
                                    }
                                }
                            }
                        }*/
                    }
                }
            }
            catch (IndexOutOfRangeException exception)
            {


            }
            catch (Exception exp)
            {

                
            }
        }

        [WebMethod()]
        public static string Download_From_Disk()
        {
            try
            {
               return ImageConverter();
            }
            catch (Exception Exp)
            {
                return Exp.Message;
            }
        }


        [WebMethod()]
        public static string Download_Dropbox(string file_url, string process)
        {
            try
            {
                // Create a new WebClient instance.
                using (WebClient myWebClient = new WebClient())
                {
                    if (process == "upload")
                    {

                        // myStringWebResource = remoteUri + fileName;
                        // Download the Web resource and save it into the current filesystem folder.
                        myWebClient.DownloadFile(file_url, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/input.pdf");
                        myWebClient.DownloadFile(file_url, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                        return ImageConverter();
                    }
                    else
                    {
                        string[] filename = file_url.Split('/');
                        string name = filename[filename.Length - 1];

                        // Download the Web resource and save it into the current filesystem folder.
                        myWebClient.DownloadFile(file_url, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Images/" + name);

                        return name;
                    }
                }
            }
            catch (Exception Exp)
            {
                return Exp.Message;
            }
        }

        [WebMethod()]
        public static string AddPage_Click(string lastpage)
        {
            try
            {
                Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                //insert an empty page at the end of a PDF file
                doc.Pages.Add();

                doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                string height = "";

                int counter = GetHighestPage();
                counter = counter + 1;

                using (FileStream imageStream = new FileStream(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Input/image-1" + counter + ".png", FileMode.Create))
                {
                    //Create Resolution object
                    Resolution resolution = new Resolution(300);
                    //create PNG device with specified attributes
                    PngDevice pngDevice = new PngDevice();
                    //Convert a particular page and save the image to stream
                    pngDevice.Process(doc.Pages[doc.Pages.Count], imageStream);
                    //Close stream
                    imageStream.Close();
                }
                string Aratio = "";
                System.Drawing.Image image = System.Drawing.Image.FromFile(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Input/image-1" + counter + ".png");
                ScaleImage(image, 1138, 760, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Input/image" + counter + ".png", out height, out Aratio);
                image.Dispose();
                return "image" + counter + ".png";
            }
            catch (Exception Exp)
            {
                return Exp.Message;
            }
        }

       

        [WebMethod()]
        protected static void ScaleImage(System.Drawing.Image image, int maxWidth, int maxHeight, string path, out string height,out string Aratio)
        {
            var ratio = (double)maxWidth / image.Width;
            Aratio = ratio.ToString();
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            height = newHeight.ToString();
            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            Bitmap bmp = new Bitmap(newImage);
            bmp.Save(path);

        }


        [WebMethod()]
        public static void DeletePage_Click(string imageData, string imageName)
        {
            try
            {
                Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                doc.Pages.Delete(Convert.ToInt32(imageData));

                doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                System.IO.File.Delete(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Input/" + imageName);
            }
            catch (Exception Exp)
            {
               // return Exp.Message;
            }
            
        }

        [WebMethod()]
        public static string[] MovePages(string moveFrom, string moveTo, string[] pageList)
        {
            try
            {
                int pageFrom = Convert.ToInt32(moveFrom);
                int pageTo = Convert.ToInt32(moveTo);
                List<string> str = pageList.ToList();

                Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                Aspose.Pdf.Page page = doc.Pages[pageFrom];

                //insert an empty page at the end of a PDF file
                doc.Pages.Insert(pageTo + 1, page);

                if (pageFrom > pageTo)
                {
                    doc.Pages.Delete(pageFrom+1);
                    str.Insert(pageTo, pageList[pageFrom - 1]);
                    str.RemoveAt(pageFrom);
                }
                else
                {
                    doc.Pages.Delete(pageFrom);                    
                    str.Insert(pageTo, pageList[pageFrom - 1]);
                    str.RemoveAt(pageFrom-1);
                }

                doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                pageList = str.ToArray();

                
                                
            }
            catch (Exception Exp)
            {
                // return Exp.Message;
            }

            return pageList;

        }


        [WebMethod()]
        public static void UploadPic(List<shap> shapes , string filename, string aspectRatio)
        {

            Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + guid + "/output-" + guid + ".pdf");

            String Date = string.Empty;
            String Technicien = string.Empty;
            String Duree = "00:00";
            String Descriptif = string.Empty;
            String Afaire = string.Empty;
            String Commercial = string.Empty;
            bool Contrat = true;
            bool ContratAstreinte = false;
            bool HorsContrat = false;
            bool Depannage = false;
            bool Entretien = false;
            bool Config = false;
            bool Sauvegarde = false;
            bool Serveur = false;
            bool PosteClient = false;
            bool Securite = false;
            bool Reseau = false;
            bool Scan = false;
            bool Messagerie = false;
            bool MOA = false;
            bool MOE = false;

            try
            {
                Depannage = (shapes[6].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Entretien = (shapes[7].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Config = (shapes[4].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Sauvegarde = (shapes[10].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Serveur = (shapes[0].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                PosteClient = (shapes[2].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Securite = (shapes[8].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Reseau = (shapes[19].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Scan = (shapes[1].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Messagerie = (shapes[3].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                MOA = (shapes[9].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                MOE = (shapes[5].t == "True") ? true : false;
            }
            catch
            { }
            try
            {
                Date = shapes[22].t;
            }
            catch
            { }
            try
            {
                Technicien = shapes[21].t;
            }
            catch
            { }
            try
            {
                Duree = shapes[20].t;
            }
            catch
            { }
            try
            {
                Descriptif = shapes[16].t;
            }
            catch
            { }
            try
            {
                Afaire = shapes[17].t;
            }
            catch
            { }
            try
            {
                Commercial = shapes[18].t;
            }
            catch
            { }
            try
            {
                Contrat = (shapes[11].t == "True") ? true : false;
            }
            catch
            { }

            try
            {
                ContratAstreinte = (shapes[12].t == "True") ? true : false;
            }
            catch
            { }

            try
            {
                HorsContrat = (shapes[13].t == "True") ? true : false;
            }
            catch
            { }


            for (int i = 0; i < shapes.Count; i++)
                {
                    //create stamp
                    Aspose.Pdf.Facades.Stamp stamp = new Aspose.Pdf.Facades.Stamp();

                    float shapeX = (shapes[i].x * 72 / 150) / (float)Convert.ToDouble(shapes[i].ratio);
                    float shapeY = (shapes[i].y * 72 / 150) / (float)Convert.ToDouble(shapes[i].ratio);
                    float shapeW = (shapes[i].w * 72 / 150) / (float)Convert.ToDouble(shapes[i].ratio);
                    float shapeH = (shapes[i].h * 72 / 150) / (float)Convert.ToDouble(shapes[i].ratio);
                                                            
                    double yaxis = (float)(doc.Pages[shapes[i].p].Rect.URY - (shapeH + shapeY));

                    var isSpecial = true;//regexItem.IsMatch(shapes[i].imName);

                    if(shapes[i].Itype == "text")
                    {

                    /*
                    // create TextBuilder for first page
                    TextBuilder tb = new TextBuilder(doc.Pages[shapes[i].p]);

                    // TextFragment with sample text
                    TextFragment fragment = new TextFragment(shapes[i].t);

                    // set the font for TextFragment
                    fragment.TextState.Font = FontRepository.FindFont(shapes[i].n);
                    fragment.TextState.FontSize = Convert.ToInt32(shapes[i].s);
                    if (shapes[i].wt == "bold")
                    {
                        fragment.TextState.FontStyle = FontStyles.Bold;
                    }

                    if (shapes[i].st == "italic")
                    {
                        fragment.TextState.FontStyle = FontStyles.Italic;
                    }

                    // set the formatting of text as Underline
                    // fragment.TextState.Underline = true;
                    fragment.TextState.ForegroundColor = GetColor(shapes[i].c);
                    // specify the position where TextFragment needs to be placed
                    fragment.Position = new Position((float)(shapeX), (float)(yaxis));

                   // fragment.Rectangle.Rotate((360 - (Convert.ToDouble(shapes[i].fieldType)) * 180 / Math.PI);

                    // append TextFragment to PDF file
                    tb.AppendText(fragment);
                    */

                    //create text stamp
                    Aspose.Pdf.TextStamp textStamp = new Aspose.Pdf.TextStamp(shapes[i].t);
                        //set whether stamp is background
                       // textStamp.Background = true;
                        //set origin
                        textStamp.XIndent = (float)(shapeX); 
                        textStamp.YIndent = (float)(yaxis);
                        //rotate stamp
                        textStamp.RotateAngle = 360 - ((Convert.ToDouble(shapes[i].fieldType)) * 180 / Math.PI);
                        
                        //set text properties
                        textStamp.TextState.Font = FontRepository.FindFont(shapes[i].n);
                        textStamp.TextState.FontSize = Convert.ToInt32(shapes[i].s) * 0.75f;

                        if (shapes[i].wt == "bold")
                        {
                            textStamp.TextState.FontStyle = FontStyles.Bold;
                        }

                        if (shapes[i].st == "italic")
                        {
                            textStamp.TextState.FontStyle = FontStyles.Italic;
                        }
                        
                       
                        textStamp.TextState.ForegroundColor = GetColor(shapes[i].c);
                        //add stamp to particular page
                        doc.Pages[shapes[i].p].AddStamp(textStamp);

                    }
                    else if (shapes[i].Itype == "field" && isSpecial)
                    {
                        if (shapes[i].fieldType == "Text")
                        {
                            // Get a field
                            TextBoxField textBoxField = doc.Form.Fields[Convert.ToInt32(shapes[i].imName)] as TextBoxField;
                            // Modify field value
                            textBoxField.Value = shapes[i].t;
                        
                        }
                        else if (shapes[i].fieldType == "CheckBox")
                        {

                            // Get a field
                            CheckboxField checkBoxField = doc.Form.Fields[Convert.ToInt32(shapes[i].imName)] as CheckboxField;
                            if(shapes[i].t != "")
                            // Modify field value
                            checkBoxField.Checked = Convert.ToBoolean(shapes[i].t);
                        }
                        else if (shapes[i].fieldType == "Radio")
                        {
                            RadioButtonOptionField field = (RadioButtonOptionField)doc.Form.Fields[Convert.ToInt32(shapes[i].imName)];

                            RadioButtonField rbf = (RadioButtonField)field.Parent;
                            if (Convert.ToBoolean(shapes[i].t))
                            {
                                rbf.Selected = rbf.Options[field.OptionName].Index;

                            }
                            else
                            {
                                field.ActiveState = "Off";

                            }
                        }
                        else if (shapes[i].fieldType == "ComboBox")
                        {                            

                            // Get a field
                            ComboBoxField comboBoxField = doc.Form.Fields[Convert.ToInt32(shapes[i].imName)] as ComboBoxField;
                            var values = shapes[i].t.Split(new string[]{"^^^"},StringSplitOptions.None)[0];

                            foreach (var item in comboBoxField.Options.Cast<Option>())
                            {
                                if (item.Value == values)
                                {

                                    comboBoxField.Selected = item.Index;
                                }
                            }
                        }
                    }

                }
            
                doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + guid + "/export-" + guid + ".pdf");

            using (var dbContext = new bureaunetEntities())
            {
                SUPPORTS_INTERVENTIONS saved = dbContext.SUPPORTS_INTERVENTIONS.FirstOrDefault(m => m.id == inter.id);

                saved.date_intervention = DateTime.Parse(Date);
                saved.descriptif = Descriptif;
                saved.actions = Afaire;
                saved.commercial = Commercial;
                saved.contrat = Contrat;
                saved.contratastreinte = ContratAstreinte;
                saved.horscontrat = HorsContrat;
                saved.depannage = Depannage;
                saved.entretien = Entretien;
                saved.config = Config;
                saved.sauvegarde = Sauvegarde;
                saved.serveur = Serveur;
                saved.posteclient = PosteClient;
                saved.reseau = Reseau;
                saved.scan = Scan;
                saved.messagerie = Messagerie;
                saved.moa = MOA;
                saved.moe = MOE;

                dbContext.SaveChanges();
            }
        }

        [WebMethod]
        public static string CreateNewFile()
        {
            Startup();

            Aspose.Pdf.Document doc = new Aspose.Pdf.Document();
            doc.Pages.Add();
            doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");


            return ImageConverter();
        }
        [WebMethod]
        public static string GetClientName()
        {
            return client.NOM + Environment.NewLine + Environment.NewLine + client.ADRESSE + Environment.NewLine + client.CP + " " + client.VILLE;
        }
        [WebMethod]
        public static string getTicketNumber()
        {
            return inter.ticket;
        }
        [WebMethod]
        public static string getDateInter()
        {
            return inter.date_intervention.Value.ToShortDateString();
        }
        [WebMethod]
        public static string getTechnicien()
        {
            return technicien.PRENOM + " " + technicien.NOM;
        }
        [WebMethod]
        public static string getField(string name)
        {
            switch (name)
            {
                case "ClientName":
                    return client.NOM + Environment.NewLine + Environment.NewLine + client.ADRESSE + Environment.NewLine + client.CP + " " + client.VILLE;
                case "TicketNumber":
                    return inter.ticket;
                case "DateInter":
                    return inter.date_intervention.Value.ToShortDateString();
                case "Technicien":
                    return technicien.PRENOM + " " + technicien.NOM;
                case "DureeInter":
                    return (inter.duree_intervention.Length > 0) ? inter.duree_intervention : "00:00";
                case "Descriptif":
                    return inter.descriptif;
                case "Afaire":
                    return inter.actions;
                case "Commercial":
                    return inter.commercial;
                default:
                    return string.Empty;

            }
        }
        [WebMethod]
        public static bool getOperation(string name)
        {
            switch(name)
            {
                case "contrat":
                    return (bool)inter.contrat;
                case "contratastreintes":
                    return (bool)inter.contratastreinte;
                case "horscontrat":
                    return (bool)inter.horscontrat;
                case "depannage":
                    return (bool)inter.depannage;
                case "entretien":
                    return (bool)inter.entretien;
                case "config":
                    return (bool)inter.config;
                case "sauvegarde":
                    return (bool)inter.sauvegarde;
                case "serveur":
                    return (bool)inter.serveur;
                case "posteclient":
                    return (bool)inter.posteclient;
                case "securite":
                    return (bool)inter.securite;
                case "reseau":
                    return (bool)inter.reseau;
                case "scan":
                    return (bool)inter.scan;
                case "messagerie":
                    return (bool)inter.messagerie;
                case "moa":
                    return (bool)inter.moa;
                case "moe":
                    return (bool)inter.moe;
                default:
                    return false;

            }
        }
        [WebMethod]
        public static string ImageConverter()
        {


            Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + guid + "/output-" + guid + ".pdf");
                
                
                string Aratio = "";
                string pages = "";
                string Ratios = "";
                string height = "";
                string Allheights = "";
                string fields = "";
                int TotalPages = 0;
                bool licensed = CheckLicense();

                if (licensed || (!licensed && doc.Pages.Count <= 4))
                    TotalPages = doc.Pages.Count;
                else
                    TotalPages = 4;

                for (int pageCount = 1; pageCount <= TotalPages; pageCount++)
                {
                    using (FileStream imageStream = new FileStream(System.Configuration.ConfigurationManager.AppSettings["tempPath"]+@"Input\" + "image_" + pageCount + ".png", FileMode.Create))
                    {
                        //Create Resolution object
                       // Resolution resolution = new Resolution(150);
                        //create PNG device with specified attributes
                        PngDevice pngDevice = new PngDevice();
                        //Convert a particular page and save the image to stream
                        pngDevice.Process(doc.Pages[pageCount], imageStream);
                        //Close stream
                        imageStream.Close();

                        System.Drawing.Image image = System.Drawing.Image.FromFile(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + @"Input\" + "image_" + pageCount + ".png");



                        ScaleImage(image, 1138, 760, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + @"Input\" + "image" + pageCount + ".png", out height, out Aratio);

                        image.Dispose();

                        if(pageCount == 1)
                        fields = CheckFields(doc, pageCount, "image" + pageCount + ".png", fields, Convert.ToDouble(Aratio), licensed);

                        pages = pages + "," + "image" + pageCount + ".png";
                        Ratios = Ratios + "," + Aratio;
                        Allheights = Allheights + "," + height;
                        
                    }

                }

                Ratios = Ratios.Substring(1, Ratios.Length - 1);
                pages = pages.Substring(1, pages.Length -1);
                Allheights = Allheights.Substring(1, Allheights.Length -1);
                if (fields != "")
                {
                    fields = fields.Substring(3, fields.Length - 3);
                }

                return pages + "%#" + Ratios + "%#" + Allheights + "%#" + fields;
           
        }

        private static string CheckFields(Aspose.Pdf.Document doc, int pageCount, string filename, string fields, double ratio, bool licensed)
        {
            double marginLeft = doc.Pages[pageCount].PageInfo.Margin.Left;
            double marginTop = doc.Pages[pageCount].PageInfo.Margin.Top;
                       
            int fieldcounter = 0;
            
            Aspose.Pdf.Facades.Form pdfForm = new Aspose.Pdf.Facades.Form(doc);

            // Get values from all fields
            foreach (Field formField in doc.Form.Fields)
            {
                try
                {

                    double lowerLeftY = (doc.Pages[pageCount].Rect.Height) - (formField.Rect.ToRect().Y + formField.Height);
                    //lowerLeftY += 12;

                    double lowerLeftX = formField.Rect.ToRect().X;
 
                    var fieldType = formField.GetType().Name; //pdfForm.GetFieldType(formField.FullName);
                    var imValue = "";

                    if (fieldType.ToString() == "CheckboxField" || fieldType.ToString() == "ComboBoxField" || fieldType.ToString() == "RadioButtonOptionField" || fieldType.ToString() == "TextBoxField")
                    {
                        var value = formField.Value;

                        if (fieldType.ToString() == "TextBoxField")
                        {
                            fieldType = "Text";
                        }
                        if (fieldType.ToString() == "CheckboxField")
                        {
                            CheckboxField field = (CheckboxField)formField;
                            if (field.Parent != null)
                                imValue = field.Parent.FullName;
                            fieldType = "CheckBox";
                            if (field.Checked)
                            {
                                value = "true";
                            }
                            else
                            {
                                value = "false";
                            }
                        }
                        if (fieldType.ToString() == "RadioButtonOptionField")
                        {
                            RadioButtonOptionField field = (RadioButtonOptionField)formField;
                            RadioButtonField rbf = (RadioButtonField)field.Parent;

                            fieldType = "Radio";
                            if (field.Parent != null)
                                imValue = field.Parent.FullName;
                            if ((rbf.Options[field.OptionName].Index == rbf.Selected))
                            {
                                value = "true";
                            }
                            else
                            {
                                value = "false";
                            }
                        }
                        if (fieldType.ToString() == "ComboBoxField")
                        {
                            ComboBoxField field = (ComboBoxField)formField;
                            string optValues = value;
                            fieldType = "ComboBox";
                            foreach (Option opt in field.Options)
                            {

                                optValues = optValues + "^^^" + opt.Value;

                            }
                            value = optValues;


                        }

                        bool isRequired = pdfForm.IsRequiredField(formField.FullName);
                        //fields += "$#$" + (lowerLeftX * 2.08) * ratio + "$#$" + (lowerLeftY * 2.08) * ratio + "$#$" + (formField.Width * 2.08) * ratio + "$#$" + (formField.Height * 2.08) * ratio + "$#$" + formField.PageIndex + "$#$" + "image" + formField.PageIndex + ".png" + "$#$" + value + "$#$" + formField.DefaultAppearance.FontName + "$#$" + formField.DefaultAppearance.FontSize + "$#$" + " " + "$#$" + " " + "$#$" + " " + "$#$" + ratio + "$#$" + " " + "$#$" + formField.FullName.Replace('/', '-') + "$#$" + fieldType;
                        fields += "$#$" + (lowerLeftX * 2.08) * ratio + "$#$" + (lowerLeftY * 2.08) * ratio + "$#$" + (formField.Width * 2.08) * ratio + "$#$" + (formField.Height * 2.08) * ratio + "$#$" + formField.PageIndex + "$#$" + "image" + formField.PageIndex + ".png" + "$#$" + value + "$#$" + formField.DefaultAppearance.FontName + "$#$" + formField.DefaultAppearance.FontSize + "$#$" + " " + "$#$" + " " + "$#$" + isRequired + "$#$" + ratio + "$#$" + imValue + "$#$" + fieldcounter + "$#$" + fieldType;
                    }
                    int length = fields.Length;
                    fieldcounter += 1;
                    if (!licensed && fieldcounter == 5)
                    {
                        break;
                    }
                }
                catch(Exception e)
                { }
            }            

            return fields;

        }

        [WebMethod]
        public static string AppendConverter(string appPages, string appRatios, string appHeights)
        {

            Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert /output.pdf");
            int totalCount = doc.Pages.Count;

            //open second document
            Aspose.Pdf.Document pdfDocument2 = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/append.pdf");

                //add pages of second document to the first
                doc.Pages.Add(pdfDocument2.Pages);
                               

                //save concatenated output file
                doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                string Aratio = "";
                string pages = "";
                string height = "";
                string Allheights = "";

                for (int pageCount = 1; pageCount <= doc.Pages.Count; pageCount++)
                {
                    if (pageCount > totalCount)
                    {
                        using (FileStream imageStream = new FileStream(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + @"Input\" + "image -"+guid+"-" + pageCount + ".png", FileMode.Create))
                        {
                            //Create Resolution object
                            //Resolution resolution = new Resolution(300);
                            //create PNG device with specified attributes
                            PngDevice pngDevice = new PngDevice();
                            //Convert a particular page and save the image to stream
                            pngDevice.Process(doc.Pages[pageCount], imageStream);
                            //Close stream
                            imageStream.Close();

                            System.Drawing.Image image = System.Drawing.Image.FromFile(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + @"Input\" + "image -" + guid + "-" + pageCount + ".png");



                            ScaleImage(image, 1138, 760, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + @"Input\" + "image -"+guid+"-" + pageCount + ".png", out height, out Aratio);

                            image.Dispose();

                            appPages = appPages + "," + "image" + pageCount + ".png";

                            appRatios = appRatios + "," + Aratio;

                            appHeights = appHeights + "," + height;
                        }
                    }                   

                }


                return appPages + "%#" + appRatios + "%#" + appHeights;
           
        }

        [WebMethod]
        public static string btnTextExport_Click(string fileType)
        {
            Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/Export.pdf");

                switch (fileType)
                {
                    case "txt":

                        //create TextAbsorber object to extract text
                        TextAbsorber textAbsorber = new TextAbsorber();

                        //accept the absorber for all the pages
                        doc.Pages.Accept(textAbsorber);

                        //get the extracted text
                        string extractedText = textAbsorber.Text;

                        System.IO.File.WriteAllText(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert /output.txt", extractedText);
                        return "output.txt";
                    case "pdf":
                        return "Export.pdf";
                    case "docx":
                        doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.docx", SaveFormat.DocX);
                        return "output.docx";
                    case "svg":
                        doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.svg", SaveFormat.Svg);
                        return "output.svg";
                    case "xps":
                        doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.xps", SaveFormat.Xps);
                        return "output.xps";
                    case "xls":
                        doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.xls", SaveFormat.Excel);
                        return "output.xls";
                    case "html":
                        doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.html", SaveFormat.Html);
                        return "output.html";
                    default:
                        return "";
                }
           

        }
             

        public static int GetHighestPage()
        {
            string[] files = Directory.GetFiles(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Input/");
            int[] highPage = new int[files.Length];
            for (int i = 0; i < files.Length; i++)
                highPage[i] = Convert.ToInt32(Path.GetFileName(files[i]).Replace("image", "").Replace(".png", ""));


            return highPage.Max();
        }

        [System.Web.Services.WebMethod()]
        public static string SearchData(string searchText, string[] pageList)
        {
            string name = DateTime.Now.Millisecond.ToString();
            System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "search/");

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
            {
                dir.Delete(true);
            }

            System.IO.Directory.CreateDirectory(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "search/" + name);

            Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                

                for (int i = 1; i <= doc.Pages.Count; i++)
                {
                    string filename = "Input/" + pageList[i-1];
                    filename = filename.Replace("image", "image-1");
                    Bitmap bmp = (Bitmap)Bitmap.FromFile(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + filename);
                    using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp))
                    {
                        float scale = 150 / 72f;
                        gr.Transform = new System.Drawing.Drawing2D.Matrix(scale, 0, 0, -scale, 0, bmp.Height);


                        Aspose.Pdf.Page page = doc.Pages[i];
                        //create TextAbsorber object to find all words
                        TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber(searchText);
                        //   textFragmentAbsorber.TextSearchOptions.IsRegularExpressionUsed = true;
                        page.Accept(textFragmentAbsorber);
                        //get the extracted text fragments
                        TextFragmentCollection textFragmentCollection = textFragmentAbsorber.TextFragments;

                        Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(50, 255, 255, 0));
                        //loop through the fragments
                        foreach (TextFragment textFragment in textFragmentCollection)
                        {
                            //  if (i == 0)
                            {
                                gr.FillRectangle(
                                    //   gr.DrawRectangle(
                                brush,
                                (float)(textFragment.Position.XIndent),
                                (float)(textFragment.Position.YIndent),
                                (float)(textFragment.Rectangle.Width),
                                (float)(textFragment.Rectangle.Height));

                                for (int segNum = 1; segNum <= textFragment.Segments.Count; segNum++)
                                {
                                    TextSegment segment = textFragment.Segments[segNum];


                                    gr.DrawRectangle(
                                    Pens.Green,
                                    (float)segment.Rectangle.LLX,
                                    (float)segment.Rectangle.LLY,
                                    (float)segment.Rectangle.Width,
                                    (float)segment.Rectangle.Height);
                                }
                            }
                        }
                        gr.Dispose();
                    }

                    bmp.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + filename.Replace("image -1","image_search"), System.Drawing.Imaging.ImageFormat.Png);
                    bmp.Dispose();
                   
                    string height = "";
                    string Aratio = "";
                    System.Drawing.Image image = System.Drawing.Image.FromFile(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + filename.Replace("image -1", "image_search"));
                    ScaleImage(image, 1138, 760, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "search/" + name + "/" + pageList[i - 1], out height, out Aratio);
                    image.Dispose();



                }

                return name;
        }

        public static string AddAttachments(string path, string filename)
        {
            // Open document
            using (Aspose.Pdf.Document pdfDocument = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf"))
            {
                // Setup new file to be added as attachment
                FileSpecification fileSpecification = new FileSpecification(path, filename);
                // Add attachment to document's attachment collection
                pdfDocument.EmbeddedFiles.Add(fileSpecification);
                // Save new output
                pdfDocument.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");
            }
            return "success";
        }

        [System.Web.Services.WebMethod()]
        public static string GetFileAttachments()
        {
            string outAttach = "";

            // Open document
            using (Aspose.Pdf.Document pdfDocument = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + guid + "/output-" + guid + ".pdf"))
            {

                // Get embedded files collection
                EmbeddedFileCollection embeddedFiles = pdfDocument.EmbeddedFiles;

                // Loop through the collection to get all the attachments
                foreach (FileSpecification fileSpecification in embeddedFiles)
                {
                    string[] filename = fileSpecification.Name.Split('\\');

                    outAttach = outAttach + "," + filename[filename.Length - 1] + "," + fileSpecification.Description;

                    // Get the attachment and write to file or stream
                    byte[] fileContent = new byte[fileSpecification.Contents.Length];
                    fileSpecification.Contents.Read(fileContent, 0, fileContent.Length);
                    FileStream fileStream = new FileStream(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Attachments/" + filename[filename.Length - 1], FileMode.Create);
                    fileStream.Write(fileContent, 0, fileContent.Length);
                    fileStream.Close();
                }
                if (outAttach.Length > 1)
                {

                    outAttach = outAttach.Substring(1);
                }
            }
                return outAttach;
        }

        [System.Web.Services.WebMethod()]
        public static string RemoveAttachments(string name)
        {

            // Open document
            using (Aspose.Pdf.Document pdfDocument = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf"))
            {

                // Delete all attachments
                pdfDocument.EmbeddedFiles.Delete(name);

                // Save updated file
                pdfDocument.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

            }
            return "success";
        }

        [System.Web.Services.WebMethod()]
        public static bool CheckLicense()
        {
           return true;
        }
        [WebMethod]
        public static string CreateSignature(string imageData)
        {

            System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Images/Signature/");

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }
            Random random = new Random();
            int rand =  random.Next(1000000);
            string fileNameWitPath = System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Images/Signature/sign" + rand+".png";
            using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    byte[] data = Convert.FromBase64String(imageData);
                    bw.Write(data);
                    bw.Close();
                }
            }

            return "Signature/sign" + rand + ".png";
        }
        [System.Web.Services.WebMethod()]
        public static string ReplaceText(string txtFind, string txtReplace, string[] pageList)
        {
            try
            {
                Aspose.Pdf.Document doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                //create TextAbsorber object to find all instances of the input search phrase
                TextFragmentAbsorber textFragmentAbsorber = new TextFragmentAbsorber("(?i)" + txtFind, new Aspose.Pdf.Text.TextSearchOptions(true));

                textFragmentAbsorber.TextReplaceOptions.ReplaceAdjustmentAction = TextReplaceOptions.ReplaceAdjustment.WholeWordsHyphenation;

                //accept the absorber for all the pages
                doc.Pages.Accept(textFragmentAbsorber);

                //get the extracted text fragments
                TextFragmentCollection textFragmentCollection = textFragmentAbsorber.TextFragments;

                //loop through the fragments
                foreach (TextFragment textFragment in textFragmentCollection)
                {
                    //update text and other properties
                    textFragment.Text = txtReplace;
                }

                doc.Save(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                doc = new Aspose.Pdf.Document(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Convert/output.pdf");

                System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Input/");

                foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                {
                    file.Delete();
                }


                for (int pageCount = 1; pageCount <= doc.Pages.Count; pageCount++)
                {
                    string filename = "Input/" + pageList[pageCount - 1];
                    //filename = filename.Replace("image", "image-1");

                    using (FileStream imageStream = new FileStream(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + filename, FileMode.Create))
                    {
                        //Create Resolution object
                        Resolution resolution = new Resolution(300);
                        //create PNG device with specified attributes
                        PngDevice pngDevice = new PngDevice();
                        //Convert a particular page and save the image to stream
                        pngDevice.Process(doc.Pages[pageCount], imageStream);
                        //Close stream
                        imageStream.Close();

                        System.Drawing.Image image = System.Drawing.Image.FromFile(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + filename);

                        string height = "";
                        string Aratio = "";

                        ScaleImage(image, 1138, 760, System.Configuration.ConfigurationManager.AppSettings["tempPath"] + filename.Replace("image -1", "image"), out height, out Aratio);

                        image.Dispose();

                    }

                }
            }
            catch (Exception exp)
            { 
                
            }
            return "success";
        }

        public static void Startup()
        {

            System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Input/");

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }

            downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Attachments/");

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }

            downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Images/");

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }

            downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "Images/Signature/");

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }

            downloadedMessageInfo = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["tempPath"] + "search/");

            foreach (FileInfo file in downloadedMessageInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        
        }
        public static Aspose.Pdf.Color GetColor(string color)
        { 
            switch(color)
            {
                case "red":
                    return Aspose.Pdf.Color.Red;
                case "black":
                    return Aspose.Pdf.Color.Black;
                case "green":
                    return Aspose.Pdf.Color.Green;
                case "white":
                    return Aspose.Pdf.Color.White;
                case "blue":
                    return Aspose.Pdf.Color.Blue;
                case "grey":
                    return Aspose.Pdf.Color.Gray;

                case "yellow":
                    return Aspose.Pdf.Color.Yellow;
                default:
                    return Aspose.Pdf.Color.Black;
            }
        }
    }

    public class shap
    {
        public float x { get; set; }
        public float y { get; set; }
        public float w { get; set; }
        public float h { get; set; }
        public int p { get; set; }
        public string f { get; set; }
        public string t { get; set; }
        public string n { get; set; }
        public string s { get; set; }
        public string c { get; set; }
        public string wt { get; set; }
        public string st { get; set; }
        public string ratio { get; set; }
        public object imfile { get; set; }
        public string imName { get; set; }
        public string Itype { get; set; }
        public string fieldType { get; set; }

    }

    


}