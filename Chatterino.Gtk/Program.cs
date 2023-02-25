﻿using Chatterino.Common;
using Chatterino.Gtk.Controls;
using Gtk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Gtk
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();

            AppSettings.SavePath = Path.Combine(Util.GetUserDataPath(), "Settings.ini");

            GuiEngine.Initialize(new GtkGuiEngine());

            Cache.Load();
            AppSettings.Load(null);

            IrcManager.Connect();

            var window = new MainWindow();

            window.ShowAll();
            window.Hidden += (s, e) => { Application.Quit(); };

            Application.Run();

            AppSettings.Save(null);
        }
    }
}
