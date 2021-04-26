﻿using DBADash;
using Newtonsoft.Json;
using Quartz;
using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using static DBADash.DBADashConnection;
using System.Drawing;

namespace DBADashServiceConfig
{
    public partial class ServiceConfig : Form
    {
        public ServiceConfig()
        {
            InitializeComponent();
        }

        string originalJson = "";
        CollectionConfig collectionConfig = new CollectionConfig();
        readonly string jsonPath = System.IO.Path.Combine(Application.StartupPath, "ServiceConfig.json");
        ServiceController svcCtrl;

        private void bttnAdd_Click(object sender, EventArgs e)
        {
            var src = new DBADashSource(cboSource.Text)
            {
                NoWMI = chkNoWMI.Checked
            };
            if (chkSlowQueryThreshold.Checked)
            {
                src.SlowQueryThresholdMs = (Int32)numSlowQueryThreshold.Value;
            }
            if (chkCustomizeSchedule.Checked)
            {
                src.Schedules = src.GetSchedule();
            }
            src.UseDualEventSession = chkDualSession.Checked;
            src.SchemaSnapshotOnServiceStart = chkSchemaSnapshotOnStart.Checked;
            if (txtSnapshotDBs.Text.Trim().Length > 0)
            {
                if (!CronExpression.IsValidExpression(txtSnapshotCron.Text))
                {
                    MessageBox.Show("Invalid cron expression", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                src.SchemaSnapshotDBs = txtSnapshotDBs.Text;
                src.SchemaSnapshotCron = txtSnapshotCron.Text;
                if (collectionConfig.SchemaSnapshotOptions == null)
                {
                    collectionConfig.SchemaSnapshotOptions=  new SchemaSnapshotDBOptions();
                }
            }
            src.PersistXESessions = chkPersistXESession.Checked;
            bool validated = validateSource();

            if (validated)
            {
                if (!(src.SourceConnection.Type == ConnectionType.SQL || collectionConfig.DestinationConnection.Type == ConnectionType.SQL))
                {
                    MessageBox.Show("Error: Invalid source and destination connection combination.  One of these should be a SQL connection string", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (collectionConfig == null)
                {
                    collectionConfig = new CollectionConfig();
                }
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                validated = src.SourceConnection.Validate();
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                if (validated == false)
                {
                    if (MessageBox.Show("Error connecting to data source.  Are you sure you want to add this to the configuration?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    {
                        return;
                    }
                }
                else if(!src.SourceConnection.IsXESupported() && src.SlowQueryThresholdMs >= 0)
                {
                    MessageBox.Show("Warning: Slow query capture is supported for SQL 2012 and later and is not available for this SQL instance", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    src.SlowQueryThresholdMs = -1;
                    src.PersistXESessions = false;
                }

                var existingConnection = collectionConfig.GetSourceFromConnectionString(cboSource.Text);
                if (existingConnection != null)
                {
                    if (chkCustomizeSchedule.Checked && existingConnection.Schedules != null)
                    {
                        src.Schedules = existingConnection.Schedules;
                    }
                    if (MessageBox.Show("Update existing connection?", "Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        collectionConfig.SourceConnections.Remove(existingConnection);
                    }
                    else
                    {
                        return;
                    }

                }

                collectionConfig.SourceConnections.Add(src);
                txtJson.Text = collectionConfig.Serialize();
                populateDropDowns();
            }
        }


        private void populateDropDowns()
        {
            foreach (var _cfg in collectionConfig.SourceConnections)
            {
                if (!(cboSource.Items.Contains(_cfg.ConnectionString)))
                {
                    cboSource.Items.Add(_cfg.ConnectionString);
                }
            }
        }

        private bool validateSource()
        {
            errorProvider1.SetError(cboSource, null);
            DBADashConnection source = new DBADashConnection(cboSource.Text);
            if (cboSource.Text == "")
            {
                return false;
            }

            if (source.Type == ConnectionType.Invalid)
            {
                errorProvider1.SetError(cboSource, "Invalid connection string, directory or S3 path");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool validateDestination()
        {
            errorProvider1.SetError(txtDestination, null);
            DBADashConnection dest = new DBADashConnection(txtDestination.Text);
            lblVersionInfo.ForeColor = Color.Black;
            lblVersionInfo.Text = "";
            if (txtDestination.Text == "")
            {
                return false;
            }

            if (dest.Type == ConnectionType.Invalid)
            {
                errorProvider1.SetError(txtDestination, "Invalid connection string, directory or S3 path");
                return false;
            }
            if(dest.Type == ConnectionType.SQL)
            {
                try
                {
                    var status = DBValidations.VersionStatus(dest.ConnectionString);
                    if (status.VersionStatus == DBValidations.DBVersionStatusEnum.CreateDB)
                    {
                        lblVersionInfo.Text = "Run Deploy to create database.";
                        lblVersionInfo.ForeColor = Color.Red;
                        return true;
                    }
                    if (status.VersionStatus == DBValidations.DBVersionStatusEnum.OK)
                    {
                        lblVersionInfo.Text = "DB upgrade not required. DacVersion/DB Version: " + status.DACVersion.ToString();
                        lblVersionInfo.ForeColor = Color.Green;
                        bttnDeployDatabase.Enabled = true;
                    }
                    else if (status.VersionStatus == DBValidations.DBVersionStatusEnum.AppUpgradeRequired)
                    {
                        lblVersionInfo.Text = "DB version " + status.DBVersion.ToString() + " is newer. Please update the app";
                        lblVersionInfo.ForeColor = Color.Red;
                        bttnDeployDatabase.Enabled = false;
                    }
                    else
                    {
                        lblVersionInfo.Text = "DB version " + status.DBVersion.ToString() + " requires upgrade to " + status.DACVersion.ToString();
                        lblVersionInfo.ForeColor = Color.Red;
                        bttnDeployDatabase.Enabled = true;
                    }
                    return true;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }


        private void bttnSave_Click(object sender, EventArgs e)
        {
            saveChanges();
        }

        private void saveChanges()
        {
            txtJson.Text = collectionConfig.Serialize();
            System.IO.File.WriteAllText(jsonPath, txtJson.Text);
            originalJson = txtJson.Text;
            MessageBox.Show("Config saved.  Restart service to apply changes.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ServiceConfig_Load(object sender, EventArgs e)
        {
            txtJson.MaxLength = 0;
            cboServiceCredentials.SelectedIndex = 3;
            if (File.Exists(jsonPath))
            {
                try
                {
                    originalJson = System.IO.File.ReadAllText(jsonPath);
                    txtJson.Text = originalJson;
                    setFromJson(originalJson);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error reading ServiceConfig.json: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            refreshServiceStatus();
            validateDestination();
        }

        private void setFromJson(string json)
        {
            collectionConfig = CollectionConfig.Deserialize(json);
            populateDropDowns();
            txtDestination.Text = collectionConfig.DestinationConnection.EncryptedConnectionString;
            txtAWSProfile.Text = collectionConfig.AWSProfile;
            txtAccessKey.Text = collectionConfig.AccessKey;
            txtSecretKey.Text = collectionConfig.SecretKey;
            chkScanAzureDB.Checked = collectionConfig.ScanForAzureDBs;
            chkScanEvery.Checked = collectionConfig.ScanForAzureDBsInterval > 0;
            numAzureScanInterval.Value = collectionConfig.ScanForAzureDBsInterval;
            chkCustomizeMaintenanceCron.Checked = (collectionConfig.MaintenanceScheduleCron != null);
            chkAutoUpgradeRepoDB.Checked = collectionConfig.AutoUpdateDatabase;
            updateScanInterval();

        }

        private void refreshServiceStatus()
        {
            svcCtrl = ServiceController.GetServices()
    .FirstOrDefault(s => s.ServiceName == collectionConfig.ServiceName);

            if (svcCtrl == null)
            {
                lblServiceStatus.Text = "Service Status: Not Installed";
                lblServiceStatus.ForeColor = Color.Red;
                bttnStart.Enabled = false;
                bttnStop.Enabled = false;
                bttnInstall.Enabled = true;
                bttnUninstall.Enabled = false;

            }
            else
            {
                lblServiceStatus.Text = Enum.GetName(typeof(ServiceControllerStatus), svcCtrl.Status);
                if(svcCtrl.Status == ServiceControllerStatus.Running)
                {
                    lblServiceStatus.ForeColor = Color.Green;
                }
                else if(svcCtrl.Status== ServiceControllerStatus.Stopped || svcCtrl.Status == ServiceControllerStatus.StopPending || svcCtrl.Status == ServiceControllerStatus.Paused)
                {
                    lblServiceStatus.ForeColor = Color.Red;
                }
                else
                {
                    lblServiceStatus.ForeColor = Color.Orange;
                }
                bttnStart.Enabled = (svcCtrl.Status == ServiceControllerStatus.Stopped);
                bttnStop.Enabled = (svcCtrl.Status == ServiceControllerStatus.Running);
                bttnInstall.Enabled = false;
                bttnUninstall.Enabled = true;
            }
        }

        private void txtJson_Validating(object sender, CancelEventArgs e)
        {
            errorProvider1.SetError(txtJson, null);
            if (txtJson.Text.Trim() == "")
            {
                collectionConfig = new CollectionConfig();
                return;
            }
            try
            {
                setFromJson(txtJson.Text);
            }
            catch (Exception ex)
            {
                errorProvider1.SetError(txtJson, ex.Message);
            }
        }

        private void ServiceConfig_FromClosing(object sender, FormClosingEventArgs e)
        {
            promptSaveChanges();
        }

        private void promptSaveChanges()
        {
            if (originalJson != txtJson.Text)
            {
                if (MessageBox.Show("Save Changes?", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    saveChanges();
                }
            }
        }

        private void bttnStart_Click(object sender, EventArgs e)
        {
            promptSaveChanges();
            svcCtrl.Refresh();
            if (svcCtrl.Status == ServiceControllerStatus.Stopped)
            {
                try
                {
                    svcCtrl.Start();
                    System.Threading.Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            refreshServiceStatus();
        }

        private void bttnStop_Click(object sender, EventArgs e)
        {
            svcCtrl.Refresh();
            if (svcCtrl.Status == ServiceControllerStatus.Running)
            {
                try
                {
                    svcCtrl.Stop();
                    System.Threading.Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            refreshServiceStatus();
        }

        private void bttnRefresh_Click(object sender, EventArgs e)
        {
            refreshServiceStatus();
        }

        private void bttnInstall_Click(object sender, EventArgs e)
        {
            promptSaveChanges();
            if (!(File.Exists(jsonPath)))
            {
                MessageBox.Show("Save configuration file before installing service", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            Process p = new Process();
            var psi = new ProcessStartInfo()
            {
                FileName = "CMD.EXE"
            };
            string arg = "";
            switch (cboServiceCredentials.SelectedIndex)
            {
                case 0:
                    arg = "--localsystem";
                    break;
                case 1:
                    arg = "--localservice";
                    break;
                case 2:
                    arg = "--networkservice";
                    break;
                case 3:
                    arg = "--interactive";
                    break;

            }
            psi.Arguments = "/K DBADashService Install " + arg;
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            System.Threading.Thread.Sleep(500);
            refreshServiceStatus();
        }

        private void bttnUninstall_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = "CMD.EXE",
                Arguments = "/K DBADashService UnInstall"
            };
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
            System.Threading.Thread.Sleep(500);
            refreshServiceStatus();
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtAWSProfile_TextChanged(object sender, EventArgs e)
        {
            txtAccessKey.Enabled = (txtAWSProfile.Text.Length == 0);
            txtSecretKey.Enabled = txtAccessKey.Enabled;

        }

        private void txtAccessKey_TextChanged(object sender, EventArgs e)
        {
            txtAWSProfile.Enabled = (txtAccessKey.Text.Length == 0 && txtSecretKey.Text.Length == 0);

        }

        private void txtSecretKey_TextChanged(object sender, EventArgs e)
        {
            txtAWSProfile.Enabled = (txtAccessKey.Text.Length == 0 && txtSecretKey.Text.Length == 0);

        }


        private void txtDestination_Validated(object sender, EventArgs e)
        {
            destinationChanged();
        }

        private void destinationChanged()
        {
            if(collectionConfig.Destination!= txtDestination.Text)
            {
                validateDestination();

                collectionConfig.Destination = txtDestination.Text;
                txtJson.Text = collectionConfig.Serialize();
            }

        }


        private void bttnRemove_Click(object sender, EventArgs e)
        {
            DBADashSource src;

            src = collectionConfig.GetSourceFromConnectionString(cboSource.Text);
            if (src != null)
            {
                collectionConfig.SourceConnections.Remove(src);
                MessageBox.Show("Connection removed", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Connection not found", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            txtJson.Text = collectionConfig.Serialize();
        }

        private void txtAccessKey_Validating(object sender, CancelEventArgs e)
        {
            collectionConfig.AccessKey = (txtAccessKey.Text == "" ? null : txtAccessKey.Text);
            txtJson.Text = collectionConfig.Serialize();
        }

        private void txtSecretKey_Validating(object sender, CancelEventArgs e)
        {
            collectionConfig.SecretKey = (txtSecretKey.Text == "" ? null : txtSecretKey.Text);
            txtJson.Text = collectionConfig.Serialize();
        }

        private void txtAWSProfile_Validating(object sender, CancelEventArgs e)
        {
            collectionConfig.AWSProfile = (txtAWSProfile.Text == "" ? null : txtAWSProfile.Text);
            txtJson.Text = collectionConfig.Serialize();
        }

        private void chkCustomizeMaintenanceCron_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCustomizeMaintenanceCron.Checked)
            {
                collectionConfig.MaintenanceScheduleCron = collectionConfig.GetMaintenanceCron();
            }
            else
            {
                collectionConfig.MaintenanceScheduleCron = null;
            }
            txtJson.Text = collectionConfig.Serialize();
        }

        private void chkSlowQueryThreshold_CheckedChanged(object sender, EventArgs e)
        {
            numSlowQueryThreshold.Enabled = chkSlowQueryThreshold.Checked;
            if (chkSlowQueryThreshold.Checked)
            {
                numSlowQueryThreshold.Value = 1000;
                lblSlow.Text = "Extended events trace to capture slow rpc and batch completed events IS enabled";
            }
            else
            {
                numSlowQueryThreshold.Value = -1;
                lblSlow.Text = "Extended events trace to capture slow rpc and batch completed events is NOT enabled";
            }
            chkDualSession.Enabled= chkSlowQueryThreshold.Checked;
            chkPersistXESession.Enabled = chkSlowQueryThreshold.Checked;

        }

        private void cboSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            DBADashSource src;

            src = collectionConfig.GetSourceFromConnectionString(cboSource.Text);
            if (src != null)
            {
                chkNoWMI.Checked = src.NoWMI;
                chkPersistXESession.Checked = src.PersistXESessions;
                chkSlowQueryThreshold.Checked = (src.SlowQueryThresholdMs != -1);
                if (chkSlowQueryThreshold.Checked)
                {
                    numSlowQueryThreshold.Value = src.SlowQueryThresholdMs;
                }
                else
                {
                    numSlowQueryThreshold.Value = 0;
                }

                chkCustomizeSchedule.Checked = src.Schedules != null;
                txtSnapshotCron.Text = src.SchemaSnapshotCron;
                txtSnapshotDBs.Text = src.SchemaSnapshotDBs;
                chkSchemaSnapshotOnStart.Checked = src.SchemaSnapshotOnServiceStart;
                chkDualSession.Checked = src.UseDualEventSession;
            }
        }

        private void bttnDeployDatabase_Click(object sender, EventArgs e)
        {
            var frm = new DBDeploy();
            var cn =  new DBADashConnection(txtDestination.Text);
            if (cn.Type == ConnectionType.SQL)
            {
                frm.ConnectionString = cn.ConnectionString;
            }
            else
            {
                if (setDestination())
                {
                    cn = new DBADashConnection(txtDestination.Text);
                    frm.ConnectionString = cn.ConnectionString;
                }
                else
                {
                    return;
                }

            }

            frm.ShowDialog();
            if (frm.DatabaseName != cn.InitialCatalog())
            {
                if(MessageBox.Show("Update connection string?","Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var builder = new SqlConnectionStringBuilder(txtDestination.Text)
                    {
                        InitialCatalog = frm.DatabaseName
                    };
                    txtDestination.Text = builder.ConnectionString;
                    destinationChanged();
                }
            }
            validateDestination();

        }

        private bool setDestination()
        {
            var frm = new DBConnection();
            var cn = new DBADashConnection(txtDestination.Text);
            if (cn.Type == ConnectionType.SQL)
            {
                frm.ConnectionString = cn.ConnectionString;
            }
            else
            {
                frm.ConnectionString = "Initial Catalog=DBADashDB;Integrated Security=SSPI;Data Source=" + Environment.MachineName;
            }
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
            {

                var builder = new SqlConnectionStringBuilder(frm.ConnectionString);
                if(builder.InitialCatalog==null || builder.InitialCatalog.Length == 0)
                {
                    builder.InitialCatalog = "DBADashDB";
                }
                cn = new DBADashConnection(builder.ConnectionString) ;

                txtDestination.Text = cn.EncryptedConnectionString;
                destinationChanged();
                return true;
            }
            return false;
        }

        private void bttnConnect_Click(object sender, EventArgs e)
        {
            setDestination();
        }

        private void bttnConnectSource_Click(object sender, EventArgs e)
        {
            var frm = new DBConnection();
            var cn = new DBADashConnection(cboSource.Text);
            if (cn.Type == ConnectionType.SQL)
            {
                frm.ConnectionString = cn.ConnectionString;
            }
            frm.ValidateInitialCatalog = true;
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
            {
                cn = new DBADashConnection(frm.ConnectionString);
                cboSource.Text = cn.EncryptedConnectionString;
            }
        }

        private void bttnScanNow_Click(object sender, EventArgs e)
        {
           var newConnections= collectionConfig.GetNewAzureDBConnections();
            if (newConnections.Count == 0)
            {
                MessageBox.Show("No new Azure DB connections found", "Scan",  MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if( MessageBox.Show(String.Format("Found {0} new connections.  Add connections to config file?",newConnections.Count), "Scan", MessageBoxButtons.YesNo, MessageBoxIcon.Question)== DialogResult.Yes)
                {
                    collectionConfig.AddConnections(newConnections);
                    txtJson.Text = collectionConfig.Serialize();
                }
            }
        }

        private void chkScanAzureDB_CheckedChanged(object sender, EventArgs e)
        {
            collectionConfig.ScanForAzureDBs = chkScanAzureDB.Checked;
            txtJson.Text = collectionConfig.Serialize();
        }

        private void chkAutoUpgradeRepoDB_CheckedChanged(object sender, EventArgs e)
        {
            collectionConfig.AutoUpdateDatabase = chkAutoUpgradeRepoDB.Checked;
            txtJson.Text = collectionConfig.Serialize();
        }

        private void chkScanEvery_CheckedChanged(object sender, EventArgs e)
        {
            if(numAzureScanInterval.Value==0 && chkScanEvery.Checked)
            {
                numAzureScanInterval.Value = 3600;
            }
            if (!chkScanEvery.Checked)
            {
                numAzureScanInterval.Value = 0;
            }
            collectionConfig.ScanForAzureDBsInterval = Convert.ToInt32(numAzureScanInterval.Value);
            updateScanInterval();
            txtJson.Text = collectionConfig.Serialize();
        }

        private void updateScanInterval()
        {
            lblHHmm.Visible = chkScanEvery.Checked;
            lblHHmm.Text = TimeSpan.FromSeconds(Convert.ToInt32(numAzureScanInterval.Value)).ToString();
        }

        private void numAzureScanInterval_ValueChanged(object sender, EventArgs e)
        {
            chkScanEvery.Checked = numAzureScanInterval.Value > 0;
            collectionConfig.ScanForAzureDBsInterval = Convert.ToInt32(numAzureScanInterval.Value);
            updateScanInterval();
            txtJson.Text = collectionConfig.Serialize();
        }

        private void lnkCronBuilder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.cronmaker.com/");
        }

        private void bttnDestFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtDestination.Text = fbd.SelectedPath;
                    destinationChanged();
                }
            }
        }

        private void bttnSrcFolder_Click_1(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    cboSource.Text  = fbd.SelectedPath;
                }
            }
        }

        private void bttnS3_Click(object sender, EventArgs e)
        {
            var cfg = new CollectionConfig
            {
                AccessKey = txtAccessKey.Text,
                SecretKey = txtSecretKey.Text,
                AWSProfile = txtAWSProfile.Text
            };

            using (var frm = new S3Browser() { AccessKey = cfg.AccessKey, SecretKey = cfg.GetSecretKey(), Folder= "DBADash_" + Environment.MachineName })
            {                
                frm.ShowDialog();
                if(frm.DialogResult== DialogResult.OK)
                {
                    txtDestination.Text = frm.AWSURL;
                    destinationChanged();
                }
            }
        }

        private void bttnS3Src_Click(object sender, EventArgs e)
        {
            var cfg = new CollectionConfig
            {
                AccessKey = txtAccessKey.Text,
                SecretKey = txtSecretKey.Text,
                AWSProfile = txtAWSProfile.Text
            };
            using (var frm = new S3Browser() { AccessKey = cfg.AccessKey, SecretKey = cfg.GetSecretKey(), Folder = "DBADash_{HostName}" })
            {
                frm.ShowDialog();
                if (frm.DialogResult == DialogResult.OK)
                {
                   cboSource.Text= frm.AWSURL;
                }
            }
        }
    }
}
