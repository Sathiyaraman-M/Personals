using MudBlazor;

namespace Personals.UI.Themes;

public static class AppThemes
{
    private static class CustomTypography
    {
        public static readonly Default Default = new()
        {
            FontFamily = ["Inter", "Helvetica", "Arial", "sans-serif"],
            FontSize = ".8rem",
            FontWeight = 400,
            LineHeight = 1.23,
            LetterSpacing = ".00971em"
        };

        public static readonly H1 DefaultH1 = new()
        {
            FontSize = "2.5rem",
            FontWeight = 500,
            LineHeight = 1.1,
            LetterSpacing = "-.01562em",
        };
        
        public static readonly H2 DefaultH2 = new()
        {            
            FontSize = "2rem",
            FontWeight = 500,
            LineHeight = 1.1,
            LetterSpacing = "-.00833em",
        };
        
        public static readonly H3 DefaultH3 = new()
        {
            FontSize = "1.75rem",
            FontWeight = 500,
            LineHeight = 1.1,
            LetterSpacing = "0",
        };
        
        public static readonly H4 DefaultH4 = new() 
        {
            FontSize = "1.5rem",
            FontWeight = 500,
            LineHeight = 1.1,
            LetterSpacing = ".00735em",
        };
        
        public static readonly H5 DefaultH5 = new()
        {
            FontSize = "1.25rem",
            FontWeight = 500,
            LineHeight = 1.1,
            LetterSpacing = "0",
        };
        
        public static readonly H6 DefaultH6 = new() 
        { 
            FontSize = "1rem",
            FontWeight = 500,
            LineHeight = 1.1,
            LetterSpacing = ".0075em"
        };

        public static readonly Body1 DefaultBody1 = new()
        {
            FontSize = "0.85rem",
            FontWeight = 400,
            LineHeight = 1.1,
            LetterSpacing = ".00838em",
        };

        public static readonly Body2 DefaultBody2 = new()
        {
            FontSize = ".775rem",
            FontWeight = 400,
            LineHeight = 1,
            LetterSpacing = ".009em"
        };

        public static readonly Button DefaultButton = new()
        {
            FontSize = ".795rem",
            FontWeight = 500,
            LineHeight = 1.5,
            LetterSpacing = ".02em",
            TextTransform = "uppercase",
        };

        public static readonly Input DefaultInput = new()
        {
            FontSize = "0.85rem", FontWeight = 400, LineHeight = 1, LetterSpacing = ".009em"
        };

        public static readonly Caption DefaultCaption = new()
        {
            FontSize = "0.6rem", FontWeight = 400, LineHeight = 1.36, LetterSpacing = ".02em"
        };
    }

    private static readonly Typography DefaultTypography = new()
    {
        Default = CustomTypography.Default,
        H1 = CustomTypography.DefaultH1,
        H2 = CustomTypography.DefaultH2,
        H3 = CustomTypography.DefaultH3,
        H4 = CustomTypography.DefaultH4,
        H5 = CustomTypography.DefaultH5,
        H6 = CustomTypography.DefaultH6,
        Subtitle1 = new Subtitle1(),
        Subtitle2 = new Subtitle2(),
        Body1 = CustomTypography.DefaultBody1,
        Body2 = CustomTypography.DefaultBody2,
        Input = CustomTypography.DefaultInput,
        Button = CustomTypography.DefaultButton,
        Caption = CustomTypography.DefaultCaption,
        Overline = new Overline(),
    };

    private static readonly LayoutProperties DefaultLayoutProperties = new()
    {
        DefaultBorderRadius = "4px",
        DrawerMiniWidthLeft = "56px",
        DrawerMiniWidthRight = "56px",
        DrawerWidthLeft = "240px",
        DrawerWidthRight = "240px",
        AppbarHeight = "42px"
    };

    public static readonly MudTheme DefaultTheme = new()
    {
        PaletteLight = new PaletteLight(),
        PaletteDark = new PaletteDark(),
        Shadows = new Shadow(),
        Typography = DefaultTypography,
        LayoutProperties = DefaultLayoutProperties,
        ZIndex = new ZIndex(),
        PseudoCss = new PseudoCss()
    };
}