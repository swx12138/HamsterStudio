using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HamsterStudio.Models
{
    internal class ValueRange<T>(T val, T min, T max) : ObservableObject
    {
        private T _value = val;
        public T Value { get => _value; set => SetProperty(ref _value, value); }

        private T _min = min;
        public T Minimum => _min;

        private T _max = max;
        public T Maximum => _max;
    }
}
