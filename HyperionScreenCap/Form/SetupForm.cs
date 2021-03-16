﻿using HyperionScreenCap.Helper;
using HyperionScreenCap.Model;
using HyperionScreenCap.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Markdig;

namespace HyperionScreenCap
{
    public partial class SetupForm : Form
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SetupForm));

        private MainForm _mainForm;
        private List<HyperionTaskConfiguration> _taskConfigurations;

        public SetupForm(MainForm mainForm)
        {
            LOG.Info("Instantiating SetupForm");
            _mainForm = mainForm;
            InitializeComponent();
            dgTaskConfig.AutoGenerateColumns = false;

            LoadSettings();

            var helpHtml = Markdown.ToHtml(Resources.SetupFormHelp);
            wbHelpContent.DocumentText = helpHtml;
            lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            btnDonate.Text = AppConstants.TrayIcon.MENU_TXT_DONATE;
            LOG.Info("SetupForm Instantiated");
        }

        private void LoadSettings()
        {
            try
            {
                LOG.Info("Loading settings using SettingsManager");
                SettingsManager.LoadSetttings();
                chkCaptureOnStartup.Checked = SettingsManager.CaptureOnStartup;
                chkPauseUserSwitch.Checked = SettingsManager.PauseOnUserSwitch;
                chkPauseSuspend.Checked = SettingsManager.PauseOnSystemSuspend;
                tbApiPort.Text = SettingsManager.ApiPort.ToString();
                chkApiEnabled.Checked = SettingsManager.ApiEnabled;
                chkApiExcludeTimesEnabled.Checked = SettingsManager.ApiExcludedTimesEnabled;
                tbApiExcludeStart.Text = SettingsManager.ApiExcludeTimeStart.ToString("HH:mm");
                tbApiExcludeEnd.Text = SettingsManager.ApiExcludeTimeEnd.ToString("HH:mm");
                chkCheckUpdate.Checked = SettingsManager.CheckUpdateOnStartup;
                cbNotificationLevel.Text = SettingsManager.NotificationLevel.ToString();
                _taskConfigurations = SettingsManager.HyperionTaskConfigurations;
                Rebind_dgTaskConfig();
                LOG.Info("Finished loading settings using SettingsManager");
            }
            catch ( Exception ex )
            {
                LOG.Error("Failed to load settings into the setup form", ex);
                MessageBox.Show($"Error occcured during LoadSettings(): {ex.Message}");
            }
        }

        private void dgTaskConfig_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = 0; i < e.RowCount; i++)
            {
                PopulateUnboundColumns(e.RowIndex + i);
            }
        }

        private void Rebind_dgTaskConfig()
        {
            var hyperionTasksBindingList = new BindingList<HyperionTaskConfiguration>(_taskConfigurations);
            var hyperionTasksDataSource = new BindingSource(hyperionTasksBindingList, null);
            dgTaskConfig.DataSource = hyperionTasksDataSource;
        }

        private void PopulateUnboundColumns(int rowIndex)
        {
            var row = dgTaskConfig.Rows[rowIndex];
            row.Cells["clmnCaptureSource"].Value = GetCaptureSourceColumnValue(_taskConfigurations[rowIndex]);
            row.Cells["clmnHyperionServers"].Value = GetHyperionServersColumnValue(_taskConfigurations[rowIndex]);
        }

        private String GetCaptureSourceColumnValue(HyperionTaskConfiguration taskConfiguration)
        {
            switch (taskConfiguration.CaptureMethod)
            {
                case CaptureMethod.DX11:
                   return $"DX11 Adap:{taskConfiguration.Dx11AdapterIndex} Mon:{taskConfiguration.Dx11MonitorIndex}";

                case CaptureMethod.DX9:
                    return $"DX9 Mon: {taskConfiguration.Dx9MonitorIndex}";

                case CaptureMethod.DX11DualScreen:
                    return $"DX11 Dual: {taskConfiguration.Dx11DualScreenMonitorIndex1}-{taskConfiguration.Dx11DualScreenMonitorIndex2}";

                default:
                    throw new NotImplementedException($"The capture method {taskConfiguration.CaptureMethod} is not supported");
            }
        }

        private String GetHyperionServersColumnValue(HyperionTaskConfiguration taskConfiguration)
        {
            var servers = new List<String>();
            foreach (HyperionServer server in taskConfiguration.HyperionServers)
            {
                servers.Add($"{server.Host}:{server.Port}");
            }
            return string.Join(", ", servers);
        }

        private void btnSaveExit_Click(object sender, EventArgs e)
        {
            LOG.Info("Save button clicked");
            if ( dgTaskConfig.Rows.Count == 0 )
            {
                MessageBox.Show("Screen capture settings cannot be empty. Click \"Add\" button to configure.");
                return;
            }
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            try
            {
                // Check if all settngs are valid
                if ( ValidatorDateTime(tbApiExcludeStart.Text) == false )
                {
                    MessageBox.Show("Error in excluded API start time", "Error in excluded API start time", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if ( ValidatorDateTime(tbApiExcludeEnd.Text) == false )
                {
                    MessageBox.Show("Error in excluded API end time", "Error in excluded API end time", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                LOG.Info("Saving settings using SettingsManager");
                SettingsManager.CaptureOnStartup = chkCaptureOnStartup.Checked;
                SettingsManager.PauseOnUserSwitch = chkPauseUserSwitch.Checked;
                SettingsManager.PauseOnSystemSuspend = chkPauseSuspend.Checked;
                SettingsManager.ApiPort = int.Parse(tbApiPort.Text);
                SettingsManager.ApiEnabled = chkApiEnabled.Checked;
                SettingsManager.ApiExcludedTimesEnabled = chkApiExcludeTimesEnabled.Checked;
                SettingsManager.ApiExcludeTimeStart = DateTime.Parse(tbApiExcludeStart.Text);
                SettingsManager.ApiExcludeTimeEnd = DateTime.Parse(tbApiExcludeEnd.Text);
                SettingsManager.CheckUpdateOnStartup = chkCheckUpdate.Checked;
                SettingsManager.NotificationLevel =
                    (NotificationLevel) Enum.Parse(typeof(NotificationLevel), cbNotificationLevel.Text);
                SettingsManager.HyperionTaskConfigurations = _taskConfigurations;
                SettingsManager.SaveSettings();
                LOG.Info("Saved settings using SettingsManager");
                _mainForm.Init(true);
            }
            catch ( Exception ex )
            {
                LOG.Error("Failed to save settings from the setup form", ex);
                MessageBox.Show($"Error occcured during SaveSettings(): {ex.Message}");
            }
        }

        private static bool ValidatorInt(string input, int minValue, int maxValue, bool validateMaxValue)
        {
            bool isValid = false;
            int value;
            bool isInteger = int.TryParse(input, out value);

            if ( isInteger )
            {
                //Only check minValue
                if ( validateMaxValue == false && value >= minValue )
                {
                    isValid = true;
                }
                //Check both min/max values
                else
                {
                    if ( value >= minValue && value <= maxValue )
                    {
                        isValid = true;
                    }
                }
            }
            return isValid;
        }

        public Boolean ValidatorDateTime(string input)
        {
            DateTime dt;
            Boolean IsValid = false;
            bool isDateTime = DateTime.TryParse(input, out dt);
            if ( isDateTime )
            {
                IsValid = true;
            }

            return IsValid;
        }

        private void tbApiPort_Validating(object sender, CancelEventArgs e)
        {
            const int minValue = 1;
            const int maxValue = 65535;
            if ( ValidatorInt(tbApiPort.Text, minValue, maxValue, false) == false )
            {
                MessageBox.Show(@"Invalid integer filled for port");
                e.Cancel = true;
            }
        }

        private void tbExcludeStart_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ( ValidatorDateTime(tbApiExcludeStart.Text) == false )
            {
                MessageBox.Show("Error in excluded API start time", "Error in excluded API start time", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        private void tbExcludeEnd_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ( ValidatorDateTime(tbApiExcludeEnd.Text) == false )
            {
                MessageBox.Show("Error in excluded API end time", "Error in excluded API end time", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        private void lblShowDx11Displays_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LOG.Info("Checking for available DX11 monitors");
            string msg;
            string title;
            MessageBoxIcon icon;
            try
            {
                msg = DX11ScreenCapture.GetAvailableMonitors();
                title = "Available Monitors";
                icon = MessageBoxIcon.Information;
            }
            catch
            {
                msg = "Failed to list monitors";
                title = "Error";
                icon = MessageBoxIcon.Error;
            }
            LOG.Info($"{title}: {msg}");
            MessageBox.Show(msg, title, MessageBoxButtons.OK, icon);
        }

        private void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            UpdateChecker.StartUpdateCheck(false);
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            Process.Start(MiscUtils.GetLogDirectory());
        }

        private void btnAddTaskConfig_Click(object sender, EventArgs e)
        {
            ServerPropertiesForm editPropFrm = new ServerPropertiesForm(HyperionTaskConfiguration.BuildUsingDefaultSettings());
            editPropFrm.ShowDialog();
            if ( editPropFrm.SaveRequested )
            {
                _taskConfigurations.Add(editPropFrm.TaskConfiguration);
                Rebind_dgTaskConfig();
            }
        }

        private void btnRemoveTaskConfig_Click(object sender, EventArgs e)
        {
            int selectedRowIndex = dgTaskConfig.SelectedRows[0].Index;
            _taskConfigurations.RemoveAt(selectedRowIndex);
            Rebind_dgTaskConfig();
        }

        private void btnEditTaskConfig_Click(object sender, EventArgs e)
        {
            EditCurrentlySelectedTaskConfiguration();
        }

        private void EditCurrentlySelectedTaskConfiguration()
        {
            int selectedRowIndex = dgTaskConfig.SelectedRows[0].Index;
            ServerPropertiesForm editPropFrm = new ServerPropertiesForm(_taskConfigurations[selectedRowIndex].DeepCopy());
            editPropFrm.ShowDialog();
            if ( editPropFrm.SaveRequested )
            {
                _taskConfigurations[selectedRowIndex] = editPropFrm.TaskConfiguration;
                Rebind_dgTaskConfig();
            }
        }

        private void dgTaskConfig_SelectionChanged(object sender, EventArgs e)
        {
            if ( dgTaskConfig.SelectedRows.Count > 0 )
            {
                btnEditTaskConfig.Enabled = true;
                btnRemoveTaskConfig.Enabled = true;
            } else
            {
                btnEditTaskConfig.Enabled = false;
                btnRemoveTaskConfig.Enabled = false;
            }
        }

        private void dgTaskConfig_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if ( dgTaskConfig.SelectedRows.Count > 0 )
                EditCurrentlySelectedTaskConfiguration();
        }

        private void btnDonate_Click(object sender, EventArgs e)
        {
            MiscUtils.ShowDonatePage();
        }
    }
}
