﻿using CodeClinic;
using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for ConstantChangesChart.xaml
    /// </summary>
    public partial class ConstantChangesChart : UserControl, INotifyPropertyChanged
    {

        private static long tickZero = DateTime.Parse("2018-01-01T08:00:00Z").Ticks;

        public Func<double, string> XAxisLabelFormatter { get; set; } = d => TimeSpan.FromTicks((long)d - tickZero).TotalSeconds.ToString();
        public ConstantChangesChart()
        {
            InitializeComponent();

            lsEffiency.Configuration = Mappers.Xy<FactoryTelemetry>().X(ft => ft.TimeStamp.Ticks).Y(ft => ft.Efficiency);

            DataContext = this;
        }

        public ChartValues<FactoryTelemetry> ChartValues { get; set; } = new ChartValues<FactoryTelemetry>();
        private bool readingData = false;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!readingData)
            {
                Task.Factory.StartNew(ReadData);
            }
            readingData = !readingData;
        }

        private void ReadData()
        {
            //Todo Populate the collection ChartValues
            string filename = @"D:\Documents\Projects\CodeClinicCSharp\dashBoardData.csv";

            foreach (var ft in FactoryTelemetry.Load(filename))
            {
                ChartValues.Add(ft);

                this.EngineEfficiency = ft.Efficiency;

                AdjustAxis(ft.TimeStamp.Ticks);

                if (ChartValues.Count > 30)
                {
                    ChartValues.RemoveAt(0);
                }

                Thread.Sleep(30);
            }
        }

        public double AxisStep { get; set; } = TimeSpan.FromSeconds(5).Ticks;
        public double AxisUnit { get; set; } = TimeSpan.FromSeconds(1).Ticks;

        private double axisMax = tickZero + TimeSpan.FromSeconds(30).Ticks;
        private double axisMin = tickZero;
        public double AxisMax { get => axisMax; set { axisMax = value; OnPropertyChanged(nameof(AxisMax)); } }

        public double AxisMin { get => axisMin; set { axisMin = value; OnPropertyChanged(nameof(AxisMin)); } }

        private void AdjustAxis(long ticks)
        {
            var width = TimeSpan.FromSeconds(30).Ticks;
            AxisMin = (ticks - tickZero < width) ? tickZero : ticks - width;
            AxisMax = (ticks - tickZero < width) ? tickZero + width : ticks;
        }

        private double _EngineEffiency = 65;

        public double EngineEfficiency
        {

            get
            {
                return _EngineEffiency;
            }
            set
            {
                _EngineEffiency = value;
                OnPropertyChanged(nameof(EngineEfficiency));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
