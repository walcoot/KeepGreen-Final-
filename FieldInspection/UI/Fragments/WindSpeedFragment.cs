﻿using System;
using Android.App;
using Android.OS;
using Android.Views;
using BarChart;

namespace FieldInspection
{
	public class WindSpeedFragment : Fragment
	{
	    private int Dashboard { get; set; }

	    public WindSpeedFragment(int dashboard)
	    {
	        Dashboard = dashboard;
	    }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.Details, container, false);
			return view;
		}

		public override void OnStart()
		{
			base.OnStart();
			var chart = new BarChartView(Activity);
			chart = Activity.FindViewById<BarChartView>(Resource.Id.barChart);
			PlotBars.PlotBarsChart(chart, Dashboard, 30, 35, 28);
		}
			
	}
}
