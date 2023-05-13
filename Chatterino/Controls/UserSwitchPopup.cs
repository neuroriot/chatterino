﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chatterino.Common;

namespace Chatterino
{
    public class UserSwitchPopup : Form
    {
        public UserSwitchPopup()
        {
            FormBorderStyle = FormBorderStyle.None;

            KeyPreview = true;

            TopMost = AppSettings.WindowTopMost;

            AppSettings.WindowTopMostChanged += (s, e) =>
            {
                TopMost = AppSettings.WindowTopMost;
            };

            // listview
            var listView = new ListView()
            {
                Font = new Font("Segoe UI", 14f),
                View = View.List,
                Size = new Size(Width, Height - 32)
            };

            Controls.Add(listView);

            listView.Items.Add("anonymous user");

            var accounts = AccountManager.Accounts.ToList();
            accounts.Sort((acc1, acc2) => string.Compare(acc1.Username, acc2.Username, StringComparison.Ordinal));
            
            foreach (var account in accounts)
            {
                listView.Items.Add(account.Username);
            }

            listView.ItemActivate += (s, e) =>
            {
                IrcManager.Account = AccountManager.FromUsername(listView.FocusedItem.Text) ?? Account.AnonAccount;
                IrcManager.Connect();
                if (IrcManager.Account != Account.AnonAccount && !GuiEngine.Current.GlobalBadgesLoaded)
                {
                    foreach (var channel in TwitchChannel.Channels)
                    {
                        channel.ReloadEmotes();
                    }
                    GuiEngine.Current.LoadBadges();
                }
                Close();
            };

            // button
            var manageAccountsButton = new Button()
            {
                AutoSize = true,
                Text = "Manage Accounts",
            };

            manageAccountsButton.Location = new Point(8, Height - (manageAccountsButton.Height + 32) / 2);

            manageAccountsButton.Click += (s, e) =>
            {
                Close();

                App.ShowSettings();
                App.SettingsDialog.Show("Accounts");
            };

            Controls.Add(manageAccountsButton);

            // hotkey
            var label = new Label
            {
                Text = "Hotkey: Ctrl+U",
                AutoSize = true,
            };

            label.Location = new Point(Width - label.Width, manageAccountsButton.Location.Y + 5);

            Controls.Add(label);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            Close();
        }
    }
}
