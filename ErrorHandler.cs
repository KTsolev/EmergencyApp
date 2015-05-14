
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace EmergencyApp
{
    [Activity(Label = "ErrorHandler")]			
    public class ErrorHandler : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            string error = Intent.GetStringExtra ("error") ?? "Data not available";
           //Display Error message
            var alertDialog = new AlertDialog.Builder(this);       
            alertDialog.SetPositiveButton("Ok", (sender, args) =>
            {
                //Return to main activity
                var goBack = new Intent(this, typeof(MainActivity))
                    .SetFlags(ActivityFlags.ReorderToFront);
                StartActivity(goBack);
            });
            alertDialog.SetMessage(error);
            alertDialog.Show();    
        }
    }
}

