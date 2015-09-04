using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Data.SqlClient;
using System.Configuration;

namespace CapMyTask
{
    public partial class FrmCaptureDetail : Form
    {
        string ScreenPath;
        string tmpSelectTaskType;
        public FrmCaptureDetail()
        {
            InitializeComponent();
        }

        public void screenCapture(bool showCursor)
        {
            Point curPos = new Point(Cursor.Position.X, Cursor.Position.Y);
            Size curSize = new Size();
            curSize.Height = Cursor.Current.Size.Height;
            curSize.Width = Cursor.Current.Size.Width;

               //saveFileDialog.FileName = "c://capmytask//capture.Png";
               //ScreenPath = saveFileDialog.FileName;

                //Conceal this form while the screen capture takes place
                this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                this.TopMost = false;

                //Allow 250 milliseconds for the screen to repaint itself (we don't want to include this form in the capture)
                System.Threading.Thread.Sleep(450);

                Rectangle bounds = Screen.GetBounds(Screen.GetBounds(Point.Empty));
                string tmpIMG;
                string tmpUser;
                tmpIMG = ScreenShot.CaptureImage(Point.Empty, Point.Empty, bounds, ScreenPath, "Png");
                bool isDone = false;
                tmpUser = Environment.UserDomainName + "\\" + Environment.UserName;
                isDone = ScreenShot.saveToDB(tmpIMG, ScreenShot.getHeader(tmpUser), tmpSelectTaskType, txtComment.Text, Environment.UserName.ToString());
                //The screen has been captured and saved to a file so bring this form back into the foreground
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
                this.TopMost = true;
             
                //MessageBox.Show("Screen saved to clipboard", "TeboScreen", MessageBoxButtons.OK);
        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            screenCapture(false);
            this.Close();
        }

        private void FrmCaptureDetail_Load(object sender, EventArgs e)
        {
            lblUserName.Text = Environment.UserName;
            getTaskType();
        }

        private void getTaskType()
        {
            Principal objPrinciapl =  AccountManagement.getUserPrincipal();
            String tmpDepartment = AccountManagement.GetDepartment(objPrinciapl);
            string cmdString = "SELECT [TaskTypeName] ,[TaskTypeDescription] FROM [vwCapMyTask_TaskTypeList] where [DepartmentName] = @DepartmentValue";

            string connectionString = ConfigurationManager.ConnectionStrings["Conn"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand comm = new SqlCommand())
                {
                    comm.Connection = conn;
                    comm.CommandText = cmdString;
                    comm.Parameters.AddWithValue("@DepartmentValue", tmpDepartment);
                    conn.Open();
                    
                    SqlDataAdapter adapter = new SqlDataAdapter(comm);
                    
                    DataSet tmpDataset = new DataSet();
                    
                    adapter.Fill(tmpDataset,"vwCapMyTask_TaskTypeList");

                    lstTask.DataSource = tmpDataset.Tables["vwCapMyTask_TaskTypeList"];
                    lstTask.DisplayMember = "TaskTypeName";
                    lstTask.ValueMember = "TaskTypeName";
                    //comm.Dispose();
                    //conn.Close();
                    //conn.Dispose();
                }
            }
        }

        private void lstTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            tmpSelectTaskType = lstTask.SelectedValue.ToString() ;
        }
    }
}
