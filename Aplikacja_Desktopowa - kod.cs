using OpenHardwareMonitor.Hardware;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Linq;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sockets.Plugin;
using System.Text;


namespace Aplikacjamonitorująca
{

    public partial class Form1 : Form
    {
        
        
       //pobieranie informacji o dysku i karcie sieciowej
        private Computer computer = new Computer() { CPUEnabled = true, GPUEnabled = true, RAMEnabled = true };


        private PerformanceCounter PerfDiskUsage = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
        private PerformanceCounter PerfDiskRead = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
        private PerformanceCounter PerfDiskWrite = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
     
        private PerformanceCounter PerfNetworkSent = new PerformanceCounter("Network Interface", "Bytes Sent/sec", "Qualcomm Atheros QCA9377 Wireless Network Adapter");
        private PerformanceCounter PerfNetworkReceived = new PerformanceCounter("Network Interface", "Bytes Received/sec", "Qualcomm Atheros QCA9377 Wireless Network Adapter");
        private PerformanceCounter PerfCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        

        string filepath = @"C:\ostrzezenia.txt"; //sciezka zapisywania ostrzezen
        

 
       
        private DriveInfo[] drives = DriveInfo.GetDrives();

        string buffer;
     //liczniki używane do pobieranie informacji o podzespołąch za pomocą biblioteki openhardwaremonitor 
        private int counter = 0;
        private int counter2 = 0;
        private int counter3 = 0;
        private int counter4 = 0;
        private int counter5 = 0;
        
     //temperatury rdzeni procesora
        private int cpu1;
        private int cpu2;
        private int cpu3;
        private int cpu4;
        private int cpuTotal;

        //dysk 
        private int diskU; // użycie dysku
        private double diskRead; // predkosc odczytu danych
        private double diskWrite; //predkosc zapisywania danych

        private double networkSent; //predkosc wysyłania danych
        private double networkReceived; // predkosc odbierania danych

        private int cpuL; // aktualne zużycie procesora

        private int maxcpu; //zmienna pomocnicza
        private int maxgpu; // zmienna pomocnicza

        private long diskA1; //aktualnie dostepna pamiec dysku 1
        private long diskT1; //calkowita pamiec dysku 1
        private long diskA2; //aktualnie dostepna pamiec dysku 2
        private long diskT2; //calkowita pamiec dysku 2

        private int cntcpul; //licznik ostrzezen zuzycia procesora
        private int cntgpul; //licznik ostrzezen zuzycia karty graficznej
        private int cntcput; //licznik ostrzezen temperatury procesora
        private int cntgput; //licznik ostrzezen temperatury karty graficznej

        private decimal ramPer; // aktulne zuzycie ram w procentach
        private decimal URAM1; // aktualne wykorzystanie RAM
        private decimal ARAM1; // aktualnie dostepna pamiec ram
       
        //po przekreczeniu tych wartosci zostanie wygenerowane ostrzezenie
        private int ifcpul = 80; 
        private int ifcputemp = 80;
        private int ifgpul = 80;
        private int ifgpuTemp = 80;

        //zmienne używane do połaczenia się z aplikacja mobilną
        private int port = 1234;
        private string address;
        

        private int _gpuTemp; // temperatura karty graficznej
        private int gpuLoad; // zuzycie karty graficznej
       

        public Form1()
        {
            InitializeComponent();
            File.WriteAllText(filepath, String.Empty);

            computer.Open();

        }
   
        //fukcje wywolane w timerze wyswietlaja sie co 1,5sec
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            cpu_gpu_ram();
            systemInfo();
           
            progress();
            RealTimeChart();
            max();
            diskspace();


        }

