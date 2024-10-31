﻿using DBADashGUI.CustomReports;

namespace DBADashGUI.Drives
{
    partial class DriveHistory
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriveHistory));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            chart1 = new LiveCharts.WinForms.CartesianChart();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsTime = new System.Windows.Forms.ToolStripDropDownButton();
            dayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            daysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            days7ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            days30ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            days90ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            days180ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            yearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsCustom = new System.Windows.Forms.ToolStripMenuItem();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            smoothLinesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tsRefresh = new System.Windows.Forms.ToolStripButton();
            tsChart = new System.Windows.Forms.ToolStripButton();
            tsGrid = new System.Windows.Forms.ToolStripButton();
            tsExcel = new System.Windows.Forms.ToolStripButton();
            tsCopy = new System.Windows.Forms.ToolStripButton();
            lblInsufficientData = new System.Windows.Forms.Label();
            dgv = new DBADashDataGridView();
            tsClearFilter = new System.Windows.Forms.ToolStripButton();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            SuspendLayout();
            // 
            // chart1
            // 
            chart1.BackColor = System.Drawing.Color.White;
            chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            chart1.Location = new System.Drawing.Point(0, 27);
            chart1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            chart1.Name = "chart1";
            chart1.Size = new System.Drawing.Size(1134, 521);
            chart1.TabIndex = 0;
            chart1.Text = "cartesianChart1";
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsTime, toolStripDropDownButton1, tsRefresh, tsChart, tsGrid, tsExcel, tsCopy, tsClearFilter });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1134, 27);
            toolStrip1.TabIndex = 3;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsTime
            // 
            tsTime.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            tsTime.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { dayToolStripMenuItem, daysToolStripMenuItem, days7ToolStripMenuItem, days30ToolStripMenuItem, days90ToolStripMenuItem, days180ToolStripMenuItem, yearToolStripMenuItem, toolStripSeparator1, tsCustom });
            tsTime.Image = Properties.Resources.Time_16x;
            tsTime.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsTime.Name = "tsTime";
            tsTime.Size = new System.Drawing.Size(97, 24);
            tsTime.Text = "{0} Days";
            // 
            // dayToolStripMenuItem
            // 
            dayToolStripMenuItem.Name = "dayToolStripMenuItem";
            dayToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            dayToolStripMenuItem.Tag = "1";
            dayToolStripMenuItem.Text = "1 Day";
            dayToolStripMenuItem.Click += Days_Click;
            // 
            // daysToolStripMenuItem
            // 
            daysToolStripMenuItem.Name = "daysToolStripMenuItem";
            daysToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            daysToolStripMenuItem.Tag = "2";
            daysToolStripMenuItem.Text = "2 Days";
            daysToolStripMenuItem.Click += Days_Click;
            // 
            // days7ToolStripMenuItem
            // 
            days7ToolStripMenuItem.Checked = true;
            days7ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            days7ToolStripMenuItem.Name = "days7ToolStripMenuItem";
            days7ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            days7ToolStripMenuItem.Tag = "7";
            days7ToolStripMenuItem.Text = "7 Days";
            days7ToolStripMenuItem.Click += Days_Click;
            // 
            // days30ToolStripMenuItem
            // 
            days30ToolStripMenuItem.Name = "days30ToolStripMenuItem";
            days30ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            days30ToolStripMenuItem.Tag = "30";
            days30ToolStripMenuItem.Text = "30 Days";
            days30ToolStripMenuItem.Click += Days_Click;
            // 
            // days90ToolStripMenuItem
            // 
            days90ToolStripMenuItem.Name = "days90ToolStripMenuItem";
            days90ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            days90ToolStripMenuItem.Tag = "90";
            days90ToolStripMenuItem.Text = "90 Days";
            days90ToolStripMenuItem.Click += Days_Click;
            // 
            // days180ToolStripMenuItem
            // 
            days180ToolStripMenuItem.Name = "days180ToolStripMenuItem";
            days180ToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            days180ToolStripMenuItem.Tag = "180";
            days180ToolStripMenuItem.Text = "180 Days";
            days180ToolStripMenuItem.Click += Days_Click;
            // 
            // yearToolStripMenuItem
            // 
            yearToolStripMenuItem.Name = "yearToolStripMenuItem";
            yearToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            yearToolStripMenuItem.Tag = "365";
            yearToolStripMenuItem.Text = "1 Year";
            yearToolStripMenuItem.Click += Days_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // tsCustom
            // 
            tsCustom.Name = "tsCustom";
            tsCustom.Size = new System.Drawing.Size(152, 26);
            tsCustom.Tag = "-1";
            tsCustom.Text = "Custom";
            tsCustom.Click += Custom_Click;
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { smoothLinesToolStripMenuItem, pointsToolStripMenuItem });
            toolStripDropDownButton1.Image = Properties.Resources.LineChart_16x;
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(34, 24);
            toolStripDropDownButton1.Text = "Chart Options";
            // 
            // smoothLinesToolStripMenuItem
            // 
            smoothLinesToolStripMenuItem.Checked = true;
            smoothLinesToolStripMenuItem.CheckOnClick = true;
            smoothLinesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            smoothLinesToolStripMenuItem.Name = "smoothLinesToolStripMenuItem";
            smoothLinesToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            smoothLinesToolStripMenuItem.Text = "Smooth Lines";
            smoothLinesToolStripMenuItem.Click += SmoothLinesToolStripMenuItem_Click;
            // 
            // pointsToolStripMenuItem
            // 
            pointsToolStripMenuItem.CheckOnClick = true;
            pointsToolStripMenuItem.Name = "pointsToolStripMenuItem";
            pointsToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            pointsToolStripMenuItem.Text = "Points";
            pointsToolStripMenuItem.Click += PointsToolStripMenuItem_Click;
            // 
            // tsRefresh
            // 
            tsRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsRefresh.Image = Properties.Resources._112_RefreshArrow_Green_16x16_72;
            tsRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsRefresh.Name = "tsRefresh";
            tsRefresh.Size = new System.Drawing.Size(29, 24);
            tsRefresh.Text = "Refresh";
            tsRefresh.Click += TsRefresh_Click;
            // 
            // tsChart
            // 
            tsChart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsChart.Image = (System.Drawing.Image)resources.GetObject("tsChart.Image");
            tsChart.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsChart.Name = "tsChart";
            tsChart.Size = new System.Drawing.Size(29, 24);
            tsChart.Text = "View Chart";
            tsChart.Visible = false;
            tsChart.Click += TsChart_Click;
            // 
            // tsGrid
            // 
            tsGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsGrid.Image = Properties.Resources.Table_16x;
            tsGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsGrid.Name = "tsGrid";
            tsGrid.Size = new System.Drawing.Size(29, 24);
            tsGrid.Text = "View Grid";
            tsGrid.Click += TsGrid_Click;
            // 
            // tsExcel
            // 
            tsExcel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsExcel.Image = Properties.Resources.excel16x16;
            tsExcel.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsExcel.Name = "tsExcel";
            tsExcel.Size = new System.Drawing.Size(29, 24);
            tsExcel.Text = "Export to Excel";
            tsExcel.Click += TsExcel_Click;
            // 
            // tsCopy
            // 
            tsCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsCopy.Image = Properties.Resources.ASX_Copy_blue_16x;
            tsCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsCopy.Name = "tsCopy";
            tsCopy.Size = new System.Drawing.Size(29, 24);
            tsCopy.Text = "Copy Data";
            tsCopy.Click += TsCopy_Click;
            // 
            // lblInsufficientData
            // 
            lblInsufficientData.Dock = System.Windows.Forms.DockStyle.Fill;
            lblInsufficientData.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblInsufficientData.Location = new System.Drawing.Point(0, 27);
            lblInsufficientData.Name = "lblInsufficientData";
            lblInsufficientData.Size = new System.Drawing.Size(1134, 521);
            lblInsufficientData.TabIndex = 4;
            lblInsufficientData.Text = "Please wait for more data to be collected...";
            lblInsufficientData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblInsufficientData.Visible = false;
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(0, 79, 131);
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(255, 255, 255);
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(241, 241, 246);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(0, 79, 131);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(211, 211, 216);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(0, 79, 131);
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgv.DefaultCellStyle = dataGridViewCellStyle2;
            dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.Location = new System.Drawing.Point(0, 27);
            dgv.Name = "dgv";
            dgv.ReadOnly = true;
            dgv.ResultSetID = 0;
            dgv.ResultSetName = null;
            dgv.RowHeadersVisible = false;
            dgv.RowHeadersWidth = 51;
            dgv.Size = new System.Drawing.Size(1134, 521);
            dgv.TabIndex = 5;
            dgv.Visible = false;
            // 
            // tsClearFilter
            // 
            tsClearFilter.Enabled = false;
            tsClearFilter.Image = Properties.Resources.Eraser_16x;
            tsClearFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsClearFilter.Name = "tsClearFilter";
            tsClearFilter.Size = new System.Drawing.Size(104, 24);
            tsClearFilter.Text = "Clear Filter";
            // 
            // DriveHistory
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(dgv);
            Controls.Add(chart1);
            Controls.Add(lblInsufficientData);
            Controls.Add(toolStrip1);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "DriveHistory";
            Size = new System.Drawing.Size(1134, 548);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private LiveCharts.WinForms.CartesianChart chart1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton tsTime;
        private System.Windows.Forms.ToolStripMenuItem days7ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem days30ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem days90ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem days180ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yearToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsCustom;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem smoothLinesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsRefresh;
        private System.Windows.Forms.ToolStripMenuItem pointsToolStripMenuItem;
        private System.Windows.Forms.Label lblInsufficientData;
        private System.Windows.Forms.ToolStripMenuItem dayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem daysToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsChart;
        private System.Windows.Forms.ToolStripButton tsGrid;
        private DBADashDataGridView dgv;
        private System.Windows.Forms.ToolStripButton tsExcel;
        private System.Windows.Forms.ToolStripButton tsCopy;
        private System.Windows.Forms.ToolStripButton tsClearFilter;
    }
}
