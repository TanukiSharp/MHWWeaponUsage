﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHWSaveUtils;
using MHWWeaponUsage.ScalableVectorGraphics;

namespace MHWWeaponUsage.ViewModels
{
    public enum WeaponType
    {
        GreatSword,
        LongSword,
        SwordAndShield,
        DualBlades,
        Hammer,
        HuntingHorn,
        Lance,
        Gunlance,
        SwitchAxe,
        ChargeBlade,
        InsectGlaive,
        LightBowgun,
        HeavyBowgun,
        Bow
    }

    public class WeaponUsageValueViewModel
    {
        public WeaponType WeaponType { get; }
        public ushort Count { get; }
        public double Ratio { get; }
        public double Scale { get; }

        private const double ScaleMin = 1.5;
        private const double ScaleMax = 0.75;

        public WeaponUsageValueViewModel(WeaponType weaponType, ushort count, double ratio)
        {
            WeaponType = weaponType;
            Count = count;
            Ratio = ratio;

            Scale = ScaleMin + ratio * (ScaleMax - ScaleMin);
        }
    }

    public interface IZoomFactorController
    {
        double ZoomFactor { get; set; }
    }

    public sealed class WeaponUsageViewModel : ViewModelBase, IDisposable, IZoomFactorController
    {
        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            private set { SetValue(ref isVisible, value); }
        }

        private IList<WeaponUsageValueViewModel> values;
        public IList<WeaponUsageValueViewModel> Values
        {
            get { return values; }
            private set { SetValue(ref values, value); }
        }

        private readonly Throttler refreshRendersThrottler;

        public static double PersistedZoomFactor { get; private set; } = 1.0;

        private double zoomFactor = PersistedZoomFactor;
        public double ZoomFactor
        {
            get { return zoomFactor; }
            set
            {
                if (SetValue(ref zoomFactor, value))
                {
                    PersistedZoomFactor = zoomFactor;
                    refreshRendersThrottler.Reset();
                }
            }
        }

        private readonly RootViewModel rootViewModel;
        private readonly IEnumerable<WeaponUsageValueViewModel> viewModels;
        private readonly ViewType viewType;

        private static readonly WeaponType[] weaponTypes = (WeaponType[])Enum.GetValues(typeof(WeaponType));

        static WeaponUsageViewModel()
        {
            RasterizedImageContainer.Initialize(() => PersistedZoomFactor);
        }

        public WeaponUsageViewModel(RootViewModel rootViewModel, ViewType viewType, WeaponUsage weaponUsage)
        {
            if (rootViewModel == null)
                throw new ArgumentNullException(nameof(rootViewModel));

            this.rootViewModel = rootViewModel;
            this.viewType = viewType;

            zoomFactor = PersistedZoomFactor;

            refreshRendersThrottler = new Throttler(ForceRender, 500);

            ushort[] arrayValues = new ushort[]
            {
                weaponUsage.GreatSword,
                weaponUsage.LongSword,
                weaponUsage.SwordAndShield,
                weaponUsage.DualBlades,
                weaponUsage.Hammer,
                weaponUsage.HuntingHorn,
                weaponUsage.Lance,
                weaponUsage.Gunlance,
                weaponUsage.SwitchAxe,
                weaponUsage.ChargeBlade,
                weaponUsage.InsectGlaive,
                weaponUsage.LightBowgun,
                weaponUsage.HeavyBowgun,
                weaponUsage.Bow
            };

            ushort min = arrayValues.Min();
            ushort max = arrayValues.Max();

            double length = max - min;

            viewModels = arrayValues
                .Select((wu, i) => new WeaponUsageValueViewModel(weaponTypes[i], wu, length > 0 ? (wu - min) / length : 1.0))
                .ToList();

            Values = ApplySorting().ToList();
            IsVisible = viewType == rootViewModel.ViewType;

            rootViewModel.SortingChanged += OnSortingChanged;
            rootViewModel.ViewTypeChanged += OnViewTypeChanged;
        }

        private void OnSortingChanged(object sender, EventArgs e)
        {
            Values = ApplySorting().ToList();
        }

        private void OnViewTypeChanged(object sender, EventArgs e)
        {
            IsVisible = viewType == rootViewModel.ViewType;
        }

        private IEnumerable<WeaponUsageValueViewModel> ApplySorting()
        {
            switch (rootViewModel.Sorting)
            {
                case Sorting.Tree: return viewModels.OrderBy(x => x.WeaponType);
                case Sorting.UsageAscending: return viewModels.OrderBy(x => x.Count);
                case Sorting.UsageDescending: return viewModels.OrderByDescending(x => x.Count);
            }

            throw new InvalidOperationException($"Unknown sorting type '{rootViewModel.Sorting}'");
        }

        public void ForceRender()
        {
            RasterizedImageContainer.ClearCache();

            // Classic MVVM hack to force re-render of the view from view model.
            IList<WeaponUsageValueViewModel> originalValues = Values;
            Values = null;
            Values = originalValues;
        }

        public void Dispose()
        {
            rootViewModel.SortingChanged -= OnSortingChanged;
            rootViewModel.ViewTypeChanged -= OnViewTypeChanged;
        }
    }
}
