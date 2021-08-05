using Sockets.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace MonitorZasobow
{
    public partial class MainPage : ContentPage
    {
        
        //wyswietlanie aktualnego ip telefonu
        public MainPage()
        {
            InitializeComponent();
            string IP = Dns.GetHostName();
            string IPadress = Dns.GetHostByName(IP).AddressList[0].ToString();
            ip.Text = IPadress;
        }

        //przejscie do kolejnego okna
        private void Start_Clicked(object sender, EventArgs e)
        {

            App.Current.MainPage = new StartPage();
        }
    }
}
