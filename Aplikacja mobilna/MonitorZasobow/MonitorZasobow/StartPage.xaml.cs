using System;
using Sockets.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Net;

namespace MonitorZasobow
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
            label.Text = Environment.NewLine + Environment.NewLine + "Czekam na połączenie";





            Task.Run(() => server());
        }

        //odbieranie informacji od komputera i wyswietlanie na ekranie telefonu
        private async void server()
        {
            var listenPort = 1234;
            var receiver = new UdpSocketReceiver();
            receiver.MessageReceived += (sender, args) =>
            {
                var from = String.Format("{0}:{1}", args.RemoteAddress, args.RemotePort);
                var data = Encoding.UTF8.GetString(args.ByteData, 0, args.ByteData.Length);

                Device.BeginInvokeOnMainThread(() =>
                {
                    label.Text = data;
                });
            };


            await receiver.StartListeningAsync(listenPort);
        }
    }
}