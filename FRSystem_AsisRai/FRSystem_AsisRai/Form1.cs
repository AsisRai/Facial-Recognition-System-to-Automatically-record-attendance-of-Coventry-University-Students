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


namespace FRSystem_AsisRai
{
	public partial class Form1 : Form
	{
        Capture grabber; //to open the camera 
        Image<Bgr, byte> currentFrame; //to capture image  

		public Form1()
		{
			InitializeComponent();
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

        void FrameGrabber(object sender, EventArgs e) //Frame grabber event 
        {
            //initialize current frame with query grabber which is catching the frame
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC); //resizing the frame with cubic frame
            //Now to view current frame in the imported ImageBox
            imageBox1.Image = currentFrame; //current frame = captured from the camera into the imagebox
            //initialize the currentframe
        }
    }
}
