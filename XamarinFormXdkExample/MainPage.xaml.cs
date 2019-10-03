using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormXdkExample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private MOLPay molpay = null;
        private StackLayout mainLayout = null;

        public MainPage()
        {
            InitializeComponent();

            var closeButton = new Button();
            closeButton.BackgroundColor = Color.Gray;
            closeButton.Text = "Close";
            closeButton.CornerRadius = 0;
            closeButton.TextColor = Color.White;
            closeButton.VerticalOptions = LayoutOptions.FillAndExpand;
            closeButton.HorizontalOptions = LayoutOptions.FillAndExpand;
            closeButton.Clicked += OnCloseButtonClicked;

            var buttonLayout = new StackLayout
            {
                BackgroundColor = Color.White,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                Spacing = 0,
                HeightRequest = 40,
                Children = {
                    closeButton
                }
            };

            this.mainLayout = new StackLayout
            {
                BackgroundColor = Color.Blue,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            this.mainLayout.Children.Add(buttonLayout);

            var paymentDetails = new Dictionary<string, object>
            {
				// ------- SDK required data ----------
				{ "mp_amount", "5.10" }, // Mandatory String. A value more than '1.00'
				{ "mp_username", "api_eboss" }, // Mandatory String. Values obtained from MOLPay
				{ "mp_password", "api_sSo1304B" }, // Mandatory String. Values obtained from MOLPay
				{ "mp_merchant_ID", "eboss_Dev" }, // Mandatory String. Values obtained from MOLPay
				{ "mp_app_name", "eboss" }, // Mandatory String. Values obtained from MOLPay
				{ "mp_order_ID", "123123123" }, // Mandatory String. Payment values
				{ "mp_currency", "MYR" }, // Mandatory String. Payment values
				{ "mp_country", "MY" }, // Mandatory String. Payment values
				{ "mp_verification_key", "8e09adede0fe24b65ce9cd54c35adc04" }, // Mandatory String. Values obtained from MOLPay
				{ "mp_channel", "fpx" }, // Optional String.
				{ "mp_bill_name", "billname" }, // Optional String.
				{ "mp_bill_email", "example@email.com" }, // Optional String.
				{ "mp_bill_mobile", "+60123456789" }, // Optional String.
				{ "mp_bill_description", "billdesc" } // Optional String.
			};

            this.molpay = new MOLPay(DependencyService.Get<MOLPayExtension>().GetAssetPath(), paymentDetails, MolpayCallback);

            this.mainLayout.Children.Add(this.molpay);

            // The root page of your application
            //MainPage = new ContentPage
            //{
            //    Content = this.mainLayout
            //};

            Content = this.mainLayout;
        }


        private void MolpayCallback(string transactionResult)
        {
            System.Diagnostics.Debug.WriteLine("MOLPayXDK Debug OK >>>>>>>> molpayCallback, transactionResult = {0}", transactionResult);
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            this.molpay.CloseMolpay();
        }
    }
}
    