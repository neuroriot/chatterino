﻿using Chatterino.Common;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace Chatterino
{
    public class ColorScheme
    {
        public bool IsLightTheme { get; private set; } = true;

        public Brush TooltipBackground { get; set; } = Brushes.Black;
        public Brush TooltipText { get; set; } = Brushes.White;

        public Color ChatSeparator { get; set; } = Color.Red;
        public Brush ChatBackground { get; set; } = Brushes.White;
        public Brush ChatBackgroundHighlighted { get; set; } = Brushes.LightBlue;
        public Brush ChatBackgroundUsernameHighlighted { get; set; } = Brushes.LightBlue;
        public Brush ChatBackgroundResub { get; set; } = Brushes.LightBlue;
        public Brush ChatBackgroundWhisper { get; set; } = Brushes.LightBlue;
        public Brush ChatBackgroundSearchResult { get; set; } = Brushes.GreenYellow;
        public Brush ChatBackgroundHighlightedMessage { get; set; } = Brushes.Teal;
        public Brush ChatInputOuter { get; set; } = Brushes.White;
        public Brush ChatInputInner { get; set; } = Brushes.White;
        public Pen ChatInputBorder { get; set; } = Pens.White;
        public Pen ChatMessageSeparatorBorder { get; set; } = Pens.Black;
        public Pen ChatMessageSeparatorBorderInner { get; set; } = Pens.Gray;

        public Pen ChatBorder { get; set; } = Pens.LightGray;
        public Pen ChatBorderFocused { get; set; } = Pens.Black;
        public Color Text { get; set; } = Color.Black;
        public Brush TextCaret { get; set; } = Brushes.Black;
        public Color TextLink { get; set; } = Color.Blue;
        public Color TextFocused { get; set; } = Color.Red;
        public Brush Menu { get; set; } = Brushes.Black;
        public Pen MenuBorder { get; set; } = Pens.Black;

        public Brush ScrollbarBG { get; set; } = Brushes.White;
        public Brush ScrollbarThumb { get; set; } = Brushes.Gray;
        public Brush ScrollbarThumbSelected { get; set; } = Brushes.Gray;
        public Brush ScrollbarArrow { get; set; } = Brushes.Gray;

        // Tabs
        public Color TabPanelBG { get; set; } = rgb(0xFFFFFF);

        public Brush TabBG { get; set; } = new SolidBrush(rgb(0xFFFFFF));
        public Brush TabHoverBG { get; set; } = new SolidBrush(rgb(0xCCCCCC));
        public Brush TabSelectedBG { get; set; } = new SolidBrush(rgb(0x8E24AA));
        public Brush TabButtonsBG { get; set; } = new SolidBrush(rgb(0x8E24AA));
        public Brush TabHighlightedBG { get; set; } = new SolidBrush(rgb(0xFF4444));
        public Brush TabNewMessageBG { get; set; } = new SolidBrush(rgb(0xFF4444));
        public Brush TabIsLiveBG { get; set; } = new SolidBrush(rgb(0xbff7aa));

        public Color TabText { get; set; } = Color.Black;
        public Color TabHoverText { get; set; } = Color.Black;
        public Color TabSelectedText { get; set; } = Color.White;
        public Color TabHighlightedText { get; set; } = Color.Black;

        static ConcurrentDictionary<string, PropertyInfo> properties = new ConcurrentDictionary<string, PropertyInfo>();

        public static ColorScheme FromHue(float hue, float multiplier)
        {
            HSLColor getColor(HSLColor hsl, float luminosity) => hsl.WithLuminosity(((luminosity - 0.5f) * multiplier) + 0.5f);

            var scheme = new ColorScheme();

            // Light scheme
            scheme.IsLightTheme = multiplier > 0;

            HSLColor gray;
            if (multiplier > 0.9f || multiplier < -0.9f)
            {
                gray = HSLColor.FromRGB(0.5f, 0.5f, 0.5f);
            }
            else
            {
                gray = HSLColor.FromRGB(0.5f, 0.5f, 0.515f);
            }

            var highlight = new HSLColor(hue, 1f, scheme.IsLightTheme ? 0.5f : 0.3f);

            // Text
            scheme.TextCaret = new SolidBrush(scheme.Text = (multiplier > 0 ? HSLColor.FromRGB(0, 0, 0) : HSLColor.FromRGB(1, 1, 1)).ToColor());
            scheme.ChatBorderFocused = scheme.ChatBorder = getColor(gray, 1f).ToPen();
            scheme.ChatMessageSeparatorBorder = getColor(gray, 0.99f).ToPen();
            scheme.ChatMessageSeparatorBorderInner = getColor(gray, 0.8f).ToPen();
            scheme.TabText = scheme.IsLightTheme ? HSLColor.FromRGB(0, 0, 0).ToColor() : HSLColor.FromRGB(1, 1, 1).ToColor();

            scheme.ChatSeparator = getColor(gray, 0.75f).ToColor();

            // Backgrounds
            var backgroundMultiplier = multiplier > 0.9f ? 1f : 0.95f;
            scheme.ChatBackground = getColor(gray, backgroundMultiplier).ToBrush();
            scheme.TabBG = getColor(gray, backgroundMultiplier).ToBrush();
            scheme.TabPanelBG = getColor(gray, backgroundMultiplier).ToColor();

            if (scheme.IsLightTheme)
            {
                scheme.ChatBackgroundHighlighted = getColor(HSLColor.FromRGB(1f, 0.5f, 0.5f), 0.9f).ToBrush();
                scheme.ChatBackgroundUsernameHighlighted = new SolidBrush(rgb(0xead09f));
                scheme.ChatBackgroundResub = getColor(HSLColor.FromRGB(0.5f, 0.5f, 1f), 0.9f).ToBrush();
                scheme.ChatBackgroundWhisper = getColor(HSLColor.FromRGB(0.5f, 1f, 0.5f), 0.9f).ToBrush();
                scheme.ChatBackgroundSearchResult = new SolidBrush(rgb(0xaaf9a4));
                scheme.ChatBackgroundHighlightedMessage = new SolidBrush(rgb(0xc3e0df));
                scheme.TabIsLiveBG = new SolidBrush(rgb(0xbff7aa));
            }
            else
            {
                var l = multiplier == -1 ? 0 : 20;
                scheme.ChatBackgroundHighlighted = new SolidBrush(Color.FromArgb(55 + l, 20 + l, 24 + l));
                scheme.ChatBackgroundUsernameHighlighted = new SolidBrush(Color.FromArgb(61 + l, 49 + l, 28 + l));
                scheme.ChatBackgroundResub = new SolidBrush(Color.FromArgb(47 + l, 20 + l, 70 + l));
                scheme.ChatBackgroundWhisper = new SolidBrush(Color.FromArgb(20 + l, 40 + l, 70 + l));
                scheme.ChatBackgroundSearchResult = new SolidBrush(rgb(0x043000));
                scheme.ChatBackgroundHighlightedMessage = new SolidBrush(rgb(0x002e2b));
                scheme.TabIsLiveBG = new SolidBrush(rgb(0x26730D));
            }

            scheme.Menu = getColor(gray, 0.90f).ToBrush();
            scheme.MenuBorder = getColor(gray, 0.86f).ToPen();

            scheme.ChatInputOuter = getColor(gray, 0.90f).ToBrush();
            scheme.ChatInputBorder = getColor(gray, 0.86f).ToPen();

            // Scroll
            scheme.ScrollbarThumb = getColor(gray, 0.8f).ToBrush();

            // Highlights
            scheme.TabSelectedBG = highlight.WithSaturation(0.5f).ToBrush();
            scheme.TabHighlightedBG = highlight.WithLuminosity(highlight.Luminosity + 0.3f).WithSaturation(0.5f).ToBrush();
            scheme.TabNewMessageBG = new HatchBrush(HatchStyle.LightUpwardDiagonal, highlight.WithLuminosity(scheme.IsLightTheme ? 0.85f : 0.65f).WithSaturation(0.5f).ToColor(), highlight.WithLuminosity(scheme.IsLightTheme ? 0.95f : 0.05f).WithSaturation(0.5f).ToColor());
            scheme.TabHighlightedText = scheme.IsLightTheme ? Color.Black : Color.White;
            scheme.TextFocused = getColor(highlight, 0.25f).ToColor();

            scheme.TabButtonsBG = highlight.WithLuminosity(scheme.IsLightTheme ? highlight.Luminosity : highlight.Luminosity + .3f).WithSaturation(.5f).ToBrush();
            return scheme;
        }

        static ColorScheme()
        {
            var T = typeof(ColorScheme);

            foreach (var property in T.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                    properties[property.Name] = property;
            }
        }

        // IO
        public void Load(string path)
        {
            var settings = new IniSettings();
            settings.Load(path);

            foreach (var prop in properties.Values)
            {
                try
                {
                    string value;
                    if (settings.TryGetString(prop.Name, out value))
                    {
                        if (prop.PropertyType == typeof(Color))
                        {
                            // #FFFFFF
                            if (value.Length == 7 && value[0] == '#')
                            {
                                prop.SetValue(this, rgb(Convert.ToInt32(value.Substring(1), 16)), null);
                            }
                        }
                        else if (prop.PropertyType == typeof(Brush))
                        {
                            // #FFFFFF
                            if (value.Length == 7 && value[0] == '#')
                            {
                                prop.SetValue(this, new SolidBrush(rgb(Convert.ToInt32(value.Substring(1), 16))), null);
                            }
                        }
                        else if (prop.PropertyType == typeof(Pen))
                        {
                            // #FFFFFF
                            if (value.Length == 7 && value[0] == '#')
                            {
                                prop.SetValue(this, new Pen(new SolidBrush(rgb(Convert.ToInt32(value.Substring(1), 16)))), null);
                            }
                        }
                    }
                }
                catch { }
            }

            var c = (ChatBackground as SolidBrush)?.Color ?? Color.White;

            IsLightTheme = HSLColor.FromRGB(c.R / 255f, c.G / 255f, c.B / 255f).Luminosity > 0.5f;
        }

        public void Save(string path)
        {
            var settings = new IniSettings();

            foreach (var prop in properties.Values)
            {
                if (prop.PropertyType == typeof(Color))
                {
                    settings.Set(prop.Name, "#" + (((Color)prop.GetValue(this, null)).ToArgb() & 0xFFFFFF).ToString("X"));
                }
                else if (prop.PropertyType == typeof(Brush))
                {
                    var value = prop.GetValue(this, null);
                    var solidBrush = value as SolidBrush;
                    if (solidBrush != null)
                    {
                        settings.Set(prop.Name, "#" + ((solidBrush.Color).ToArgb() & 0xFFFFFF).ToString("X"));
                    }
                }
                else if (prop.PropertyType == typeof(Pen))
                {
                    var value = prop.GetValue(this, null);
                    var solidBrush = (value as Pen)?.Brush as SolidBrush;
                    if (solidBrush != null)
                    {
                        settings.Set(prop.Name, "#" + ((solidBrush.Color).ToArgb() & 0xFFFFFF).ToString("X"));
                    }
                }
            }

            settings.Save(path);
        }

        // helpers
        static Color rgb(int rgb)
        {
            return Color.FromArgb(-16777216 | rgb);
        }
    }
}
