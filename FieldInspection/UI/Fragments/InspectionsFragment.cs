﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.refractored.fab;
using Newtonsoft.Json;

namespace FieldInspection
{
    public class InspectionsFragment : Fragment, IScrollDirectorListener
    {
        public Culture SelectedCulture { get; set; }
        public Inspection[] Inspections { get; set; }
        
        SwipeRefreshLayout refresher;
        RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        PhotoAlbumAdapter mAdapter;

        public InspectionsFragment(Inspection[] inspections)
        {
            Inspections = inspections;
        }

        async void OnItemClick(object sender, int position)
        {
            var progress = new ProgressDialog(Activity, AlertDialog.ThemeDeviceDefaultLight);

            progress.SetMessage("I'm getting data...");
            progress.SetTitle("Please wait");
            progress.Show();

            await Task.Run(() =>
            {
                var inspections = ApiUitilities.GetData<Inspection>("api/Cultures/Inspections/", $"{SelectedCulture.ID}").ToArray();
                var ft = FragmentManager.BeginTransaction();
                var inspecView = new InspectionViewFragment();

                ft.AddToBackStack(null);
                ft.Replace(Resource.Id.HomeFrameLayout, inspecView);
                ft.Commit();
                inspecView.Inspection = inspections[position];

                return true;
            });
            
            progress.Dismiss();
            Toast.MakeText(Activity, "Done", ToastLength.Long).Show();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            var view = inflater.Inflate(Resource.Layout.Inspections_Layout, container, false);

            SelectedCulture = JsonConvert.DeserializeObject<Culture>(this.Activity.Intent.GetStringExtra("key"));
  
            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView);
            mLayoutManager = new LinearLayoutManager(view.Context);
            mRecyclerView.HasFixedSize = true;
            mRecyclerView.SetItemAnimator(new DefaultItemAnimator());

            mRecyclerView.SetLayoutManager(mLayoutManager);
            mRecyclerView.AddItemDecoration(new DividerItemDecoration(Activity, DividerItemDecoration.VerticalList));

            mAdapter = new PhotoAlbumAdapter(SelectedCulture,Inspections);
            mAdapter.ItemClick += OnItemClick;
            mRecyclerView.SetAdapter(mAdapter);

            var fab = view.FindViewById<FloatingActionButton>(Resource.Id.fab);

            fab.AttachToRecyclerView(mRecyclerView, this);
            fab.Size = FabSize.Mini;
            fab.Click += (sender, args) =>
            {
                var ft = FragmentManager.BeginTransaction();
                var addPres = new InspectionFragment();

                ft.AddToBackStack(null);
                ft.Replace(Resource.Id.HomeFrameLayout, addPres);
                ft.Commit();
            };

            return view;

        }

        public override void OnStart()
        {
            base.OnStart();

            refresher = Activity.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);
            refresher.SetColorScheme(Resource.Color.my_blue,
                Resource.Color.my_purple,
                Resource.Color.my_gray,
                Resource.Color.green);
            
            refresher.Refresh += HandleRefresh;
        }

        async void HandleRefresh(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                var ftt = FragmentManager.BeginTransaction();
                var inspp = new InspectionsFragment(ApiUitilities.GetData<Inspection>("api/Cultures/Inspections/", $"{SelectedCulture.ID}").ToArray());
                ftt.Replace(Resource.Id.HomeFrameLayout, inspp);
                ftt.Commit();
            });

            refresher.Refreshing = false;
        }

        public void OnScrollDown()
        {
            Console.WriteLine("RecyclerViewFragment: OnScrollDown");
        }

        public void OnScrollUp()
        {
            Console.WriteLine("RecyclerViewFragment: OnScrollUp");
        }
    }

    public class PhotoViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Image { get; private set; }
        public TextView Caption { get; private set; }

        // Get references to the views defined in the CardView layout.
        public PhotoViewHolder(View itemView, Action<int> listener)
            : base(itemView)
        {
            // Locate and cache view references:
            Image = itemView.FindViewById<ImageView>(Resource.Id.cardImage);
            Caption = itemView.FindViewById<TextView>(Resource.Id.cardFieldName);

            // Detect user clicks on the item view and report which item
            // was clicked (by position) to the listener:
            //#pragma warning disable CS0618 // Type or member is obsolete
            itemView.Click += (sender, e) => listener(Position);
            //#pragma warning restore CS0618 // Type or member is obsolete

        }
    }

    public class PhotoAlbumAdapter : RecyclerView.Adapter
    {
        public Culture SelectedCulture { get; set; }
        public event EventHandler<int> ItemClick;
        
        public Inspection[] Inspections { get; set; }
   
        // Load the adapter with the data set (photo album) at construction time:
        public PhotoAlbumAdapter(Culture culture, Inspection[] inspections)
        {
            SelectedCulture = culture;
            Inspections = inspections;       
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
        }

        // Create a new photo CardView (invoked by the layout manager): 
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                                          Inflate(Resource.Layout.Card_Layout, parent, false);
            PhotoViewHolder vh = new PhotoViewHolder(itemView, OnClick);
        
            return vh;
        }
        
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            PhotoViewHolder vh = holder as PhotoViewHolder;

            if (Inspections.Length > 0)
            {
                vh.Image.SetImageBitmap(Utilities.ConvertToBitmap(Inspections[position].Image));
                vh.Caption.Text = Inspections[position].Name;
            }

        }
        
        public override int ItemCount
        {
            get { return Inspections.Length; }
        }
        
        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
    }
}

