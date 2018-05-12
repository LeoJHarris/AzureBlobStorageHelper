using FFImageLoading;
using System;
using Xamarin.Forms;

namespace SampleApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void clickedevent(object sender, EventArgs e)
        {
            var stream = await ImageService.Instance.LoadUrl("https://cdn4.iconfinder.com/data/icons/iconsimple-logotypes/512/github-128.png").AsPNGStreamAsync();

            Guid guid = Guid.NewGuid();

            Uri uri = await LeoJHarris.XForms.Plugin.BlobStorageHelper.CrossBlobStorageHelper.Current.UploadBlob("#YOUR CONTAINER NAME ON AZURE#", guid, "#CONNECTION STRING#", stream);
        }
    }
}
