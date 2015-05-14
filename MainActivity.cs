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
using Android.Util;

namespace EmergencyApp
{
	[Activity (Label = "EmergencyApp", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, ILocationListener
	{
		private Location currentLocation;
        private Location prevLocation;
        private LocationManager locationManager;
        private TextView locationText;
        private String locationProvider;
        private LatLng location;
        private GoogleMap map;
        private MapFragment mapFrag;
		readonly double eps = 0.0001D;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Main);

			locationText = FindViewById<TextView>(Resource.Id.location_text);

            mapFrag = (MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
            map = mapFrag.Map;

            InitializeLocationManager();
            InitializeMap();
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
            if(locationProvider != null)
                locationManager.RequestLocationUpdates(locationProvider, 2000L, 0.01F,  this);
        }
		protected override void OnPause()
		{
			base.OnPause();
            if(locationManager != null)
			locationManager.RemoveUpdates(this);
		}
            
		public void OnLocationChanged(Location location)
		{
			currentLocation = location;
            prevLocation = locationManager.GetLastKnownLocation(locationProvider);
			if (currentLocation == null && prevLocation == null)
			{
                //GPS Provider can not find current location uses network provider for finding device location
                string Provider = LocationManager.NetworkProvider;

                if(locationManager.IsProviderEnabled(Provider))
                {
                    locationManager.RequestLocationUpdates (Provider, 2000L, 0.01F,  this);
                } 
			}
			else
			{
				locationText.Text = String.Format ("{0},{1}", currentLocation.Latitude, currentLocation.Longitude);
                double diffAlt = (double) Math.Abs(prevLocation.Latitude - currentLocation.Latitude);
                double diffLong = (double) Math.Abs(prevLocation.Longitude - currentLocation.Longitude);
                if ((diffAlt > eps ) && (diffLong > eps))
                {

                    string tag = "prevLocation - currentLocation";
                    string tag1 = "prevLocation";
                    string tag2 = "currentLocation";
                    Log.Info(tag, diffAlt.ToString());
                    Log.Info(tag, diffLong.ToString());
                    Log.Info(tag1, prevLocation.ToString());
                    Log.Info(tag2, currentLocation.ToString());
                    InitializeMap();
                }
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
                //GPS Provider can not find current location uses network provider for finding device location
                string Provider = LocationManager.NetworkProvider;

                if(locationManager.IsProviderEnabled(Provider))
                {
                    locationManager.RequestLocationUpdates (Provider, 2000L, 0.01F, this);
                    currentLocation = locationManager.GetLastKnownLocation(locationProvider);
                    location = new LatLng (currentLocation.Latitude, currentLocation.Longitude);
                } 
			}
         
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var mobileState = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi).GetState();
            if(location != null)
            if (mobileState == NetworkInfo.State.Connected)
            {
                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();

                builder.Target(location);
                builder.Zoom(18);
                builder.Bearing(155);
                builder.Tilt(65);


                CameraPosition cameraPosition = builder.Build();
                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);


                if (map != null)
                {
                    map.MoveCamera(cameraUpdate);
                    map.MapType = GoogleMap.MapTypeSatellite;
                    map.UiSettings.MapToolbarEnabled = true;
                    map.UiSettings.MyLocationButtonEnabled = true;
                    map.UiSettings.ZoomControlsEnabled = true;
                    map.UiSettings.CompassEnabled = true;
                    MarkerOptions markerOpt1 = new MarkerOptions();
                    markerOpt1.SetPosition(location);
                    markerOpt1.SetTitle("You are here!");
                    map.AddMarker(markerOpt1);
                }
                else
                {
                    InitializeErrorHandler("Can't load map!");
                }
            }
            else
            {
                InitializeErrorHandler("No Internet Conection!");
            }
		}

        public void InitializeErrorHandler(string error)
        {
            var activity2 = new Intent (this, typeof(ErrorHandler));
            activity2.PutExtra ("error", error);
            StartActivity (activity2);
        }
	}
}


