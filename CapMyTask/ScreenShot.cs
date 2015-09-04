using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;

using System.Linq;
using System.Text;

using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace CapMyTask
{
    public static class AccountManagement
    {

        public static String GetProperty(this Principal principal, String property)
        {
            DirectoryEntry directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry.Properties.Contains(property))
                return directoryEntry.Properties[property].Value.ToString();
            else
                return String.Empty;
        }

        public static String GetCompany(this Principal principal)
        {
            return principal.GetProperty("company");
        }

        public static String GetDepartment(this Principal principal)
        {
            return principal.GetProperty("department");
        }

        public static UserPrincipal getUserPrincipal()
        {
      
            PrincipalContext domain = new PrincipalContext(ContextType.Domain);
            UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(domain, Environment.UserName);
            return userPrincipal;
    }

    }
    class ScreenShot
    {
        public static String getDepartmentManager(String tmpDepartment)
        {
            String _sResult = "No Department Specified";

            string cmdString = "SELECT [DepartmentManager] FROM [Department] where [DepartmentName] = @DepartmentValue";
            string connectionString = ConfigurationManager.ConnectionStrings["Conn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;
                    comm.CommandText = cmdString;
                    comm.Parameters.AddWithValue("@DepartmentValue", tmpDepartment);
                    
                    conn.Open();

                    SqlDataReader oReader = comm.ExecuteReader();

                    if (oReader.HasRows) // found record
                    {
                        // get ref number    
                        oReader.Read();
                        _sResult = oReader["DepartmentManager"].ToString();
                    }

                    //comm.Dispose();
                    //conn.Close();
                    //conn.Dispose();
                }
            }
            return _sResult;
        }
        public static String getDateText()
        {
            String _sResult = "ddmmyyyy";

            _sResult = DateTime.Today.Day.ToString().PadLeft(2, '0');
            _sResult = _sResult + DateTime.Today.Month.ToString().PadLeft(2,'0');
            _sResult = _sResult + DateTime.Today.Year.ToString().PadLeft(4, '0');

            return _sResult;
        }
        public static Int32 getHeader(String User)
        {
            Int32 _iResult = 0;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["Conn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand comm = new SqlCommand())
                    {
                        string cmdString = "Select top 1 [ID] from [dbo].[TaskTracking] where DATEDIFF(DAY, [SubmittedDate], @val2) = 0 and [SubmittedBy] = @val3";
                        
                        comm.Connection = conn;
                        comm.CommandText = cmdString;
                        comm.Parameters.AddWithValue("@val3", User);
                        //comm.Parameters.AddWithValue("@val2", "DAY");
                        comm.Parameters.AddWithValue("@val2", DateTime.Today);
                        conn.Open();
                        SqlDataReader oReader = comm.ExecuteReader();

                        if (oReader.HasRows) // found record
                            {
                                // get ref number    
                                oReader.Read();
                                _iResult = Convert.ToInt32(oReader["ID"].ToString());
                            } else {

                                oReader.Close();
                                
                            Principal objPrinciapl =  AccountManagement.getUserPrincipal();
                            String tmpDepartment = AccountManagement.GetDepartment(objPrinciapl);


                                // Create new record
                                cmdString = "INSERT INTO [dbo].[TaskTracking] ([SubmittedBy],[SubmittedDate],[Status],[DepartmentID],[Manager]) VALUES (@SubmittedByVal, getdate(),@StatusVal,@DepartmentVal,@ManagerVal);SELECT SCOPE_IDENTITY();";
                                comm.CommandText = cmdString;
                                comm.Parameters.AddWithValue("@SubmittedByVal", Environment.UserDomainName + "\\" + Environment.UserName);
                                comm.Parameters.AddWithValue("@StatusVal","Submitted");
                                comm.Parameters.AddWithValue("@DepartmentVal",tmpDepartment);
                                comm.Parameters.AddWithValue("@ManagerVal", getDepartmentManager(tmpDepartment));

                                _iResult  = (Int32) comm.ExecuteNonQuery();
                            }

                        comm.Dispose();
                        conn.Close();
                        conn.Dispose();
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return _iResult;
        }
        public static bool saveToDB(String ImgPNGBase64, Int32 TaskTrackingID, String TaskType, String Comment, String User)
        {
            bool _bResult = false;

            try
            {
                String DateText = getDateText();
                String SerializedK2ImgBase64 = String.Format("<image><name>{0}</name><content>{1}</content></image>", User + "-" + TaskType + "-" + TaskTrackingID + DateText + ".Png", ImgPNGBase64);
                String cmdString = "INSERT INTO [dbo].[TaskTrackingItem]([TaskTrackingID],[TaskType],[Attachment],[Comment],[CommentOn],[Requestor],[SubmittedOn],[DepartmentID],[Manager],[Status]) VALUES (@val1, @val2, @val3, @val4, @val5, @val6, @val7, @val8, @val9, @val10)";
                String tmpUser = Environment.UserDomainName + "\\" + Environment.UserName;
                Principal objPrinciapl = AccountManagement.getUserPrincipal();
                String tmpDepartment = AccountManagement.GetDepartment(objPrinciapl);
                
                string connectionString = ConfigurationManager.ConnectionStrings["Conn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand comm = new SqlCommand())
                    {
                        comm.Connection = conn;
                        comm.CommandText = cmdString;
                        comm.Parameters.AddWithValue("@val1", TaskTrackingID);
                        comm.Parameters.AddWithValue("@val2", TaskType);
                        comm.Parameters.AddWithValue("@val3", SerializedK2ImgBase64);
                        comm.Parameters.AddWithValue("@val4", Comment);
                        comm.Parameters.AddWithValue("@val5", DateTime.Today);
                        comm.Parameters.AddWithValue("@val6", tmpUser);
                        comm.Parameters.AddWithValue("@val7", DateTime.Today);
                        comm.Parameters.AddWithValue("@val8", tmpDepartment);
                        comm.Parameters.AddWithValue("@val9", getDepartmentManager(tmpDepartment));
                        comm.Parameters.AddWithValue("@val10", "Waiting for Manager Approval");
                        conn.Open();
                        comm.ExecuteNonQuery();
                        
                        
                        comm.Dispose();
                        conn.Close();
                        conn.Dispose();
                    }
                }
                
                _bResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _bResult = false;
            }
            

            return _bResult;
        }
        public static string ImageToBase64(Image image,  System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
        public static string CaptureImage(Point SourcePoint, Point DestinationPoint, Rectangle SelectionRectangle, string FilePath, string extension)
        {

            using (Bitmap bitmap = new Bitmap(SelectionRectangle.Width, SelectionRectangle.Height))
            {

                using (Graphics g = Graphics.FromImage(bitmap))
                {

                    g.CopyFromScreen(SourcePoint, DestinationPoint, SelectionRectangle.Size);
                }
                    Image img = (Image)bitmap;
                    Clipboard.SetImage(img);
                    return ImageToBase64(img, ImageFormat.Png); 
            }
        }
    }
}