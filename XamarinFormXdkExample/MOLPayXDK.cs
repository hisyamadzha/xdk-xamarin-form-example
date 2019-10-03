using System;

using Xamarin.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace XamarinFormXdkExample //Update to your project namespace accordingly
{
	public interface MOLPayExtension
	{
		String GetAssetPath();
		void saveImageToDevice(String base64ImageString, String filename);
		void SetMOLPayContext(Object mpActivity);
	}

	public class MOLPay : AbsoluteLayout
	{ 
		// deploy
		private bool isInternalDebugging = false;
		private const string moduleId = "molpay-mobile-xdk-xamarin";
		private const string wrapperVersion = "0";
		private const string molpaySdkUrl = "molpay-mobile-xdk-www/index.html";
		private const string mpopenmolpaywindow = "mpopenmolpaywindow://";
		private const string mptransactionresults = "mptransactionresults://";
		private const string mpcloseallwindows = "mpcloseallwindows://";
		private const string mprunscriptonpopup = "mprunscriptonpopup://";
		private const string mppinstructioncapture = "mppinstructioncapture://";
		private const string mpopenbankwindow = "mpopenbankwindow://";
		private const string molpaynbepayurl = "MOLPay/nbepay.php";

		private Dictionary<string, object> molpayPaymentDetails = null;
		private Action<string> TransactionResultCallback = null;
		private WebView mainUI = null;
		private WebView molpayUI = null;
		private bool isClosingMolpay = false;

		public MOLPay (String assetsPath, Dictionary<string, object> paymentDetails, Action<string> MolpayCallback)
		{
			this.TransactionResultCallback = MolpayCallback;

			if (assetsPath.Length > 0)
			{
				this.molpayPaymentDetails = paymentDetails;
				this.molpayPaymentDetails.Add("module_id", moduleId);
				this.molpayPaymentDetails.Add("wrapper_version", wrapperVersion);

				System.Diagnostics.Debug.WriteLine("MOLPayXDK assetsPath OK >>>>>>>> {0}", assetsPath);

				HorizontalOptions = LayoutOptions.FillAndExpand;
				VerticalOptions = LayoutOptions.FillAndExpand;

				this.mainUI = new WebView ();

				this.mainUI.Source = new UrlWebViewSource {
					Url = System.IO.Path.Combine (assetsPath, molpaySdkUrl)
				};

				AbsoluteLayout.SetLayoutBounds (mainUI, new Rectangle (0, 0, 1, 1));
				AbsoluteLayout.SetLayoutFlags (mainUI, AbsoluteLayoutFlags.All);

				this.mainUI.Navigating += OnMainUILoadBegin;
				this.mainUI.Navigated += OnMainUILoadFinish;

				Children.Add (this.mainUI);
			} 
			else {
				System.Diagnostics.Debug.WriteLine("MOLPayXDK Error >>>>>>>> assetsPath not found");
			}
		}

		public void CloseMolpay()
		{
			this.mainUI.Eval("closemolpay();");
		}

		// MolpayUI handlers
		private void OnMolpayUILoadBegin(object sender, WebNavigatingEventArgs e)
		{
			if (isInternalDebugging)
			{
				//System.Diagnostics.Debug.WriteLine("+++++++++++ onMainUILoadBegin, sender = {0}, e.NavigationEvent = {1}, e.Source = {2}, e.Url = {3}", sender, e.NavigationEvent, e.Source, e.Url);
				System.Diagnostics.Debug.WriteLine("+++++++++++ OnMolpayUILoadFinish, e.Url = {0}", e.Url);
			}

			string url = e.Url;

			if (url != null)
			{
				// nativeWebRequestUrlUpdates
				var nativeWebRequestUrlUpdatesDict = new Dictionary<string, object>
				{
					{ "requestPath", url }
				};
				string nativeWebRequestUrlUpdatesJSON = Newtonsoft.Json.JsonConvert.SerializeObject(nativeWebRequestUrlUpdatesDict);

				this.mainUI.Eval(string.Format("nativeWebRequestUrlUpdates({0})", nativeWebRequestUrlUpdatesJSON));
			}
			
		}

		private void OnMolpayUILoadFinish(object sender, WebNavigatedEventArgs e)
		{
			if (isInternalDebugging)
			{
				//System.Diagnostics.Debug.WriteLine("+++++++++++ onMainUILoadBegin, sender = {0}, e.NavigationEvent = {1}, e.Source = {2}, e.Url = {3}", sender, e.NavigationEvent, e.Source, e.Url);
				System.Diagnostics.Debug.WriteLine("+++++++++++ OnMolpayUILoadFinish, e.Url = {0}", e.Url);
			}

			string url = e.Url;

			if (url.Contains(molpaynbepayurl))
			{
				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ molpaynbepayurl found");
				}
				// Override popup
				string evaljs = "window.open = function (open) { return function (url, name, features) { window.location = url ; return window; };  } (window.open);";
				this.molpayUI.Eval(evaljs);
			}
		}

		// MainUI handlers
		private void OnMainUILoadBegin (object sender, WebNavigatingEventArgs e) 
		{
			if (isInternalDebugging) {
				System.Diagnostics.Debug.WriteLine("+++++++++++ OnMainUILoadBegin, e.Url = {0}", e.Url);
			}

			string url = e.Url;

			if (url.Contains(mpopenmolpaywindow))
			{
				// Must stop loading
				e.Cancel = true;

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mpopenmolpaywindow found");
				}

				this.molpayUI = new WebView();

				url = url.Replace(mpopenmolpaywindow, "");

				if (Device.OS == TargetPlatform.iOS)
				{
					url = url.Replace("-", "+");
					url = url.Replace("_", "=");
				}

				string htmlString = Base64Decode(url);

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mpopenmolpaywindow htmlString = {0}", htmlString);
				}

				this.molpayUI.Source = new HtmlWebViewSource
				{
					Html = htmlString
				};

				AbsoluteLayout.SetLayoutBounds(molpayUI, new Rectangle(0, 0, 1, 1));
				AbsoluteLayout.SetLayoutFlags(molpayUI, AbsoluteLayoutFlags.All);

				this.molpayUI.Navigating += OnMolpayUILoadBegin;
				this.molpayUI.Navigated += OnMolpayUILoadFinish;

				Children.Add(this.molpayUI);
			}

			else if (url.Contains(mptransactionresults))
			{
				// Must stop loading
				e.Cancel = true;

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mptransactionresults found");
				}

				url = url.Replace(mptransactionresults, "");

				if (Device.OS == TargetPlatform.iOS)
				{
					url = url.Replace("-", "+");
					url = url.Replace("_", "=");
				}

				string resultString = Base64Decode(url);

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mptransactionresults resultString = {0}", resultString);
				}

				this.TransactionResultCallback(resultString);

				// CLosing Molpay
				if (this.isClosingMolpay)
				{
					if (this.molpayUI != null)
					{
						Children.Remove(this.molpayUI);
					}
					this.isClosingMolpay = false;
				}
			}

			else if (url.Contains(mpcloseallwindows))
			{
				// Must stop loading
				e.Cancel = true;

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mpcloseallwindows found");
				}

				if (this.molpayUI != null)
				{
					Children.Remove(this.molpayUI);
				}
			}

			else if (url.Contains(mprunscriptonpopup))
			{
				// Must stop loading
				e.Cancel = true;

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mprunscriptonpopup found");
				}

				url = url.Replace(mprunscriptonpopup, "");

				if (Device.OS == TargetPlatform.iOS)
				{
					url = url.Replace("-", "+");
					url = url.Replace("_", "=");
				}

				string evaljs = Base64Decode(url);

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mprunscriptonpopup evaljs = {0}", evaljs);
				}

				this.molpayUI.IsVisible = false;
				this.molpayUI.Eval(evaljs);
			}

			else if (url.Contains(mppinstructioncapture))
			{
				// Must stop loading
				e.Cancel = true;

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mppinstructioncapture found");
				}

				url = url.Replace(mppinstructioncapture, "");

				if (Device.OS == TargetPlatform.iOS)
				{
					url = url.Replace("-", "+");
					url = url.Replace("_", "=");
				}

				string dataString = Base64Decode(url);

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mppinstructioncapture dataString = {0}", dataString);
				}

				var dataDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>> (dataString);
				
				System.Diagnostics.Debug.WriteLine ("+++++++++++ mppinstructioncapture, dataDict = {0}", dataDict);

				string base64ImageString;
				if (!dataDict.TryGetValue("base64ImageUrlData", out base64ImageString))
				{
					return;
				}

				string filename;
				if (!dataDict.TryGetValue("filename", out filename))
				{
					return;
				}

				DependencyService.Get<MOLPayExtension>().saveImageToDevice(base64ImageString, filename);
			}

			else if (url.Contains(mpopenbankwindow))
			{
				// Must stop loading
				e.Cancel = true;

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mpopenbankwindow found");
				}

				this.molpayUI = new WebView();

				url = url.Replace(mpopenbankwindow, "");

				if (Device.OS == TargetPlatform.iOS)
				{
					url = url.Replace("-", "+");
					url = url.Replace("_", "=");
				}

				string urlString = Base64Decode(url);

				if (isInternalDebugging)
				{
					System.Diagnostics.Debug.WriteLine("+++++++++++ mpopenbankwindow urlString = {0}", urlString);
				}

				this.molpayUI.Source = new UrlWebViewSource
				{
					Url = urlString
				};

				AbsoluteLayout.SetLayoutBounds(molpayUI, new Rectangle(0, 0, 1, 1));
				AbsoluteLayout.SetLayoutFlags(molpayUI, AbsoluteLayoutFlags.All);

				this.molpayUI.Navigating += OnMolpayUILoadBegin;
				this.molpayUI.Navigated += OnMolpayUILoadFinish;

				Children.Add(this.molpayUI);
			}

			//var _results = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>> (paymentDetailsJSON);
			//
			//_MolpayCallback (paymentDetailsJSON);
			//
			//System.Diagnostics.Debug.WriteLine ("+++++++++++ onMainUILoadFinish, paymentDetailsJSON = {0}", paymentDetailsJSON);
			//System.Diagnostics.Debug.WriteLine ("+++++++++++ onMainUILoadFinish, _results = {0}", _results);
		}

		private void OnMainUILoadFinish(object sender, WebNavigatedEventArgs e)
		{
			string paymentDetailsJSON = Newtonsoft.Json.JsonConvert.SerializeObject(molpayPaymentDetails);

			if (isInternalDebugging)
			{
				System.Diagnostics.Debug.WriteLine("+++++++++++ onMainUILoadFinish, sender = {0}, e.NavigationEvent = {1}, e.Source = {2}, e.Url = {3}, e.Result = {4}", sender, e.NavigationEvent, e.Source, e.Url, e.Result);
				System.Diagnostics.Debug.WriteLine("+++++++++++ onMainUILoadFinish, paymentDetails = {0}", paymentDetailsJSON);
			}

			this.mainUI.Eval(string.Format("updateSdkData({0})", paymentDetailsJSON));
			this.mainUI.Navigated -= OnMainUILoadFinish;
		}


		// Utilities
		private static string Base64Decode(string base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes, 0, base64EncodedBytes.Length);
		}

		private static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}
			
	}
}


