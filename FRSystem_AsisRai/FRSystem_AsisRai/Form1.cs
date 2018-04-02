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


namespace FRSystem_AsisRai
{
	public partial class Form1 : Form
	{
        Capture grabber; //to open the camera 
        Image<Bgr, byte> currentFrame; //to capture image 
        Image<Gray, byte> gray,result,TrainedFace = null; //initializing as an empty object  

        //initializing hharcascade for face detection
        HaarCascade face; //detection by face

        //initializing faces and name storage array 
        List<Image<Gray, byte>> detectedImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();

        int t = 0; //making t into 0 so that algorithm can increment it into 1 = True when face is found 

        int NumLabels,ContTrain=0;


        public Form1()
		{
            //loading haarcascade file by file name
            face = new HaarCascade("haarcascade-frontalface-default.xml");

			InitializeComponent();

            try
            {
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]); //total number of faces detected 
                ContTrain = NumLabels; //new images will be added to the previous set  
                string LoadFaces;

                for (int tf = 1; tf <NumLabels+1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    TrainedFace.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);

                }
            }

            catch (Exception e)
            {
                MessageBox.Show("Unexpected Error!");
            }


		}

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) //start camera button 
        {
            grabber = new Capture(); //when click camera wil be opened
            // 1.initializing the grabber event 
            grabber.QueryFrame();
            // 2.Now to capture the video 
            Application.Idle += new EventHandler(FrameGrabber); //if the application is idel and the camera is on then call the frame grabber event 
            // 3.initializing frame grabber 

        }

        private void button3_Click(object sender, EventArgs e) //save face button
        {
            ContTrain=ContTrain+1;
            
            //detected faces will be saved into a folder with the name of the person 
            //setting commands
            detectedImages.Add(TrainedFace);
            labels.Add(textBox1.Text);
            //write name of the detected person into list 
            File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", detectedImages.ToArray().Length.ToString() + "%");

            //write to files 
            for (int i = 1; i < detectedImages.ToArray().Length + 1; i++)
            {
                //save faces to folder with name face(i) i being the name/number of the face detected
                detectedImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                //Saves name to text file
                File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", labels.ToArray()[i - 1] + "%");
            }

            MessageBox.Show("Face(s) saved to the database!");

        }

        private void button2_Click(object sender, EventArgs e) //Show detected faces button 
        {
            //a. Resizing detected faces to grey scale images
            TrainedFace=result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            //b. Image box to show the detected faces
            imageBox2.Image = TrainedFace;

        }

        void FrameGrabber(object sender, EventArgs e) //Frame grabber event 
        {
            //initialize current frame with query grabber which is catching the frame
            currentFrame = grabber.QueryFrame().Resize(400, 300, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC); //resizing the frame with cubic frame

            //1. Converting image frame to gray scale (image processing) 
            gray = currentFrame.Convert<Gray, Byte>();

            //2. Detecting face by using Haar Classifier 
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            //face is name of the haar cascade, giving sizes to the cascade, applying canny pruning on haar classifier 

            //3. Checking each frame of image processed by the classifer through ImageBox (video is processed as image frames for face detection), then detect face
            foreach (MCvAvgComp f in facesDetected[0])
            {
                //a. If face is detected then increment t into 1 = True 
                t = t + 1;
                //b. Copy detected face in a frame name as result (gray.result)
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //resize copied frame and make it as cubic
                //view the result (detected image, face), convert current frame to grey scale 

                //c. Drawing traingle around on detected image (face) 
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);

            }
            //View current frame in the imported ImageBox
            imageBox1.Image = currentFrame; //current frame = captured from the camera into the imagebox
            //initialize the currentframe
        }
    }
}
