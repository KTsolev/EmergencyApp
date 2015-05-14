using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
//##################//
using Android;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net;

namespace EmergencyApp
{
	[Activity (Label = "EmergencyApp", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, ILocationListener
	{
		private Location currentLocation;
        private Location prevLocation;
        private LocationManager locationManager;
        private TextView locationText;
        private TextView addressText;
        private String locationProvider;
        private LatLng location;
		readonly double eps = 0.001D;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Main);

			addressText = FindViewById<TextView>(Resource.Id.address_text);
			locationText = FindViewById<TextView>(Resource.Id.location_text);
			FindViewById<TextView>(Resource.Id.get_address_button).Click += AddressButton_OnClick;
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var mobileState = connectivityManager.GetNetworkInfo(ConnectivityType.Mobile).GetState();
            if (mobileState == NetworkInfo.State.Connected)
            {
                InitializeLocationManager();        
            }
            else
            {
                IvokeErrorHandler("There is no internet connection");
            }		
		}

		void InitializeLocationManager()
		{
			locationManager = (LocationManager)GetSystemService(LocationService);
			Criteria criteriaForLocationService = new Criteria
			{
				Accuracy = Accuracy.Medium
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
                IvokeErrorHandler("Can't determine the current address.");
				addressText.Text = "Can't determine the current address.";
				return;
			}

			Geocoder geocoder = new Geocoder(this);
			IList<Address> addressList = await geocoder.GetFromLocationAsync(currentLocation.Latitude, currentLocation.Longitude, 10);
			prevLocation = currentLocation;
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
                IvokeErrorHandler("Unable to determine the address.");
			}	
            InitializeMap ();
		}

		public void OnLocationChanged(Location location)
		{
			currentLocation = location;

			if (currentLocation == null)
			{
				locationText.Text = "Unable to determine your location.";
                IvokeErrorHandler("Unable to determine your location.");
			}
			else
			{
				locationText.Text = String.Format ("{0},{1}", currentLocation.Latitude, currentLocation.Longitude);
                prevLocation = currentLocation;
                InitializeMap();
			}
		}

		public void InitializeMap()
		{
			if (currentLocation != null) 
			{
				location = new LatLng (currentLocation.Latitude, currentLocation.Longitude);
			}
			else 
			{
				addressText.Text = "Can't load map!";
                IvokeErrorHandler("Can't load map!");

			}

            if (Math.Abs(prevLocation.Latitude - currentLocation.Latitude) < eps && Math.Abs(prevLocation.Longitude - currentLocation.Longitude) < eps)
            {
                prevLocation = currentLocation;

                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();

                builder.Target(location);
                builder.Zoom(18);
                builder.Bearing(155);
                builder.Tilt(65);

                MapFragment mapFrag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
                GoogleMap map = mapFrag.Map;
                CameraPosition cameraPosition = builder.Build();
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

                if (map != null)
                {
                    map.MoveCamera(cameraUpdate);
                    map.MapType = GoogleMap.MapTypeHybrid;
                    map.UiSettings.ZoomControlsEnabled = true;
                    map.UiSettings.CompassEnabled = true;
                    MarkerOptions markerOpt1 = new MarkerOptions();
                    markerOpt1.SetPosition(location);
                    markerOpt1.SetTitle("You are here!");
                    map.AddMarker(markerOpt1);
                }
                else
                {
                    addressText.Text = "Can't load map!";
                    IvokeErrorHandler("Can't load map!");
                }
            }
		}

        public void IvokeErrorHandler(string error)
        {
            var activity2 = new Intent (this, typeof(ErrorHandler));
            activity2.PutExtra ("error", error);
            StartActivity (activity2);
        }
	}
}


