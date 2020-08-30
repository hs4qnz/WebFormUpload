using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Collections;
using System.Text;
using System.Web.SessionState;
using System.Data.SqlClient;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Net;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace WebFormUpload
{
    public partial class Upload : System.Web.UI.Page
    {

        SqlConnection con = null;
        SqlCommand com = null;
        string TransactionIdentificator = string.Empty;
        string Amount = string.Empty;
        string CurrencyCode = string.Empty;
        string TransactionData = string.Empty;
        string Status = string.Empty;    
        string type = String.Empty;
        string csvPath = String.Empty;
        

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                lblMessage.Visible = false;
                lblMessage.Text = "Upload File";
            }

        }


        protected void ReadCSV(object sender, EventArgs e)
        {
           
            try 

            {

                lblMessage.Visible = true;
                string filePath = FileUpload1.PostedFile.FileName; 
                string filename1 = Path.GetFileName(filePath); 
                string ext = Path.GetExtension(filename1); 
                string type = String.Empty;

                

                if (!FileUpload1.HasFile)
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Text = "Please Select File";
                    alert("Please Select File");
                    GridView1.DataSource = null;
                    GridView1.Columns.Clear();
                    GridView1.Visible = false;


                }

                else
                {
                    
                    csvPath = Server.MapPath("~/Files/") + Path.GetFileName(FileUpload1.PostedFile.FileName);
                    FileUpload1.SaveAs(csvPath);

                    switch (ext)  
                    {
                        case ".csv":
                            type = ConfigurationManager.AppSettings["FileUploadUrl"].ToString();
                            break;
                        case ".xml":
                            type = ConfigurationManager.AppSettings["FileUploadUrl"].ToString();
                            break;
                    }

                }


                
                int fileSize = FileUpload1.PostedFile.ContentLength;
                if (fileSize > 1048576)  // 1MB
                {
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                    lblMessage.Text = "Filesize of " + ext  + " is too large. Maximum file size is 1 MB";
                    WriteLog("Time:" + System.DateTime.Now.ToString("dd-MMM-yyyy HH:mm") + lblMessage.Text + string.Format(" Filesize is : {0} ", fileSize));
                   
                    return;
                }


                if (type != String.Empty && ext == ".csv")
                {

                    ImportCSV(csvPath);

                }
                else if (type != String.Empty && ext == ".xml")
                {

                    ImportXML(csvPath);

                }

                else
                {

                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Text = "Unknow format"; 
                    alert("Unknow format");
                    WriteLog("Time:" + System.DateTime.Now.ToString("dd-MMM-yyyy HH:mm") + lblMessage.Text);



                }


               

            }
            catch (Exception Ex)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Error info:" + Ex.Message;

                alert("Error info:" + Ex.Message);
            }


        }

        public void alert(string message)
        {
            Response.Write("<script>alert('" + message + "')</script>");
        }


        public void ImportXML(String XMlFile)

        {

          
            string TransactionDate = null;
            string Amount = null;
            string Status = null;
            string TransactionIdentificator = null;
            string CurrencyCode = null;


            SqlConnection connection;
            SqlCommand command;
            SqlDataAdapter adpter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            XmlReader xmlFile;
            string sql = null;

            

            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TD_DB_ConnectionString"].ToString());
            using (XmlReader reader = XmlReader.Create(XMlFile))
                xmlFile = XmlReader.Create(XMlFile, new XmlReaderSettings());
            ds.ReadXml(XMlFile);
            int i;
            connection.Open();
            for (i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
            {
           
                String date  = ds.Tables[0].Rows[i].ItemArray[0].ToString();
                TransactionDate = date.Replace("T", " ");
                Amount = ds.Tables[0].Rows[i].ItemArray[1].ToString();
            Status = ds.Tables[0].Rows[i].ItemArray[2].ToString();
            TransactionIdentificator = ds.Tables[0].Rows[i].ItemArray[3].ToString();
       

            sql = "insert into [dbo].[TBL_Upload] ([TransactionData],[Amount],[Status],[TransactionIdentificator],[CurrencyCode]) values('" + TransactionDate + "','" + Amount + "','" + Status + "','" + TransactionIdentificator + "','" + CurrencyCode + "')";
            command = new SqlCommand(sql, connection);
            adpter.InsertCommand = command;
            adpter.InsertCommand.ExecuteNonQuery();
        }
        connection.Close();
          

        }




    public  void ImportCSV(String csvPath)
        {

            try { 

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[5] {
        new DataColumn("TransactionIdentificator", typeof(string)),
        new DataColumn("Amount", typeof(Decimal)),
        new DataColumn("CurrencyCode", typeof(string)),
        new DataColumn("TransactionData", typeof(DateTime)),
        new DataColumn("Status", typeof(string)) });

           
            string csvData = File.ReadAllText(csvPath);

          
            foreach (string row in csvData.Split('\n'))
            {
                if (!string.IsNullOrEmpty(row))
                {
                    dt.Rows.Add();
                    int i = 0;




                    foreach (string cell in row.Split(','))
                    {
                        dt.Rows[dt.Rows.Count - 1][i] = cell;

                        i++;
                    }
                }
            }

             
                GridView1.DataSource = null;
                GridView1.DataSource = dt;
                GridView1.DataBind();

            for (int i = 0; i <= GridView1.Rows.Count - 1; i++)
            {

                CultureInfo culture = new CultureInfo("en-US");

                TransactionIdentificator = GridView1.Rows[i].Cells[0].Text;
                Amount = GridView1.Rows[i].Cells[1].Text;
                CurrencyCode = GridView1.Rows[i].Cells[2].Text;
                TransactionData = GridView1.Rows[i].Cells[3].Text;
                Status = GridView1.Rows[i].Cells[4].Text;
                DateTime NewDate = DateTime.Parse(TransactionData);



                con = new SqlConnection(ConfigurationManager.ConnectionStrings["TD_DB_ConnectionString"].ToString());

                Stream fs = FileUpload1.PostedFile.InputStream;
                BinaryReader br = new BinaryReader(fs); //reads the binary files  
                Byte[] bytes = br.ReadBytes((Int32)fs.Length); //counting the file length into bytes  
                String query = "insert into [dbo].[TBL_Upload] ([TransactionIdentificator],[Amount],[CurrencyCode],[TransactionData],[Status])"; 
                query = query + " values(@TransactionIdentificator, @Amount, @CurrencyCode, @TransactionData, @Status)";
                com = new SqlCommand(query, con);
                com.Parameters.Add("@TransactionIdentificator", SqlDbType.VarChar).Value = TransactionIdentificator;
                com.Parameters.Add("@Amount", SqlDbType.Decimal).Value = Amount;
                com.Parameters.Add("@CurrencyCode", SqlDbType.VarChar).Value = CurrencyCode;
                com.Parameters.Add("@TransactionData", SqlDbType.DateTime).Value = NewDate;
                com.Parameters.Add("@Status", SqlDbType.VarChar).Value = Status;


                con.Open();
                com.ExecuteNonQuery();
                con.Close();


            }
            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Text = "File Uploaded Successfully";


        }
            catch (Exception Ex)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Error info:" + Ex.Message;

                alert("Error info:" + Ex.Message);

                WriteLog("Time:" + System.DateTime.Now.ToString("dd-MMM-yyyy HH:mm") + lblMessage.Text);
            }

}





        private static FileStream fileStream;
        private static StreamWriter streamWriter;

        public  void OpenFile()
        {

            string strPath = Server.MapPath("~/Exception/") + System.DateTime.Today.ToString("yyyyMMdd") + ".log"; 

            if (System.IO.File.Exists(strPath))
                fileStream = new FileStream(strPath, FileMode.Append, FileAccess.Write);
            else
                fileStream = new FileStream(strPath, FileMode.Create, FileAccess.Write);

            streamWriter = new StreamWriter(fileStream);

        }

        public  void WriteLog(string strComments)
        {
            OpenFile();
            streamWriter.WriteLine(strComments);
            CloseFile();
        }

        public static void CloseFile()
        {
            streamWriter.Close();
            fileStream.Close();
        }



        


    }




}
