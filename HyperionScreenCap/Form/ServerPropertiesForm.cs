using HyperionScreenCap.Model;
using log4net;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace HyperionScreenCap
{
    public partial class ServerPropertiesForm : Form
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ServerPropertiesForm));

        public HyperionTaskConfiguration TaskConfiguration { get; private set; }
        public bool SaveRequested { get; private set; }
        private HyperionServer _defaultServerConfiguration;

        public ServerPropertiesForm(HyperionTaskConfiguration taskConfiguration)
        {
            this._defaultServerConfiguration = HyperionServer.BuildUsingDefaultFbsSettings();
            this.TaskConfiguration = taskConfiguration;
            InitializeComponent();
            this.Text = $"{this.Text} - {taskConfiguration.Id}";
            var protocolColumn = (DataGridViewComboBoxColumn) this.dgHyperionAddress.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
            protocolColumn.DataSource = Enum.GetValues(typeof(HyperionServerProtocol));
            protocolColumn.ValueType = typeof(HyperionServerProtocol);
            InitFormFields();
        }

        private void InitFormFields()
        {
            EnableRelevantDxFields(TaskConfiguration.CaptureMethod);

            SelectValueFromComboBox(cbDx11AdapterIndex, TaskConfiguration.Dx11AdapterIndex); // TODO check item list for each combo box
            SelectValueFromComboBox(cbDx11MonitorIndex, TaskConfiguration.Dx11MonitorIndex);
            tbDx11FrameCaptureTimeout.Text = TaskConfiguration.Dx11FrameCaptureTimeout.ToString();
            SelectValueFromComboBox(cbDx11ImageScalingFactor, TaskConfiguration.Dx11ImageScalingFactor);
            tbDx11MaxFps.Text = TaskConfiguration.Dx11MaxFps.ToString();
            SelectValueFromComboBox(cbDx9MonitorIndex, TaskConfiguration.Dx9MonitorIndex);
            tbDx9CaptureWidth.Text = TaskConfiguration.Dx9CaptureWidth.ToString();
            tbDx9CaptureHeight.Text = TaskConfiguration.Dx9CaptureHeight.ToString();
            tbDx9CaptureInterval.Text = TaskConfiguration.Dx9CaptureInterval.ToString();
            SelectValueFromComboBox(cbDx11DualScreenAdapterIndex, TaskConfiguration.Dx11DualScreenAdapterIndex);
            SelectValueFromComboBox(cbDx11DualScreenMonitorIndex1, TaskConfiguration.Dx11DualScreenMonitorIndex1);
            SelectValueFromComboBox(cbDx11DualScreenMonitorIndex2, TaskConfiguration.Dx11DualScreenMonitorIndex2);
            tbDx11DualScreenFrameCaptureTimeout.Text = TaskConfiguration.Dx11DualScreenFrameCaptureTimeout.ToString();
            SelectValueFromComboBox(cbDx11DualScreenImageScalingFactor, TaskConfiguration.Dx11DualScreenImageScalingFactor);
            tbDx11DualScreenMaxFps.Text = TaskConfiguration.Dx11DualScreenMaxFps.ToString();

            var hyperionServersBindingList = new BindingList<HyperionServer>(TaskConfiguration.HyperionServers);
            var hyperionServersDataSource = new BindingSource(hyperionServersBindingList, null);
            dgHyperionAddress.DataSource = hyperionServersDataSource;
        }

        private void SaveFormFields()
        {
            if (rbcmDx11.Checked)
            {
                TaskConfiguration.CaptureMethod = CaptureMethod.DX11;
            }
            else if(rbcmDx11DualScreen.Checked)
            {
                TaskConfiguration.CaptureMethod = CaptureMethod.DX11DualScreen;
            }
            else
            {
                TaskConfiguration.CaptureMethod = CaptureMethod.DX9;
            }
            TaskConfiguration.Dx11DualScreenAdapterIndex = int.Parse(cbDx11DualScreenAdapterIndex.SelectedItem.ToString());
            TaskConfiguration.Dx11DualScreenMonitorIndex1 = int.Parse(cbDx11DualScreenMonitorIndex1.SelectedItem.ToString());
            TaskConfiguration.Dx11DualScreenMonitorIndex2 = int.Parse(cbDx11DualScreenMonitorIndex2.SelectedItem.ToString());
            TaskConfiguration.Dx11DualScreenFrameCaptureTimeout = int.Parse(tbDx11DualScreenFrameCaptureTimeout.Text);
            TaskConfiguration.Dx11DualScreenImageScalingFactor = int.Parse(cbDx11DualScreenImageScalingFactor.SelectedItem.ToString());
            TaskConfiguration.Dx11DualScreenMaxFps = int.Parse(tbDx11DualScreenMaxFps.Text);
            TaskConfiguration.Dx11AdapterIndex = int.Parse(cbDx11AdapterIndex.SelectedItem.ToString());
            TaskConfiguration.Dx11MonitorIndex = int.Parse(cbDx11MonitorIndex.SelectedItem.ToString());
            TaskConfiguration.Dx11FrameCaptureTimeout = int.Parse(tbDx11FrameCaptureTimeout.Text);
            TaskConfiguration.Dx11ImageScalingFactor = int.Parse(cbDx11ImageScalingFactor.SelectedItem.ToString());
            TaskConfiguration.Dx11MaxFps = int.Parse(tbDx11MaxFps.Text);
            TaskConfiguration.Dx9MonitorIndex = int.Parse(cbDx9MonitorIndex.SelectedItem.ToString());
            TaskConfiguration.Dx9CaptureWidth = int.Parse(tbDx9CaptureWidth.Text);
            TaskConfiguration.Dx9CaptureHeight = int.Parse(tbDx9CaptureHeight.Text);
            TaskConfiguration.Dx9CaptureInterval = int.Parse(tbDx9CaptureInterval.Text);
        }

        private void EnableRelevantDxFields(CaptureMethod captureMethod)
        {
            switch(captureMethod)
            {
                case CaptureMethod.DX11:
                    rbcmDx11.Checked = true;
                    cbDx9MonitorIndex.Enabled = false;
                    tbDx9CaptureWidth.Enabled = false;
                    tbDx9CaptureHeight.Enabled = false;
                    tbDx9CaptureInterval.Enabled = false;
                    cbDx11DualScreenAdapterIndex.Enabled = false;
                    cbDx11DualScreenMonitorIndex1.Enabled = false;
                    cbDx11DualScreenMonitorIndex2.Enabled = false;
                    tbDx11DualScreenFrameCaptureTimeout.Enabled = false;
                    cbDx11DualScreenImageScalingFactor.Enabled = false;
                    tbDx11DualScreenMaxFps.Enabled = false;
                    cbDx11AdapterIndex.Enabled = true;
                    cbDx11MonitorIndex.Enabled = true;
                    tbDx11FrameCaptureTimeout.Enabled = true;
                    cbDx11ImageScalingFactor.Enabled = true;
                    tbDx11MaxFps.Enabled = true;
                    break;

                case CaptureMethod.DX9:
                    rbcmDx9.Checked = true;
                    cbDx11AdapterIndex.Enabled = false;
                    cbDx11MonitorIndex.Enabled = false;
                    tbDx11FrameCaptureTimeout.Enabled = false;
                    cbDx11ImageScalingFactor.Enabled = false;
                    tbDx11MaxFps.Enabled = false;
                    cbDx11DualScreenAdapterIndex.Enabled = false;
                    cbDx11DualScreenMonitorIndex1.Enabled = false;
                    cbDx11DualScreenMonitorIndex2.Enabled = false;
                    tbDx11DualScreenFrameCaptureTimeout.Enabled = false;
                    cbDx11DualScreenImageScalingFactor.Enabled = false;
                    tbDx11DualScreenMaxFps.Enabled = false;
                    cbDx9MonitorIndex.Enabled = true;
                    tbDx9CaptureWidth.Enabled = true;
                    tbDx9CaptureHeight.Enabled = true;
                    tbDx9CaptureInterval.Enabled = true;
                    break;

                case CaptureMethod.DX11DualScreen:
                    rbcmDx11DualScreen.Checked = true;
                    cbDx11AdapterIndex.Enabled = false;
                    cbDx11MonitorIndex.Enabled = false;
                    tbDx11FrameCaptureTimeout.Enabled = false;
                    cbDx11ImageScalingFactor.Enabled = false;
                    tbDx11MaxFps.Enabled = false;
                    cbDx11DualScreenAdapterIndex.Enabled = true;
                    cbDx11DualScreenMonitorIndex1.Enabled = true;
                    cbDx11DualScreenMonitorIndex2.Enabled = true;
                    tbDx11DualScreenFrameCaptureTimeout.Enabled = true;
                    cbDx11DualScreenImageScalingFactor.Enabled = true;
                    tbDx11DualScreenMaxFps.Enabled = true;
                    cbDx9MonitorIndex.Enabled = false;
                    tbDx9CaptureWidth.Enabled = false;
                    tbDx9CaptureHeight.Enabled = false;
                    tbDx9CaptureInterval.Enabled = false;
                    break;

                default:
                    throw new NotImplementedException($"The capture method {captureMethod} is not supported");
            }
        }

        private static void SelectValueFromComboBox(ComboBox comboBox, Object value)
        {
            var valueAsString = value.ToString();
            int indexToSelect = 0;
            foreach ( object obj in comboBox.Items )
            {
                if ( obj.Equals(valueAsString) )
                {
                    comboBox.SelectedIndex = indexToSelect;
                    return;
                }
                indexToSelect++;
            }
            LOG.Error($"Unable to select value {value} from comboBox {comboBox.Name}");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool validServerFound = false;
            bool invalidPriority = false;
            // Validate server rows using IP address default value
            for ( int i = 0; i < TaskConfiguration.HyperionServers.Count; i++ )
            {
                HyperionServer server = TaskConfiguration.HyperionServers[i];
                if (server.Priority < HyperionServer.MIN_PRIORITY || server.Priority > HyperionServer.MAX_PRIORITY)
                {
                    invalidPriority = true;
                }
                if ( !_defaultServerConfiguration.Host.Equals(server.Host) )
                {
                    validServerFound = true;
                    break;
                }
            }
            // Check if all rows are invalid
            if ( !validServerFound )
            {
                MessageBox.Show("All Hyperion server host names are invalid. Please sepcify a valid Hyperion server configuraion.");
                return;
            }
            if (invalidPriority)
            {
                MessageBox.Show("Invalid priority value found. Priority should be set within the range 100-199.");
                return;
            }

            TaskConfiguration.HyperionServers.RemoveAll(server => _defaultServerConfiguration.Host.Equals(server.Host));
            SaveRequested = true;
            Close();
        }

        public new void Close()
        {
            SaveFormFields();
            base.Close();
        }

        private void ServerPropertiesForm_Shown(object sender, EventArgs e)
        {
            SaveRequested = false;
        }

        private void dgHyperionAddress_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(PreventNonNumeric_KeyPressEventHandler);
            if ( dgHyperionAddress.CurrentCell.ColumnIndex == dgHyperionAddress.Columns["clmnPort"].Index
                || dgHyperionAddress.CurrentCell.ColumnIndex == dgHyperionAddress.Columns["clmnPriority"].Index
                || dgHyperionAddress.CurrentCell.ColumnIndex == dgHyperionAddress.Columns["clmnMessageDuration"].Index)
            {
                e.Control.KeyPress += new KeyPressEventHandler(PreventNonNumeric_KeyPressEventHandler);
            }
        }

        private void dgHyperionAddress_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgHyperionAddress.Columns["clmnProtocol"].Index)
            {
                var columnValue = dgHyperionAddress.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                if (columnValue != null)
                {
                    var serverProtocol = (HyperionServerProtocol) columnValue;
                    int newPortValue = -1;
                    switch (serverProtocol)
                    {
                        case HyperionServerProtocol.FLAT_BUFFERS:
                            newPortValue = HyperionServer.BuildUsingDefaultFbsSettings().Port;
                            break;

                        case HyperionServerProtocol.PROTOCOL_BUFFERS:
                            newPortValue = HyperionServer.BuildUsingDefaultProtoSettings().Port;
                            break;

                        default:
                            throw new NotImplementedException($"Hyperion server protocol {serverProtocol} is not supported yet");
                    }
                    dgHyperionAddress.Rows[e.RowIndex].Cells["clmnPort"].Value = newPortValue; // Change port according to protocol
                }
            }
        }

        private void PreventNonNumeric_KeyPressEventHandler(object sender, KeyPressEventArgs e)
        {
            if ( !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) )
            {
                e.Handled = true;
            }
        }

        private void dgHyperionAddress_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[0].Value = _defaultServerConfiguration.Protocol;
            e.Row.Cells[1].Value = _defaultServerConfiguration.Host;
            e.Row.Cells[2].Value = _defaultServerConfiguration.Port;
            e.Row.Cells[3].Value = _defaultServerConfiguration.Priority;
            e.Row.Cells[4].Value = _defaultServerConfiguration.MessageDuration;
        }

        private void rbcmDx11_CheckedChanged(object sender, EventArgs e)
        {
            if ( rbcmDx11.Checked )
                EnableRelevantDxFields(CaptureMethod.DX11);
            else if (rbcmDx11DualScreen.Checked)
                EnableRelevantDxFields(CaptureMethod.DX11DualScreen);
            else
                EnableRelevantDxFields(CaptureMethod.DX9);
        }

        private void tblScreenCaptureMethod_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
