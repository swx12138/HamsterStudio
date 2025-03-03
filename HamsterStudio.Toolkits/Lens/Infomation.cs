using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HamsterStudio.Models.Lens
{
    class Infomation(string name, string brand, string model, string mount,
        double minFl, double maxFl, double minAOV, double maxAOV, double minAp, double maxAp,
        int apb, double ca, double minMd, double maxMd, double weight)
    {
        [DisplayName("名称")]
        public string Name { get; } = name;

        [DisplayName("品牌")]
        public string Brand { get; } = brand;

        [DisplayName("型号")]
        public string Model { get; } = model;

        #region Important

        [DisplayName("卡口")]
        public string Mount { get; } = mount;

        [DisplayName("焦距")]
        public ValueRange<double> FocalLength { get; } = new ValueRange<double>(minFl, minFl, maxFl);

        [DisplayName("视角")]
        public ValueRange<double> AngleOfView { get; } = new ValueRange<double>(minAOV, minAOV, maxAOV);

        [DisplayName("光圈")]
        public ValueRange<double> Aperture { get; } = new ValueRange<double>(maxAp, maxAp, minAp);

        [DisplayName("光圈叶片数")]
        public int ApertureBlades { get; } = apb;

        [DisplayName("口径")]
        public double Calibre { get; } = ca;

        [DisplayName("最近拍摄距离")]
        public ValueRange<double> MinimumDistance { get; } = new ValueRange<double>(0, minMd, maxMd);

        #endregion

        [DisplayName("重量")]
        public double Weight { get; } = weight;

        [DisplayName("喜爱")]
        public bool Favorite { get; set; }
    }
}
