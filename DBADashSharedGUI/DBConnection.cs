﻿using System.Diagnostics.Eventing.Reader;
using DBADashGUI.Theme;
using Microsoft.Data.SqlClient;
using System.Runtime.Versioning;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DBADash
{
    [SupportedOSPlatform("windows")]
    public partial class DBConnection : Form
    {
        public DBConnection()
        {
            InitializeComponent();
            this.ApplyTheme();
            PopulateCombos();
        }

        private string connectionString = string.Empty;

        public bool ValidateInitialCatalog = false;

        private SqlAuthenticationMethod SelectedAuthenticationMethod => cboAuthType.SelectedItem is KeyValuePair<SqlAuthenticationMethod, string> selectedPair ? selectedPair.Key : SqlAuthenticationMethod.ActiveDirectoryIntegrated;
        private SqlConnectionEncryptOption SelectedEncryptionOption => cboEncryption.SelectedItem is KeyValuePair<SqlConnectionEncryptOption, string> selectedPair ? selectedPair.Key : SqlConnectionEncryptOption.Mandatory;

        private bool IsPasswordSupported => SelectedAuthenticationMethod is SqlAuthenticationMethod.SqlPassword
            or SqlAuthenticationMethod.ActiveDirectoryPassword
            or SqlAuthenticationMethod.ActiveDirectoryServicePrincipal;

        private bool IsUserNameSupported => SelectedAuthenticationMethod is SqlAuthenticationMethod.SqlPassword
            or SqlAuthenticationMethod.ActiveDirectoryInteractive or SqlAuthenticationMethod.ActiveDirectoryPassword
            or SqlAuthenticationMethod.ActiveDirectoryServicePrincipal or SqlAuthenticationMethod.ActiveDirectoryManagedIdentity;

        private static Dictionary<SqlAuthenticationMethod, string> AuthenticationMethods =>
            new Dictionary<SqlAuthenticationMethod, string>
            {
                { SqlAuthenticationMethod.ActiveDirectoryIntegrated, "Windows Authentication" },
                { SqlAuthenticationMethod.SqlPassword, "SQL Server Authentication" },
                { SqlAuthenticationMethod.ActiveDirectoryInteractive, "Microsoft Entra MFA" },
                { SqlAuthenticationMethod.ActiveDirectoryPassword ,"Microsoft Entra Password"},
                { SqlAuthenticationMethod.ActiveDirectoryServicePrincipal, "Microsoft Entra Service Principal" },
                { SqlAuthenticationMethod.ActiveDirectoryManagedIdentity, "Microsoft Entra Managed Identity"},
                { SqlAuthenticationMethod.ActiveDirectoryDefault, "Microsoft Entra Default" }
            };

        private static Dictionary<SqlConnectionEncryptOption, string> EncryptionOptions => new Dictionary<SqlConnectionEncryptOption, string>
        {
            { SqlConnectionEncryptOption.Mandatory, "Mandatory" },
            { SqlConnectionEncryptOption.Optional, "Optional" },
            { SqlConnectionEncryptOption.Strict, "Strict (SQL Server 2022 & Azure SQL)" }
        };

        private void PopulateCombos()
        {
            cboAuthType.DataSource = new BindingSource(AuthenticationMethods, null);
            cboAuthType.DisplayMember = "Value";
            cboAuthType.ValueMember = "Key";

            cboEncryption.DataSource = new BindingSource(EncryptionOptions, null);
            cboEncryption.DisplayMember = "Value";
            cboEncryption.ValueMember = "Key";
        }

        public string ConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(connectionString)
                {
                    UserID = txtUserName.Text,
                    Password = txtPassword.Text,
                    Authentication = SelectedAuthenticationMethod,
                    DataSource = txtServerName.Text,
                    InitialCatalog = cboDatabase.Text,
                    Encrypt = SelectedEncryptionOption,
                    TrustServerCertificate = chkTrustServerCert.Checked,
                    HostNameInCertificate = txtHostNameInCertificate.Text
                };

                if (!IsPasswordSupported || string.IsNullOrEmpty(txtPassword.Text))
                {
                    builder.Remove("Password");
                    builder.Remove("PWD");
                }
                if (!IsUserNameSupported || string.IsNullOrEmpty(txtUserName.Text))
                {
                    builder.Remove("UID");
                    builder.Remove("UserID");
                }

                builder.Remove("Integrated Security"); // Replaced with Authentication

                return builder.ConnectionString;
            }
            set
            {
                connectionString = value;
                var builder = new SqlConnectionStringBuilder(connectionString);

                if (AuthenticationMethods.ContainsKey(builder.Authentication))
                {
                    cboAuthType.SelectedValue = builder.Authentication;
                }
                else if (builder.Authentication == SqlAuthenticationMethod.NotSpecified)
                {
                    cboAuthType.SelectedValue = builder.IntegratedSecurity ? SqlAuthenticationMethod.ActiveDirectoryIntegrated : SqlAuthenticationMethod.SqlPassword;
                }

                cboDatabase.Text = builder.InitialCatalog;
                txtUserName.Text = builder.UserID;
                txtPassword.Text = builder.Password;
                txtServerName.Text = builder.DataSource;
                chkTrustServerCert.Checked = builder.TrustServerCertificate;
                cboEncryption.SelectedValue = builder.Encrypt;
                txtHostNameInCertificate.Text = builder.HostNameInCertificate;
            }
        }

        public string ConnectionStringWithoutInitialCatalog
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);
                builder.Remove("Initial Catalog");
                return builder.ConnectionString;
            }
        }

        private void BttnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        public static void TestConnection(string connectionString)
        {
            using var cn = new SqlConnection(connectionString);
            cn.Open();
        }

        private void BttnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                TestConnection(ValidateInitialCatalog ? ConnectionString : ConnectionStringWithoutInitialCatalog); // Try without initial catalog as DB might not have been created yet
            }
            catch (SqlException ex) when (ex.Number == -2146893019)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error: Deploy a trusted certificate or use the 'Trust Server Certificate' connection option.\n\n{ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error connecting to data source. \n{ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            DialogResult = DialogResult.OK;
        }

        private void DBConnection_Load(object sender, EventArgs e)
        {
            if (txtServerName.Text == @"localhost")
            {
                txtServerName.Text = Environment.MachineName;
            }
        }

        private void CboDatabase_Dropdown(object sender, EventArgs e)
        {
            try
            {
                cboDatabase.Items.Clear();
                var DBs = GetDatabases(ConnectionStringWithoutInitialCatalog);
                foreach (var db in DBs)
                {
                    cboDatabase.Items.Add(db);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static List<string> GetDatabases(string ConnectionString)
        {
            using var cn = new SqlConnection(ConnectionString);
            using SqlCommand cmd = new("SELECT name FROM sys.databases WHERE state=0", cn);
            cn.Open();
            var DBs = new List<string>();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                DBs.Add((string)rdr[0]);
            }
            DBs.Sort();
            return DBs;
        }

        private void cboAuthType_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPassword.Visible = IsPasswordSupported;
            txtUserName.Visible = IsUserNameSupported;
            lblPassword.Visible = IsPasswordSupported;
            lblUserName.Visible = IsUserNameSupported;
            lblUserName.Text = SelectedAuthenticationMethod == SqlAuthenticationMethod.ActiveDirectoryManagedIdentity ? "User assigned identity:" : "User name:";
        }

        private void cboEncryption_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkTrustServerCert.Enabled = !Equals(cboEncryption.SelectedValue, SqlConnectionEncryptOption.Strict);
            chkTrustServerCert.Checked = !Equals(cboEncryption.SelectedValue, SqlConnectionEncryptOption.Strict) && chkTrustServerCert.Checked;
        }
    }
}