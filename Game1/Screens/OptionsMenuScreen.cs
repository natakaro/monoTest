using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Game1.GameSettings;

namespace Game1.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields
        GameSettings settings;

        MenuEntry languageMenuEntry;

        MenuEntry emptyMenuEntry;

        MenuEntry shadowsMenuEntry;
        MenuEntry shadowFilteringMenuEntry;
        MenuEntry stabilizeCascadesMenuEntry;
        MenuEntry filterAcrossCascadesMenuEntry;
        MenuEntry shadowBiasMenuEntry;
        MenuEntry shadowOffsetMenuEntry;

        MenuEntry ssaoMenuEntry;
        MenuEntry ssaoRadiusMenuEntry;
        MenuEntry ssaoPowerMenuEntry;

        MenuEntry fogMenuEntry;
        MenuEntry dofMenuEntry;
        MenuEntry fxaaMenuEntry;
        MenuEntry hdrMenuEntry;
        MenuEntry reflectionsMenuEntry;
        MenuEntry vignetteMenuEntry;

        MenuEntry back;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            languageMenuEntry = new MenuEntry(string.Empty);

            emptyMenuEntry = new MenuEntry(string.Empty);

            shadowsMenuEntry = new MenuEntry(string.Empty);
            shadowFilteringMenuEntry = new MenuEntry(string.Empty, true);
            stabilizeCascadesMenuEntry = new MenuEntry(string.Empty);
            filterAcrossCascadesMenuEntry = new MenuEntry(string.Empty);
            shadowBiasMenuEntry = new MenuEntry(string.Empty, true);
            shadowOffsetMenuEntry = new MenuEntry(string.Empty, true);

            ssaoMenuEntry = new MenuEntry(string.Empty);
            ssaoRadiusMenuEntry = new MenuEntry(string.Empty, true);
            ssaoPowerMenuEntry = new MenuEntry(string.Empty, true);

            fogMenuEntry = new MenuEntry(string.Empty, true);
            dofMenuEntry = new MenuEntry(string.Empty, true);
            fxaaMenuEntry = new MenuEntry(string.Empty);
            hdrMenuEntry = new MenuEntry(string.Empty);
            reflectionsMenuEntry = new MenuEntry(string.Empty);
            vignetteMenuEntry = new MenuEntry(string.Empty);

            back = new MenuEntry("Back");

            languageMenuEntry.Selected += LanguageMenuEntrySelected;
            languageMenuEntry.SelectedLeft += LanguageMenuEntrySelected;
            languageMenuEntry.SelectedRight += LanguageMenuEntrySelected;

            shadowsMenuEntry.Selected += ShadowsMenuEntrySelected;
            shadowsMenuEntry.SelectedLeft += ShadowsMenuEntrySelected;
            shadowsMenuEntry.SelectedRight += ShadowsMenuEntrySelected;
            shadowFilteringMenuEntry.Selected += ShadowFilteringMenuEntrySelected;
            shadowFilteringMenuEntry.SelectedLeft += ShadowFilteringMenuEntrySelectedLeft;
            shadowFilteringMenuEntry.SelectedRight += ShadowFilteringMenuEntrySelected;
            stabilizeCascadesMenuEntry.Selected += StabilizeCascadesMenuEntrySelected;
            stabilizeCascadesMenuEntry.SelectedLeft += StabilizeCascadesMenuEntrySelected;
            stabilizeCascadesMenuEntry.SelectedRight += StabilizeCascadesMenuEntrySelected;
            shadowBiasMenuEntry.Selected += ShadowBiasMenuEntrySelected;
            shadowBiasMenuEntry.SelectedLeft += ShadowBiasMenuEntrySelectedLeft;
            shadowBiasMenuEntry.SelectedRight += ShadowBiasMenuEntrySelected;
            shadowOffsetMenuEntry.Selected += ShadowOffsetMenuEntrySelected;
            shadowOffsetMenuEntry.SelectedLeft += ShadowOffsetMenuEntrySelectedLeft;
            shadowOffsetMenuEntry.SelectedRight += ShadowOffsetMenuEntrySelected;

            ssaoMenuEntry.Selected += SsaoMenuEntrySelected;
            ssaoMenuEntry.SelectedLeft += SsaoMenuEntrySelected;
            ssaoMenuEntry.SelectedRight += SsaoMenuEntrySelected;
            ssaoRadiusMenuEntry.Selected += SsaoRadiusMenuEntrySelected;
            ssaoRadiusMenuEntry.SelectedLeft += SsaoRadiusMenuEntrySelectedLeft;
            ssaoRadiusMenuEntry.SelectedRight += SsaoRadiusMenuEntrySelected;
            ssaoPowerMenuEntry.Selected += SsaoPowerMenuEntrySelected;
            ssaoPowerMenuEntry.SelectedLeft += SsaoPowerMenuEntrySelectedLeft;
            ssaoPowerMenuEntry.SelectedRight += SsaoPowerMenuEntrySelected;

            fogMenuEntry.Selected += FogMenuEntrySelected;
            fogMenuEntry.SelectedLeft += FogMenuEntrySelectedLeft;
            fogMenuEntry.SelectedRight += FogMenuEntrySelected;
            dofMenuEntry.Selected += DofMenuEntrySelected;
            dofMenuEntry.SelectedLeft += DofMenuEntrySelectedLeft;
            dofMenuEntry.SelectedRight += DofMenuEntrySelected;
            fxaaMenuEntry.Selected += FxaaMenuEntrySelected;
            fxaaMenuEntry.SelectedLeft += FxaaMenuEntrySelected;
            fxaaMenuEntry.SelectedRight += FxaaMenuEntrySelected;
            hdrMenuEntry.Selected += HdrMenuEntrySelected;
            hdrMenuEntry.SelectedLeft += HdrMenuEntrySelected;
            hdrMenuEntry.SelectedRight += HdrMenuEntrySelected;
            reflectionsMenuEntry.Selected += ReflectionsMenuEntrySelected;
            reflectionsMenuEntry.SelectedLeft += ReflectionsMenuEntrySelected;
            reflectionsMenuEntry.SelectedRight += ReflectionsMenuEntrySelected;

            vignetteMenuEntry.Selected += VignetteMenuEntrySelected;
            vignetteMenuEntry.SelectedLeft += VignetteMenuEntrySelected;
            vignetteMenuEntry.SelectedRight += VignetteMenuEntrySelected;

            back.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(languageMenuEntry);

            MenuEntries.Add(emptyMenuEntry);

            MenuEntries.Add(shadowsMenuEntry);
            MenuEntries.Add(shadowFilteringMenuEntry);
            MenuEntries.Add(stabilizeCascadesMenuEntry);
            MenuEntries.Add(filterAcrossCascadesMenuEntry);
            MenuEntries.Add(shadowBiasMenuEntry);
            MenuEntries.Add(shadowOffsetMenuEntry);

            MenuEntries.Add(emptyMenuEntry);

            MenuEntries.Add(ssaoMenuEntry);
            MenuEntries.Add(ssaoRadiusMenuEntry);
            MenuEntries.Add(ssaoPowerMenuEntry);

            MenuEntries.Add(emptyMenuEntry);

            MenuEntries.Add(fogMenuEntry);
            MenuEntries.Add(dofMenuEntry);
            MenuEntries.Add(fxaaMenuEntry);
            MenuEntries.Add(hdrMenuEntry);
            MenuEntries.Add(reflectionsMenuEntry);
            MenuEntries.Add(vignetteMenuEntry);

            MenuEntries.Add(emptyMenuEntry);

            MenuEntries.Add(back);
        }

        public override void Activate()
        {
            base.Activate();

            settings = (ScreenManager.Game as Game1).settings;
            SetMenuEntryText();
        }

        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            languageMenuEntry.Text = "Language: " + (settings.Polish ? "Polish" : "English");

            shadowsMenuEntry.Text = "Shadows: " + (settings.Shadows ? "on" : "off");
            shadowFilteringMenuEntry.Text = "Shadow filtering: " + settings.FixedFilterSize.ToString();
            stabilizeCascadesMenuEntry.Text = "Stabilize shadow cascades: " + (settings.StabilizeCascades ? "on" : "off");
            filterAcrossCascadesMenuEntry.Text = "Filter across shadow cascades: " + (settings.FilterAcrossCascades ? "on" : "off");
            shadowBiasMenuEntry.Text = "Shadow bias: " + settings.Bias.ToString();
            shadowOffsetMenuEntry.Text = "Shadow offset: " + settings.OffsetScale.ToString();

            ssaoMenuEntry.Text = "SSAO: " + (settings.DrawSSAO ? "on" : "off");
            ssaoRadiusMenuEntry.Text = "SSAO Radius: " + settings.SSAORadius.ToString();
            ssaoPowerMenuEntry.Text = "SSAO Power: " + settings.SSAOPower.ToString();

            fogMenuEntry.Text = "Fog: " + settings.fog.ToString();
            dofMenuEntry.Text = "Depth of field: " + settings.dofType.ToString();
            fxaaMenuEntry.Text = "FXAA: " + (settings.FXAA ? "on" : "off");
            hdrMenuEntry.Text = "HDR and Tonemapping: " + (settings.ToneMap ? "on" : "off");
            reflectionsMenuEntry.Text = "Water reflections: " + (settings.ReflectObjects ? "on" : "off");
            vignetteMenuEntry.Text = "Vignette: " + (settings.Vignette ? "on" : "off");
        }
        #endregion

        #region Handle Input

        void LanguageMenuEntrySelected(object sender, EventArgs e)
        {
            settings.Polish = !settings.Polish;

            SetMenuEntryText();
        }

        void ShadowsMenuEntrySelected(object sender, EventArgs e)
        {
            settings.Shadows = !settings.Shadows;

            SetMenuEntryText();
        }

        void ShadowFilteringMenuEntrySelected(object sender, EventArgs e)
        {
            settings.FixedFilterSize++;
            if (settings.FixedFilterSize > FixedFilterSize.Filter7x7)
                settings.FixedFilterSize = FixedFilterSize.Filter2x2;

            SetMenuEntryText();
        }
        void ShadowFilteringMenuEntrySelectedLeft(object sender, EventArgs e)
        {
            settings.FixedFilterSize--;
            if (settings.FixedFilterSize < FixedFilterSize.Filter2x2)
                settings.FixedFilterSize = FixedFilterSize.Filter7x7;

            SetMenuEntryText();
        }

        void StabilizeCascadesMenuEntrySelected(object sender, EventArgs e)
        {
            settings.StabilizeCascades = !settings.StabilizeCascades;

            SetMenuEntryText();
        }

        void FilterAcrossCascadesMenuEntrySelected(object sender, EventArgs e)
        {
            settings.FilterAcrossCascades = !settings.FilterAcrossCascades;

            SetMenuEntryText();
        }

        void ShadowBiasMenuEntrySelected(object sender, EventArgs e)
        {
            settings.Bias += 0.001f;
            settings.Bias = (float)Math.Round(settings.Bias, 3);

            SetMenuEntryText();
        }
        void ShadowBiasMenuEntrySelectedLeft(object sender, EventArgs e)
        {
            settings.Bias -= 0.001f;
            settings.Bias = Math.Max(settings.Bias, 0.0f);
            settings.Bias = (float)Math.Round(settings.Bias, 3);

            SetMenuEntryText();
        }

        void ShadowOffsetMenuEntrySelected(object sender, EventArgs e)
        {
            settings.OffsetScale += 0.1f;
            settings.OffsetScale = (float)Math.Round(settings.OffsetScale, 1);

            SetMenuEntryText();
        }
        void ShadowOffsetMenuEntrySelectedLeft(object sender, EventArgs e)
        {
            settings.OffsetScale -= 0.1f;
            settings.OffsetScale = Math.Max(settings.OffsetScale, 0.0f);
            settings.OffsetScale = (float)Math.Round(settings.OffsetScale, 1);

            SetMenuEntryText();
        }

        void SsaoMenuEntrySelected(object sender, EventArgs e)
        {
            settings.DrawSSAO = !settings.DrawSSAO;

            SetMenuEntryText();
        }

        void SsaoRadiusMenuEntrySelected(object sender, EventArgs e)
        {
            settings.SSAORadius += 0.1f;
            settings.SSAORadius = (float)Math.Round(settings.SSAORadius, 1);

            SetMenuEntryText();
        }
        void SsaoRadiusMenuEntrySelectedLeft(object sender, EventArgs e)
        {
            settings.SSAORadius -= 0.1f;
            settings.SSAORadius = Math.Max(settings.SSAORadius, 0.0f);
            settings.SSAORadius = (float)Math.Round(settings.SSAORadius, 1);

            SetMenuEntryText();
        }

        void SsaoPowerMenuEntrySelected(object sender, EventArgs e)
        {
            settings.SSAOPower += 0.1f;
            settings.SSAOPower = (float)Math.Round(settings.SSAOPower, 1);

            SetMenuEntryText();
        }
        void SsaoPowerMenuEntrySelectedLeft(object sender, EventArgs e)
        {
            settings.SSAOPower -= 0.1f;
            settings.SSAOPower = Math.Max(settings.SSAOPower, 0.0f);
            settings.SSAOPower = (float)Math.Round(settings.SSAOPower, 1);

            SetMenuEntryText();
        }
        
        void FogMenuEntrySelected(object sender, EventArgs e)
        {
            settings.fog++;
            if (settings.fog > FogEffect.Exponential2)
                settings.fog = FogEffect.Off;

            SetMenuEntryText();
        }
        void FogMenuEntrySelectedLeft(object sender, EventArgs e)
        {
            settings.fog--;
            if (settings.fog < FogEffect.Off)
                settings.fog = FogEffect.Exponential2;

            SetMenuEntryText();
        }
        void FogMenuEntrySelectedRight(object sender, EventArgs e)
        {
            settings.fog++;
            if (settings.fog > FogEffect.Exponential2)
                settings.fog = FogEffect.Off;

            SetMenuEntryText();
        }

        void DofMenuEntrySelected(object sender, EventArgs e)
        {
            settings.dofType++;
            if (settings.dofType > DOFType.DiscBlur)
                settings.dofType = DOFType.Off;

            SetMenuEntryText();
        }
        void DofMenuEntrySelectedLeft(object sender, EventArgs e)
        {
            settings.dofType--;
            if (settings.dofType < DOFType.Off)
                settings.dofType = DOFType.DiscBlur;

            SetMenuEntryText();
        }

        void FxaaMenuEntrySelected(object sender, EventArgs e)
        {
            settings.FXAA = !settings.FXAA;

            SetMenuEntryText();
        }

        void HdrMenuEntrySelected(object sender, EventArgs e)
        {
            settings.ToneMap = !settings.ToneMap;

            SetMenuEntryText();
        }

        void ReflectionsMenuEntrySelected(object sender, EventArgs e)
        {
            settings.ReflectObjects = !settings.ReflectObjects;

            SetMenuEntryText();
        }

        void VignetteMenuEntrySelected(object sender, EventArgs e)
        {
            settings.Vignette = !settings.Vignette;

            SetMenuEntryText();
        }
        #endregion
    }
}
