using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
//##################//
using Android;
using Android.Locations;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace EmergencyApp
{
	[Activity (Label = "EmergencyApp", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, ILocationListener
	{
		Location currentLocation;
		LocationManager locationManager;
		TextView locationText;
		TextView addressText;
		String locationProvider;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Main);

			addressText = FindViewById<TextView>(Resource.Id.address_text);
			locationText = FindViewById<TextView>(Resource.Id.location_text);
			FindViewById<TextView>(Resource.Id.get_address_button).Click += AddressButton_OnClick;

			InitializeLocationManager();
		}

		void InitializeLocationManager()
		{
			locationManager = (LocationManager)GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Fine
			};

			IList<string> acceptableLocationProviders = locationManager.GetProviders(criteriaForLocationService, true);

			if (acceptableLocationProviders.Any())
			{
				locationProvider = acceptableLocationProviders.First();
			}
			else
			{
				locationProvider = String.Empty;
			}
		}
			
		public void OnProviderDisabled(string provider) {}

		public void OnProviderEnabled(string provider) {}

		public void OnStatusChanged(string provider, Availability status, Bundle extras) {}

		protected override void OnResume()
		{
			base.OnResume();
			locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
		}

		protected override void OnPause()
		{
			base.OnPause();
			locationManager.RemoveUpdates(this);
		}

		async void AddressButton_OnClick(object sender, EventArgs eventArgs)
		{
			if (currentLocation == null)
			{
				addressText.Text = "Can't determine the current address.";
				return;
			}

			Geocoder geocoder = new Geocoder(this);
			IList<Address> addressList = await geocoder.GetFromLocationAsync(currentLocation.Latitude, currentLocation.Longitude, 10);

			Address address = addressList.FirstOrDefault();
			if (address != null)
			{
				StringBuilder deviceAddress = new StringBuilder();
				for (int i = 0; i < address.MaxAddressLineIndex; i++)
				{
					deviceAddress.Append(address.GetAddressLine(i))
						.AppendLine(",");
				}
				addressText.Text = deviceAddress.ToString();
			}
			else
			{
				addressText.Text = "Unable to determine the address.";
			}
		}

		public void OnLocationChanged(Location location)
		{
			currentLocation = location;
			if (currentLocation == null)
			{
				locationText.Text = "Unable to determine your location.";
			}
			else
			{
				locationText.Text = String.Format("{0},{1}", currentLocation.Latitude, currentLocation.Longitude);
			}
		}
	}
}


