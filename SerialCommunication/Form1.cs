using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SerialCommunication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();
                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;

                comboBoxBaudrate.SelectedIndex = comboBoxBaudrate.Items.IndexOf("115200");
            }
            catch (Exception)
            { }
        }

        private void cboPoort_DropDown(object sender, EventArgs e)
        {
            try
            {
                string selected = (string)comboBoxPoort.SelectedItem;
                string[] portNames = SerialPort.GetPortNames().Distinct().ToArray();

                comboBoxPoort.Items.Clear();
                comboBoxPoort.Items.AddRange(portNames);

                comboBoxPoort.SelectedIndex = comboBoxPoort.Items.IndexOf(selected);
            }
            catch (Exception)
            {
                if (comboBoxPoort.Items.Count > 0) comboBoxPoort.SelectedIndex = 0;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {

            try
            {

                if (!serialPortArduino.IsOpen)
                {
                    serialPortArduino.PortName = comboBoxPoort.Text;
                    serialPortArduino.BaudRate = int.Parse(comboBoxBaudrate.Text);
                    serialPortArduino.DataBits = 8;
                    serialPortArduino.Parity = Parity.None;
                    serialPortArduino.StopBits = StopBits.One;
                    serialPortArduino.Handshake = Handshake.None;
                    serialPortArduino.RtsEnable = true;
                    serialPortArduino.DtrEnable = true;
                    serialPortArduino.Open();
                    radioButtonVerbonden.Checked = true;
                    buttonConnect.Text = "Disconnect";
                    labelStatus.Text = "Verbonden met Arduino";
                }
                else
                {
                    serialPortArduino.Close();
                    radioButtonVerbonden.Checked = false;
                    buttonConnect.Text = "Connect";
                    labelStatus.Text = "Niet verbonden";
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            timerOefening5.Enabled = tabControl.SelectedIndex == 5;
            timerOefening4.Enabled = tabControl.SelectedIndex == 4;
            timerOefening3.Enabled = tabControl.SelectedIndex == 3;
            timerSchemerschakeling.Enabled = tabControl.SelectedIndex == 6;
        }
        //Oefening5
        private void timerOefening5_Tick(object sender, EventArgs e)
        {
            if (!serialPortArduino.IsOpen)
                return;
            try
            {
                // Gewenste temperatuur (AO)
                serialPortArduino.WriteLine("get a0");
                if (!serialPortArduino.IsOpen)
                    return;
                serialPortArduino.ReadExisting();
                serialPortArduino.WriteLine("get a0");
                string antwoord = serialPortArduino.ReadLine().Trim();
                if (antwoord.Length < 4)
                    return;
                if (!int.TryParse(antwoord.Substring(4), out int rawGewenst))
                    return;
                labelAnalog0.Text = rawGewenst.ToString();
                double gewensteTemp = (40.0 / 1023.0) * rawGewenst + 5.0;
                labelGewensteTemp.Text = gewensteTemp.ToString("0.0") + " °C";
                // Huidige temperatuur (A1)
                serialPortArduino.WriteLine("get a1");
                if (!serialPortArduino.IsOpen)
                    return;
                serialPortArduino.ReadExisting(); serialPortArduino.WriteLine("get a1");
                string antwoord2 = serialPortArduino.ReadLine().Trim();
                if (antwoord2.Length < 4)
                    return;
                if (!int.TryParse(antwoord2.Substring(4), out int rawHuidig))
                    return;
                if (rawHuidig < 20)
                    return;
                double ruw = rawHuidig * 500 / 1023.0;
                double huidigeTemp = ruw;
                labelHuidigeTemp.Text = huidigeTemp.ToString("0.0") + " °C";
                // LED aansturen
                if (huidigeTemp < gewensteTemp)
                    serialPortArduino.WriteLine("set d2 high");
                else
                    serialPortArduino.WriteLine("set d2 low");
            }
            catch (Exception exception)
            {
                labelStatus.Text = "Error: " + exception.Message;
                serialPortArduino.Close();
                radioButtonVerbonden.Checked = false;

                buttonConnect.Text = "Connect";
            }

        }


    }
}