      // pobieranie inforamcji o CPU,GPU,RAM za pomoca bilioteki hardware monitor
        private void cpu_gpu_ram()
        {
            counter = 0;
            counter2 = 0;
            counter3 = 0;

            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();

                if (counter2 == 0)
                {
                   
                    cpuPanel.Text = hardware.Name.ToString();
                }
                if (counter2 == 2)
                {
                    if (_gpuTemp == 0)
                    {
                        gpuPanel.ForeColor = Color.Red;
                        gpuPanel.Text = "Karta graficzna nie jest w tej chwili używana";

                    }
                    else
                    {
                        gpuPanel.ForeColor = Color.Black;
                        gpuPanel.Text = hardware.Name.ToString();
                    }
                }
                if (counter2 == 1)
                {
                    ramPanel.Text = hardware.Name.ToString();
                }

         

                counter2++;

                 foreach (ISensor sensor in hardware.Sensors)
                 {
                     if (sensor.SensorType == SensorType.Temperature)
                     {
                         if (counter == 0)
                         {

                            c1.Text =  sensor.Value.ToString() + " .C";
                            core1.Text = sensor.Name.ToString();
                            cpu1 = (int)sensor.Value;

                         }
                         if (counter == 1)
                         {
                             c2.Text = sensor.Value.ToString() + " .C";
                             core2.Text = sensor.Name.ToString();
                            cpu2 = (int)sensor.Value;
                         }
                         if (counter == 2)
                         {
                             c3.Text =sensor.Value.ToString() + " .C";
                             core3.Text = sensor.Name.ToString();
                            cpu3 = (int)sensor.Value;
                         }
                         if (counter == 3)
                         {
                             c4.Text =sensor.Value.ToString() + " .C";
                             core4.Text = sensor.Name.ToString();
                             cpu4 = (int)sensor.Value;
                         }
                         if (counter == 4)
                         {
                             buffer = sensor.Name.ToString() + "   " + sensor.Value.ToString();
                         }
                         if (counter == 5)
                         {
                             gpuTemp.Text = sensor.Value.ToString() + " .C";
                             _gpuTemp = (int)sensor.Value;
                         }

                         counter++;
                     }
                    cpuTotal = (cpu1 + cpu2 + cpu3 + cpu4) / 4;

                    coreAve.Text = cpuTotal.ToString() + " .C";
                   

        

                    if (sensor.SensorType == SensorType.Load)
                    {
                        if (counter3 == 6)
                        {
                            gpuLoad = Convert.ToInt32(sensor.Value);
                           gpul.Text = gpuLoad.ToString() + "%";


                        }



                        counter3++;
                    }
                    if (sensor.SensorType == SensorType.Data)
                    {
                        if (counter4 == 0)
                        {
                            URAM1 = Convert.ToDecimal(sensor.Value);
                            URAM1 = Math.Round(URAM1, 2);

                            URAM.Text = URAM1.ToString() + "GB";


                        }
                        if (counter4 == 1)
                        {
                            ARAM1 = Convert.ToDecimal(sensor.Value);
                            ARAM1 = Math.Round(ARAM1, 2);
                            ARAM.Text = ARAM1.ToString() + "GB";
           

                        }
                        ramPer = Math.Round(( URAM1 / (URAM1+ARAM1)) * 100,0) ;
                        ramUsage.Text = ramPer.ToString() + " %";
 
                        counter4++;


                    }



                }
             }
         }



         // pobieranie informacji o dysku, karcie sieciowej oraz zuzycia CPU
         private void systemInfo()
         {
            
            diskU = (int)PerfDiskUsage.NextValue() ;
            diskUsage.Text = diskU.ToString() + " %";

            diskRead =Math.Round( (float)PerfDiskRead.NextValue() / 1000000, 1) ;
            diskR.Text = diskRead.ToString() + "MB/s";
            diskWrite = Math.Round((float)PerfDiskWrite.NextValue()/ 1000000, 1)  ;
            diskW.Text = diskWrite.ToString() + "MB/s";

            networkSent = Math.Round(PerfNetworkSent.NextValue()/ 125000, 1);
            NetSent.Text = networkSent.ToString() + "Mb/s";
            networkReceived = Math.Round(PerfNetworkReceived.NextValue()/ 125000, 1);
            NetRec.Text = networkReceived.ToString() + "Mb/s";

            cpuL = (int)PerfCPU.NextValue();
            cpuLoad.Text = cpuL.ToString() + " %";



        }

        //przypisywanie zmiennych do paska postepu
        private void progress()
        {


            gpuTempBar.Value = _gpuTemp;
            Core1Bar.Value = cpu1;
            Core2Bar.Value = cpu2;
            Core3Bar.Value = cpu3;
            Core4Bar.Value = cpu4;
            CPUBar.Value = cpuTotal;
            gpuPerBar.Value = gpuLoad;
            CPULoadBar.Value = cpuL;
            Disk1pb.Maximum = Convert.ToInt32( diskT1);
            Disk1pb.Value = Convert.ToInt32(diskT1-diskA1);
            Disk2pb.Maximum = Convert.ToInt32(diskT2);
            Disk2pb.Value = Convert.ToInt32(diskT2 - diskA2);


    }


        // wykres czasu rzeczywistego
        private void RealTimeChart()
        {
            chartDisk.Series["Zapis Dysku"].Points.AddXY(DateTime.Now.ToLongTimeString(), diskWrite);
            chartDisk.Series["Odczyt Dysku"].Points.AddXY(DateTime.Now.ToLongTimeString(), diskRead);
     
            chartNet.Series["Wysyłanie Danych"].Points.AddXY(DateTime.Now.ToLongTimeString(), networkSent);
            chartNet.Series["Odbieranie Danych"].Points.AddXY(DateTime.Now.ToLongTimeString(), networkReceived);
        
            chartRAM.Series["zużycie RAM"].Points.AddXY(DateTime.Now.ToLongTimeString(), ramPer);

         
            chartCPU.Series["Zużycie CPU"].Points.AddXY(DateTime.Now.ToLongTimeString(), cpuL);
            chartCPU.Series["Temperatura CPU"].Points.AddXY(DateTime.Now.ToLongTimeString(), cpuTotal);
       
            GPUchart.Series["Zużycie GPU"].Points.AddXY(DateTime.Now.ToLongTimeString(), gpuLoad);
            GPUchart.Series["Temperatura GPU"].Points.AddXY(DateTime.Now.ToLongTimeString(), _gpuTemp);
        

        }

        //zapisywanie ostrzezen do pliku
        private void writetofile()
        {

           
            
            List<string> cpulines = new List<string>();
            List<string> cputlines = new List<string>();
            List<string> gpulines = new List<string>();
            List<string> gputlines = new List<string>();
            if (cpuL > ifcpul)
            {
                cpulines.Add(DateTime.Now + " uwaga: zużycie CPU ponad " + ifcpul + ": " + cpuL.ToString() + " %");
                File.AppendAllLines(filepath,  cpulines);
                warning.Text = File.ReadAllText(filepath);

                cntcpul = cntcpul + 1;
                
            }
            countcpu.Text = cntcpul.ToString();
            if(cpuTotal > ifcputemp)
            {
                
                cputlines.Add(DateTime.Now + " uwaga: temperatura CPU ponad " + ifcputemp + ": " + cpuTotal.ToString() +" .C");
                File.AppendAllLines(filepath, cputlines);
               

               warning.Text = File.ReadAllText(filepath);
                cntcput = cntcput + 1;

            }
            if(gpuLoad > ifgpul)
            {
                gpulines.Add(DateTime.Now + " uwaga: zużycie GPU ponad " + ifgpul + ": " + gpuLoad.ToString() + " %");
                File.AppendAllLines(filepath, gpulines);


                warning.Text = File.ReadAllText(filepath);
                cntgpul = cntgpul + 1;
            }
            if (_gpuTemp > ifgpuTemp)
            {
                gputlines.Add(DateTime.Now + " uwaga: zużycie GPU ponad " + ifgpuTemp + ": " + _gpuTemp.ToString() + " .C");
                File.AppendAllLines(filepath, gputlines);


                warning.Text = File.ReadAllText(filepath);
                cntgput = cntgput + 1;
            }

            countcpu.Text = "licznik ostrzeżeń zużycia CPU: " + cntcpul.ToString();
            countcput.Text = "licznik ostrzeżeń temperatury CPU: " + cntcput.ToString();
            countgpu.Text = "licznik ostrzeżeń zużycia GPU: " + cntgpul.ToString();
            countgput.Text = "licznik ostrzeżeń temperatury CPU: " + cntgput.ToString();


            
        }
        //wysylanie informacji o podzespołach do serwera na telefonie za pomocą protokolu UDP 
        private async void client()
        {           
            var client = new UdpSocketClient();
            var msg = Environment.NewLine + Environment.NewLine + Environment.NewLine + "CPU: " + cpuL.ToString() + " %" + Environment.NewLine + "Temp: " + cpuTotal.ToString() + " .C" + Environment.NewLine + Environment.NewLine + Environment.NewLine + "GPU: " + gpuLoad.ToString()+" %" + Environment.NewLine + "Temp: " + _gpuTemp.ToString() + " .C"; 
            var msgBytes = Encoding.UTF8.GetBytes(msg);            
            IPAddress ip;            
            bool walidacjaip = IPAddress.TryParse(address, out ip);
            if (walidacjaip && (address.Contains(".") == true))
            {
                await client.SendToAsync(msgBytes, address, port);
            }
            else
            {
                MessageBox.Show("Nieprawidłowy format adresu IP", "Uwaga");
            }

        }

        //krytyczne temperatury procesora i karty graficznej
        private void max ()
        {
            if (cpuTotal > 80)
            {
                maxcpu = cpuTotal;
            }
            if (_gpuTemp > 80)
            {
                maxgpu = _gpuTemp;
            }

            if (maxcpu > 90)
            {
                uwagacpu.Text = "Zbyt wysoka temperatura CPU wyczyść chłodzenie i wymień pastę termiczną";
            }
            if (maxgpu > 90)
            {
                uwagagpu.Text = "Zbyt wysoka temperatura GPU wyczyść chłodzenie i wymień pastę termiczną";
            }


        }


        // dostepna przestrzen na dysku
        private void diskspace()
        {
            counter5 = 0;
            foreach (DriveInfo disk in drives)
            {
                
                if(counter5 == 0)
                {
                    diskname1.Text = disk.Name;

                }
                if(counter5 == 1)
                {
                    diskname2.Text = disk.Name;
                }

                if(counter5 ==0)
                {
                    diskA1 = disk.AvailableFreeSpace / 1073741824 ;
                    diskT1 = disk.TotalSize / 1073741824;
                    disksize1.Text = diskA1.ToString() + " GB wolnych z " + diskT1.ToString() + " GB";
                }
                if (counter5 == 1)
                {
                    diskA2 = disk.AvailableFreeSpace / 1073741824;
                    diskT2 = disk.TotalSize / 1073741824;
                    disksize2.Text = diskA2.ToString() + " GB wolnych z " + diskT2.ToString() + " GB";
                }
                counter5++;

            }
            
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void gpuPanel_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

  

        private void Change_Click(object sender, EventArgs e)
        {
            

        }

        //przypisywanie wartosci ustaloncyh przez użytkownika oraz rozpoczecie zapisywanie do pliku
        private void Start_Click(object sender, EventArgs e)
        {

            int value;
            if (int.TryParse(txtifcpul.Text, out value) && int.TryParse(txtgpul.Text, out value) && int.TryParse(txtcputotal.Text, out value) && int.TryParse(txtgputemp.Text, out value))
            {
                if (Convert.ToInt32(txtifcpul.Text) < 101 && Convert.ToInt32(txtcputotal.Text) < 101 && Convert.ToInt32(txtgpul.Text) < 101 && Convert.ToInt32(txtgputemp.Text) < 101)
                {
                    ifcpul = Convert.ToInt32(txtifcpul.Text);
                    ifcputemp = Convert.ToInt32(txtcputotal.Text);
                    ifgpul = Convert.ToInt32(txtgpul.Text);
                    ifgpuTemp = Convert.ToInt32(txtgputemp.Text);
                    timer2.Start();//rozpoczecie zapisywania
                }
                else
                {
                    MessageBox.Show("nieprawidłowy format proszę podać liczbę od 0-100", "Uwaga");
                }
                
            }
            else
            {
                MessageBox.Show("nieprawidłowy format proszę podać liczbę od 0-100","Uwaga");
            }
       

            
        }

        //zapisywanie do pliku wywolane w timerze
        private void timer2_Tick(object sender, EventArgs e)
        {
            writetofile();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            timer2.Stop();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            client();
        }



        private void Set_Click(object sender, EventArgs e)
        {
            
        }

        //rozpoczecie wysylanie informacji o podzespolach do serwera na telefonie
        private void Send_Click(object sender, EventArgs e)
        {

            IPAddress ip;


            address = IPadress.Text;
            bool walidacjaip = IPAddress.TryParse(address, out ip);
            if (walidacjaip && (address.Contains(".") == true))
            {

            }
            else
            {
                MessageBox.Show("Nieprawidłowy format adresu IP", "Uwaga");
            }

            timer3.Start();
        }

        private void StopSend_Click(object sender, EventArgs e)
        {
            timer3.Stop();
        }

        //zapisywanie wykresu jako obraz
        private void button1_Click(object sender, EventArgs e)

        {
            SaveFileDialog zapiszdopliku = new SaveFileDialog();
            zapiszdopliku.DefaultExt = "png";
            if (zapiszdopliku.ShowDialog() == DialogResult.OK)
            {


                this.chartCPU.SaveImage(zapiszdopliku.FileName, System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
            }
        }
        // wybor sciezki zapisu ostrzezen
        private void wyb_Click(object sender, EventArgs e)
        {
            SaveFileDialog zapiszdopliku = new SaveFileDialog();
            zapiszdopliku.DefaultExt = "txt";
            if (zapiszdopliku.ShowDialog() == DialogResult.OK)
            {
                filepath = zapiszdopliku.FileName;
             
            }
        }
    }
   
}
    

