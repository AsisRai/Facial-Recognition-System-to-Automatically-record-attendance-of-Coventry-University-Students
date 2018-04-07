using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Data.SqlClient;

namespace FRSystem_AsisRai
{
    public partial class DetectAndAttendance : Form
    {
        //initializing 
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        Image<Gray, byte> result, TrainedFace = null, TrainedEyes = null, TrainedMouth = null, TrainedNose = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        //Initializing a list to save detected names of students
        List<string> NamePersons = new List<string>();
        string name = null, names = null;
        int t, ContTrain, NumLabels;

        SqlConnection con; //connection

        private HashSet<string> FacesAlreadyDetected = new HashSet<string>();

        private void resetAttendanceButton_Click(object sender, EventArgs e)
        {
            FacesAlreadyDetected.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Trust\Documents\GitHub\Facial-Recognition-System-to-Automatically-record-attendance-of-Coventry-University-Students\FRSystem_AsisRai\frsystem_database.mdf;Integrated Security=True;Connect Timeout=30");
            SqlDataAdapter checkup = new SqlDataAdapter("SELECT * FROM attendance", con); //this will get all marked attendance from the database
            DataTable sd = new DataTable();

            checkup.Fill(sd);
            dataGridView1.DataSource = sd;

            DataTable sd1 = new DataTable();
            sd1 = sd.DefaultView.ToTable(true, "name", "studentid", "dateandtime");

            dataGridView1.DataSource = sd1;
        }

        private void closeProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MAINGUI main = new MAINGUI();
            main.Show();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            grabber = new Capture();
            grabber.QueryFrame();
            Application.Idle += new EventHandler(FrameGrabber);
        }
        
        public DetectAndAttendance()
        {
            InitializeComponent();
            face = new HaarCascade("haarcascade-frontalface-default.xml");
            try
            {
                //Load previous trainned faces of students and their names
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels + 1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    //make a list of string
                    labels.Add(Labels[tf]); 
                }  
            }
            catch (Exception e)
            {
                MessageBox.Show("Press OK to proceed!");
            }
        }

        public void FrameGrabber(object sender, EventArgs e)
        {
            NamePersons.Add("");
            //Detect number of faces on screen
            label5.Text = "0";

            //Get the current frame form capture device
            currentFrame = grabber.QueryFrame().Resize(501, 407, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //Convert it to Grayscale
            gray = currentFrame.Convert<Gray, Byte>();

            //Face Detector
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
          face,
          1.3,
          10,
          Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
          new Size(20, 20));

            

            //Action for each element detected
            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //draw the face detected in the 0th (gray) channel with blue color
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);
                //initialize result,t and gray if (trainingImages.ToArray().Length != 0)
                {
                    //term criteria against each image to find a match with it, perform different iterations
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    //call class by creating object and pass parameters
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                         trainingImages.ToArray(),
                         labels.ToArray(),
                         5000,
                         ref termCrit);
                    //next step is to name find for recognize face
                    name = recognizer.Recognize(result);
                    //now show recognized person name so
                    currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));//initalize font for the name captured

                }

                if (!FacesAlreadyDetected.Contains(name))
                {
                    SaveToDatabase(name, DateTime.Now);
                    FacesAlreadyDetected.Add(name);
                }
                

                NamePersons[t - 1] = name;
                NamePersons.Add("");
                //check detected faces 
                label5.Text = facesDetected[0].Length.ToString();
            }
            t = 0;

            //Names concatenation of persons recognized
            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                names = names + NamePersons[nnn] + ", ";
                //MessageBox.Show(NamePersons[nnn]);

                string test = NamePersons[nnn] + ",";

                System.IO.File.AppendAllText("C:\\Users\\Trust\\Documents\\GitHub\\Facial-Recognition-System-to-Automatically-record-attendance-of-Coventry-University-Students\\FRSystem_AsisRai\\FRSystem_AsisRai\\Names\\names.txt", test);

            }
            //load haarclassifier and previous saved images to find matches
            imageBox1.Image = currentFrame;
            label3.Text = names;
            names = "";
            NamePersons.Clear();

        }

        private void SaveToDatabase(string studentID, DateTime dateTime)
        {
            using (SqlConnection Connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Trust\Documents\GitHub\Facial-Recognition-System-to-Automatically-record-attendance-of-Coventry-University-Students\FRSystem_AsisRai\frsystem_database.mdf;Integrated Security=True;Connect Timeout=30"))
            {
                
                try
                {
                    Connection.Open();

                    SqlCommand getNameFromSID = new SqlCommand(@"SELECT name from STUDENT WHERE studentid=@studentid", Connection);
                    getNameFromSID.Parameters.AddWithValue("@studentid", studentID);

                    string studentName = string.Empty;

                    using (SqlDataReader reader = getNameFromSID.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                studentName = reader["name"].ToString();
                            }
                        }
                    }

                    SqlCommand cmd = new SqlCommand(@"INSERT INTO attendance ([name],[studentid],[dateandtime]) VALUES (@name, @studentid, @datetime);", Connection);
                    cmd.Parameters.AddWithValue("@name", studentName);
                    cmd.Parameters.AddWithValue("@studentid", studentID);
                    cmd.Parameters.AddWithValue("@datetime", dateTime);


                    int i = cmd.ExecuteNonQuery();
                    Connection.Close();

                    if (i == 1)
                    {
                        MessageBox.Show($"Attendance Registered for {studentName}");
                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected Error has occured:" + ex.Message);

                }
            }
        }
        
        private void DetectAndAttendance_Load(object sender, EventArgs e)
        {

        }
    }
}
