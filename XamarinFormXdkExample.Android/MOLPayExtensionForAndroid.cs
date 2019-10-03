using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.Util;
using System.IO;
using Android.Media;

using XamarinFormXdkExample.Droid;
using XamarinFormXdkExample;

[assembly: Xamarin.Forms.Dependency(typeof(MPExtension))]

namespace XamarinFormXdkExample.Droid
{
	public class MPExtension : MOLPayExtension
	{
		private static Object molpayActivity;

		public void SetMOLPayContext(Object mpActivity)
		{
			molpayActivity = mpActivity;
		}

		public String GetAssetPath()
		{
			return "file:///android_asset/";
		}

		public void saveImageToDevice(String base64ImageString, String filename)
		{
			System.Diagnostics.Debug.WriteLine("+++++++++++ saveImageToDevice, base64ImageString = {0}", base64ImageString);
			System.Diagnostics.Debug.WriteLine("+++++++++++ saveImageToDevice, filename = {0}", filename);

			byte[] imageAsBytes = Base64.Decode(base64ImageString, Base64Flags.Default);
            Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);

			var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
			var filePath = System.IO.Path.Combine(sdCardPath.ToString(), filename);
			var stream = new FileStream(filePath, FileMode.Create);

			bool compress = bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
			stream.Close();

			MediaScannerConnection.ScanFile(Application.Context, new String[] { filePath }, null, null);

			if (compress)
			{
				Toast.MakeText((MainActivity)molpayActivity, "Image saved", ToastLength.Short).Show();
			}
			else
			{
				Toast.MakeText((MainActivity)molpayActivity, "Image not saved", ToastLength.Short).Show();
			}
		}

		private static String Base64Encode(String plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		private static String Base64Decode(String base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}
	}
}
