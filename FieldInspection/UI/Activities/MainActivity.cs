﻿using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Java.IO;
using Newtonsoft.Json;

namespace FieldInspection
{
    public static class App
    {
        public static File _file;
        public static File _dir;
        public static Bitmap _bitmap;
    }

    [Activity(Label = "FieldInspection", Theme = "@style/MyTheme.Base")]

    public class MainActivity : AppCompatActivity
    {
        public Culture SelectedCulture { get; set; }

        DrawerLayout _drawerLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetContentView(Resource.Layout.Main);

            base.OnCreate(savedInstanceState);

            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            // Init toolbar
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.App_Bar);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetTitle(Resource.String.app_name);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            // Create ActionBarDrawerToggle button and add it to the toolbar
            var drawerToggle = new ActionBarDrawerToggle(this, _drawerLayout, toolbar, Resource.String.open_drawer, Resource.String.close_drawer);

            if (drawerToggle != null)
            {
                _drawerLayout.SetDrawerListener(drawerToggle);
            }

            drawerToggle.SyncState();

            //load default home screen
            var ft = FragmentManager.BeginTransaction();

            //ft.AddToBackStack(null);
            ft.Add(Resource.Id.HomeFrameLayout, new DashboardFragment());
            ft.Commit();

            SelectedCulture = JsonConvert.DeserializeObject<Culture>(Intent.GetStringExtra("key"));

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            navigationView.NavigationItemSelected += NavigationView_NavigationItemSelected;
            // Attach item selected handler to navigation view

        }

        //define custom title text
        protected override void OnResume()
        {
            SupportActionBar.SetTitle(Resource.String.app_name);

            base.OnResume();
        }

        //define action for navigation menu selection
        async void NavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            var progress = new ProgressDialog(this, Android.App.AlertDialog.ThemeDeviceDefaultLight);

            progress.SetMessage("I'm getting data...");
            progress.SetTitle("Please wait");
            progress.Show();

            await Task.Run(() =>
            {
                switch (e.MenuItem.ItemId)
                {
                    case (Resource.Id.nav_dashboard):

                        var ft = FragmentManager.BeginTransaction();
                        var home = new DashboardFragment();

                        ft.Replace(Resource.Id.HomeFrameLayout, home);
                        ft.Commit();
                        
                        break;

                    case (Resource.Id.nav_inspection):

                        var ftt = FragmentManager.BeginTransaction();
                        var inspp = new InspectionsFragment(ApiUitilities.GetData<Inspection>("api/Cultures/Inspections/", $"{SelectedCulture.ID}").ToArray());

                        ftt.Replace(Resource.Id.HomeFrameLayout, inspp);
                        ftt.Commit();

                        break;

                }
                return true;
            });
            // Close drawer
            _drawerLayout.CloseDrawers();
            progress.Dismiss();
        }

        //define action for tolbar icon press
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }


        //to avoid direct app exit on backpreesed and to show fragment from stack
        public override void OnBackPressed()
        {
            if (FragmentManager.BackStackEntryCount != 0)
            {
                FragmentManager.PopBackStack();
            }

            else
            {
                base.OnBackPressed();
            }
        }
    }
}

